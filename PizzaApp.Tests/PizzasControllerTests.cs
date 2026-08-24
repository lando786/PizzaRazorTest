using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PizzaApp.Models;
using Xunit;

namespace PizzaApp.Tests;

public class PizzasControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public PizzasControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithSeededPizzas()
    {
        var response = await _client.GetAsync("/api/pizzas");

        response.EnsureSuccessStatusCode();
        var pizzas = await response.Content.ReadFromJsonAsync<List<Pizza>>();
        Assert.NotNull(pizzas);
        Assert.NotEmpty(pizzas!);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForUnknownId()
    {
        var response = await _client.GetAsync("/api/pizzas/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ThenGetById_ReturnsCreatedPizza()
    {
        var newPizza = new Pizza
        {
            Name = "Test Pizza",
            Description = "Created by a test",
            Price = 8.99m,
            Size = PizzaSize.Small,
            Toppings = ["Onion"],
            ImageUrl = "https://example.com/test.jpg"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/pizzas", newPizza);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<Pizza>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);

        var getResponse = await _client.GetAsync($"/api/pizzas/{created.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<Pizza>();
        Assert.Equal("Test Pizza", fetched!.Name);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_ForExistingPizza()
    {
        var created = await CreatePizzaAsync();

        created.Name = "Updated Name";
        var updateResponse = await _client.PutAsJsonAsync($"/api/pizzas/{created.Id}", created);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/pizzas/{created.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<Pizza>();
        Assert.Equal("Updated Name", fetched!.Name);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_ForUnknownId()
    {
        var response = await _client.PutAsJsonAsync("/api/pizzas/999999", new Pizza { Name = "Nope" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_ThenNotFoundOnFollowUpGet()
    {
        var created = await CreatePizzaAsync();

        var deleteResponse = await _client.DeleteAsync($"/api/pizzas/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/pizzas/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_ForUnknownId()
    {
        var response = await _client.DeleteAsync("/api/pizzas/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenPizzaExceedsConfiguredToppingLimit()
    {
        var response = await _client.PostAsJsonAsync("/api/pizzas", new Pizza
        {
            Name = "Too Many Toppings",
            Price = 10.00m,
            Toppings = ["Pepperoni", "Mushroom", "Onion", "Sausage", "Bacon"]
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("no more than 4 toppings", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenPizzaExceedsConfiguredToppingLimit()
    {
        var pizza = await CreatePizzaAsync();
        pizza.Toppings = ["Pepperoni", "Mushroom", "Onion", "Sausage", "Bacon"];

        var response = await _client.PutAsJsonAsync($"/api/pizzas/{pizza.Id}", pizza);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("no more than 4 toppings", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task WriteOperations_ReturnNotFound_WhenPizzaManagementIsDisabled()
    {
        using var disabledFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(
                [
                    new KeyValuePair<string, string?>("FeatureManagement:PizzaManagement", "false")
                ])));
        using var disabledClient = disabledFactory.CreateClient();

        var response = await disabledClient.PostAsJsonAsync("/api/pizzas", new Pizza
        {
            Name = "Disabled Feature",
            Price = 10.00m
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await disabledClient.PutAsJsonAsync("/api/pizzas/1", new Pizza { Name = "Disabled Feature" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await disabledClient.DeleteAsync("/api/pizzas/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await disabledClient.GetAsync("/Pizzas/Create")).StatusCode);
    }

    private async Task<Pizza> CreatePizzaAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/pizzas", new Pizza
        {
            Name = "Fixture Pizza",
            Price = 5.00m
        });
        return (await response.Content.ReadFromJsonAsync<Pizza>())!;
    }
}
