using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using PizzaApp;
using PizzaApp.Models;
using PizzaApp.Services;

var builder = WebApplication.CreateBuilder(args);
var appConfigurationEndpoint = builder.Configuration["AppConfiguration:Endpoint"];
var useAppConfiguration = !string.IsNullOrWhiteSpace(appConfigurationEndpoint);
var refreshInterval = TimeSpan.FromSeconds(30);

if (useAppConfiguration)
{
    if (!Uri.TryCreate(appConfigurationEndpoint, UriKind.Absolute, out var endpoint))
    {
        throw new InvalidOperationException("The setting 'AppConfiguration:Endpoint' must be an absolute URI.");
    }

    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(endpoint, new DefaultAzureCredential())
            .Select("PizzaApp:*", LabelFilter.Null)
            .Select("PizzaApp:*", builder.Environment.EnvironmentName)
            .ConfigureRefresh(refreshOptions =>
                refreshOptions.RegisterAll().SetRefreshInterval(refreshInterval));

        options.UseFeatureFlags(featureFlagOptions =>
        {
            featureFlagOptions.Select(FeatureFlags.PizzaManagement, LabelFilter.Null);
            featureFlagOptions.Select(FeatureFlags.PizzaManagement, builder.Environment.EnvironmentName);
            featureFlagOptions.SetRefreshInterval(refreshInterval);
        });
    });
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services
    .AddOptions<PizzaOrderingOptions>()
    .Bind(builder.Configuration.GetSection(PizzaOrderingOptions.SectionName))
    .Validate(
        options => options.MaximumToppings > 0 && options.MaximumToppings <= ToppingOptions.All.Count,
        $"MaximumToppings must be between 1 and {ToppingOptions.All.Count}.")
    .ValidateOnStart();
builder.Services.AddFeatureManagement();
if (useAppConfiguration)
{
    builder.Services.AddAzureAppConfiguration();
}
builder.Services.AddSingleton<IPizzaStore, InMemoryPizzaStore>();
builder.Services.AddHttpClient<PizzaApiClient>((sp, client) =>
{
    var request = sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request;
    var baseUrl = request is not null
        ? $"{request.Scheme}://{request.Host}"
        : "https://localhost:5001";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (useAppConfiguration)
{
    app.UseAzureAppConfiguration();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

app.Run();

public partial class Program;
