using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace PizzaApp.UITests.Infrastructure;

public abstract class SeleniumTestBase : IDisposable
{
    private readonly WebDriverWait _wait;

    protected SeleniumTestBase(PizzaAppFixture app)
    {
        App = app;

        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1440,1200");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.SetLoggingPreference(LogType.Browser, LogLevel.All);

        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        _wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
    }

    protected PizzaAppFixture App { get; }
    protected IWebDriver Driver { get; }

    public void Dispose()
    {
        try
        {
            Driver.Quit();
        }
        catch (WebDriverException exception)
        {
            Console.Error.WriteLine($"Failed to quit the Selenium browser session: {exception.Message}");
        }
        finally
        {
            try
            {
                Driver.Dispose();
            }
            catch (WebDriverException exception)
            {
                Console.Error.WriteLine($"Failed to dispose the Selenium browser session: {exception.Message}");
            }
        }
    }

    protected IWebElement FindByTestId(string testId) =>
        _wait.Until(driver =>
        {
            var element = driver.FindElements(By.CssSelector($"[data-testid='{testId}']")).SingleOrDefault();
            return element is { Displayed: true } ? element : null;
        });

    protected IReadOnlyCollection<IWebElement> FindAllByTestIdPrefix(string prefix) =>
        _wait.Until(driver =>
        {
            var elements = driver.FindElements(By.CssSelector($"[data-testid^='{prefix}']"))
                .Where(element => element.Displayed)
                .ToArray();
            return elements.Length > 0 ? elements : null;
        });

    protected void NavigateTo(string relativePath)
    {
        Driver.Navigate().GoToUrl(new Uri(App.BaseUri, relativePath));
    }

    protected void WaitForList()
    {
        _wait.Until(driver => new Uri(driver.Url).AbsolutePath.Equals("/Pizzas", StringComparison.OrdinalIgnoreCase));
        FindByTestId("pizza-table");
    }

    protected async Task CaptureDiagnosticsOnFailureAsync(string testName, Func<Task> test)
    {
        try
        {
            await test();
        }
        catch
        {
            var directory = Path.Combine(AppContext.BaseDirectory, "TestResults", "selenium");
            Directory.CreateDirectory(directory);
            var fileStem = $"{testName}-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";

            File.WriteAllText(Path.Combine(directory, $"{fileStem}.txt"), $"""
                URL: {Driver.Url}

                Browser logs:
                {GetBrowserLogs()}

                Page source:
                {Driver.PageSource}
                """);

            if (Driver is ITakesScreenshot screenshotDriver)
            {
                screenshotDriver.GetScreenshot()
                    .SaveAsFile(Path.Combine(directory, $"{fileStem}.png"));
            }

            throw;
        }
    }

    private string GetBrowserLogs()
    {
        try
        {
            return string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog(LogType.Browser));
        }
        catch (WebDriverException)
        {
            return "Browser logs were unavailable.";
        }
    }
}
