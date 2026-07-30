using System.Net.Http.Json;
using PizzaApp.Models;

namespace PizzaApp.Services;

public class PizzaApiClient
{
    private readonly HttpClient _httpClient;

    public PizzaApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Pizza>> GetAllAsync() =>
        await _httpClient.GetFromJsonAsync<List<Pizza>>("api/pizzas") ?? [];

    public async Task<Pizza?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/pizzas/{id}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<Pizza>()
            : null;
    }

    public async Task<Pizza?> CreateAsync(Pizza pizza)
    {
        var response = await _httpClient.PostAsJsonAsync("api/pizzas", pizza);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<Pizza>()
            : null;
    }

    public async Task<bool> UpdateAsync(int id, Pizza pizza)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/pizzas/{id}", pizza);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/pizzas/{id}");
        return response.IsSuccessStatusCode;
    }
}
