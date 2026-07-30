using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaApp.Models;
using PizzaApp.Services;

namespace PizzaApp.Pages.Pizzas;

public class CreateModel : PageModel
{
    private readonly PizzaApiClient _pizzaApiClient;

    public CreateModel(PizzaApiClient pizzaApiClient)
    {
        _pizzaApiClient = pizzaApiClient;
    }

    [BindProperty]
    public Pizza Pizza { get; set; } = new();

    [BindProperty]
    public List<string> SelectedToppings { get; set; } = [];

    public IReadOnlyList<string> AvailableToppings => ToppingOptions.All;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Pizza.Toppings = SelectedToppings;
        await _pizzaApiClient.CreateAsync(Pizza);
        return RedirectToPage("Index");
    }
}
