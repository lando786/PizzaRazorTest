using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PizzaApp.UITests.Infrastructure;

namespace PizzaApp.UITests;

[Collection(SeleniumCollection.Name)]
public sealed class PizzaCrudUiTests : SeleniumTestBase
{
    public PizzaCrudUiTests(PizzaAppFixture app)
        : base(app)
    {
    }

    [Fact]
    public Task List_ShowsSeededPizzasAndAddAction() =>
        CaptureDiagnosticsOnFailureAsync(nameof(List_ShowsSeededPizzasAndAddAction), () =>
        {
            NavigateTo(string.Empty);
            WaitForList();

            Assert.Contains(FindAllByTestIdPrefix("pizza-name-"), element => element.Text == "Margherita");
            Assert.Contains(FindAllByTestIdPrefix("pizza-name-"), element => element.Text == "Pepperoni");
            Assert.True(FindByTestId("add-pizza").Enabled);

            return Task.CompletedTask;
        });

    [Fact]
    public Task Create_AddsPizzaWithSelectedToppings() =>
        CaptureDiagnosticsOnFailureAsync(nameof(Create_AddsPizzaWithSelectedToppings), async () =>
        {
            var name = UniquePizzaName("Created");

            await CreatePizzaAsync(name, "Created through Selenium", "15.25", "Large", ["Bacon", "Olives"]);

            var row = FindPizzaRow(name);
            Assert.Contains("Created through Selenium", row.Text);
            Assert.Contains("Large", row.Text);
            Assert.Contains("15.25", row.Text);
            Assert.Contains("Bacon", row.Text);
            Assert.Contains("Olives", row.Text);
        });

    [Fact]
    public Task Edit_UpdatesPizzaAndToppingSelection() =>
        CaptureDiagnosticsOnFailureAsync(nameof(Edit_UpdatesPizzaAndToppingSelection), async () =>
        {
            var originalName = UniquePizzaName("BeforeEdit");
            var updatedName = UniquePizzaName("AfterEdit");
            await CreatePizzaAsync(originalName, "Before edit", "10.00", "Small", ["Onion"]);
            var pizzaId = GetPizzaId(originalName);

            FindByTestId($"edit-pizza-{pizzaId}").Click();
            FindByTestId("pizza-name").Clear();
            FindByTestId("pizza-name").SendKeys(updatedName);
            FindByTestId("pizza-description").Clear();
            FindByTestId("pizza-description").SendKeys("Updated through Selenium");
            FindByTestId("pizza-price").Clear();
            FindByTestId("pizza-price").SendKeys("18.50");
            new SelectElement(FindByTestId("pizza-size")).SelectByText("Medium");
            SetTopping("Onion", false);
            SetTopping("Extra Cheese", true);
            FindByTestId("save-pizza").Click();

            WaitForList();
            var row = FindPizzaRow(updatedName);
            Assert.Contains("Updated through Selenium", row.Text);
            Assert.Contains("Medium", row.Text);
            Assert.Contains("18.50", row.Text);
            Assert.Contains("Extra Cheese", row.Text);
            Assert.DoesNotContain("Onion", row.Text);
        });

    [Fact]
    public Task Delete_CancelPreservesPizzaThenConfirmRemovesIt() =>
        CaptureDiagnosticsOnFailureAsync(nameof(Delete_CancelPreservesPizzaThenConfirmRemovesIt), async () =>
        {
            var name = UniquePizzaName("Delete");
            await CreatePizzaAsync(name, "Delete me", "11.00", "Medium", ["Mushroom"]);
            var pizzaId = GetPizzaId(name);

            FindByTestId($"delete-pizza-{pizzaId}").Click();
            Assert.Contains(name, FindByTestId("delete-confirmation").Text);
            FindByTestId("cancel-delete").Click();
            WaitForList();
            Assert.Contains(name, FindPizzaRow(name).Text);

            FindByTestId($"delete-pizza-{pizzaId}").Click();
            FindByTestId("confirm-delete").Click();
            WaitForList();

            Assert.DoesNotContain(
                FindAllByTestIdPrefix("pizza-name-"),
                element => element.Text == name);
        });

    private async Task CreatePizzaAsync(
        string name,
        string description,
        string price,
        string size,
        IReadOnlyCollection<string> toppings)
    {
        NavigateTo("Pizzas/Create");
        FindByTestId("pizza-name").SendKeys(name);
        FindByTestId("pizza-description").SendKeys(description);
        var priceInput = FindByTestId("pizza-price");
        priceInput.Clear();
        priceInput.SendKeys(price);
        new SelectElement(FindByTestId("pizza-size")).SelectByText(size);
        FindByTestId("pizza-image-url").SendKeys("https://example.com/selenium-pizza.jpg");

        foreach (var topping in toppings)
        {
            SetTopping(topping, true);
        }

        FindByTestId("save-pizza").Click();
        WaitForList();
        FindPizzaRow(name);
        await Task.CompletedTask;
    }

    private IWebElement FindPizzaRow(string name) =>
        FindAllByTestIdPrefix("pizza-name-").Single(element => element.Text == name)
            .FindElement(By.XPath("./ancestor::tr"));

    private string GetPizzaId(string name)
    {
        var testId = FindAllByTestIdPrefix("pizza-name-")
            .Single(element => element.Text == name)
            .GetAttribute("data-testid");

        return testId is null
            ? throw new InvalidOperationException("The pizza name cell did not expose a data-testid.")
            : testId["pizza-name-".Length..];
    }

    private void SetTopping(string topping, bool selected)
    {
        var checkbox = FindByTestId($"pizza-toppings-{topping.Replace(" ", string.Empty)}");
        if (checkbox.Selected != selected)
        {
            checkbox.Click();
        }
    }

    private static string UniquePizzaName(string scenario) =>
        $"Selenium {scenario} {Guid.NewGuid():N}";
}
