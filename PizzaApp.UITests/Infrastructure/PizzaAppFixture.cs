using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PizzaApp.UITests.Infrastructure;

public sealed class PizzaAppFixture : IAsyncLifetime
{
    private readonly StringBuilder _applicationOutput = new();
    private readonly HttpClient _httpClient = new();
    private Process? _applicationProcess;

    public Uri BaseUri { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var port = GetAvailablePort();
        BaseUri = new Uri($"http://127.0.0.1:{port}/");

        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = FindRepositoryRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add("PizzaApp/PizzaApp.csproj");
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(BaseUri.ToString().TrimEnd('/'));

        _applicationProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start PizzaApp for Selenium tests.");
        _applicationProcess.OutputDataReceived += CaptureApplicationOutput;
        _applicationProcess.ErrorDataReceived += CaptureApplicationOutput;
        _applicationProcess.BeginOutputReadLine();
        _applicationProcess.BeginErrorReadLine();

        await WaitForApplicationAsync();
    }

    public Task DisposeAsync()
    {
        _httpClient.Dispose();

        if (_applicationProcess is { HasExited: false })
        {
            _applicationProcess.Kill(entireProcessTree: true);
            _applicationProcess.WaitForExit();
        }

        _applicationProcess?.Dispose();
        return Task.CompletedTask;
    }

    private async Task WaitForApplicationAsync()
    {
        var timeout = DateTimeOffset.UtcNow.AddSeconds(30);
        Exception? lastException = null;

        while (DateTimeOffset.UtcNow < timeout)
        {
            if (_applicationProcess?.HasExited == true)
            {
                throw new InvalidOperationException(
                    $"PizzaApp exited before it was ready.{Environment.NewLine}{GetApplicationOutput()}");
            }

            try
            {
                using var response = await _httpClient.GetAsync(new Uri(BaseUri, "api/pizzas"));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch (HttpRequestException exception)
            {
                lastException = exception;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException(
            $"PizzaApp did not become ready at {BaseUri}.{Environment.NewLine}{GetApplicationOutput()}",
            lastException);
    }

    private void CaptureApplicationOutput(object sender, DataReceivedEventArgs eventArgs)
    {
        if (eventArgs.Data is not null)
        {
            lock (_applicationOutput)
            {
                _applicationOutput.AppendLine(eventArgs.Data);
            }
        }
    }

    private string GetApplicationOutput()
    {
        lock (_applicationOutput)
        {
            return _applicationOutput.ToString();
        }
    }

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "PizzaApp.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate PizzaApp.slnx from the UI test output directory.");
    }
}
