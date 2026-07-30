using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaApp.Models;
using PizzaApp.Services;

namespace PizzaApp.Pages.Pizzas;

public class EditModel : PageModel
{
    private readonly PizzaApiClient _pizzaApiClient;

    public EditModel(PizzaApiClient pizzaApiClient)
    {
        _pizzaApiClient = pizzaApiClient;
    }

    [BindProperty]
    public Pizza Pizza { get; set; } = new();

    [BindProperty]
    public List<string> SelectedToppings { get; set; } = [];

    public IReadOnlyList<string> AvailableToppings => ToppingOptions.All;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var pizza = await _pizzaApiClient.GetByIdAsync(id);
        if (pizza is null)
        {
            return NotFound();
        }

        Pizza = pizza;
        SelectedToppings = pizza.Toppings;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Pizza.Toppings = SelectedToppings;
        var updated = await _pizzaApiClient.UpdateAsync(id, Pizza);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToPage("Index");
    }
}
