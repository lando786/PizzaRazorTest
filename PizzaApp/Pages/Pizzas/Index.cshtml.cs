using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaApp.Models;
using PizzaApp.Services;

namespace PizzaApp.Pages.Pizzas;

public class IndexModel : PageModel
{
    private readonly PizzaApiClient _pizzaApiClient;

    public IndexModel(PizzaApiClient pizzaApiClient)
    {
        _pizzaApiClient = pizzaApiClient;
    }

    public List<Pizza> Pizzas { get; set; } = [];

    public async Task OnGetAsync()
    {
        Pizzas = await _pizzaApiClient.GetAllAsync();
    }
}
