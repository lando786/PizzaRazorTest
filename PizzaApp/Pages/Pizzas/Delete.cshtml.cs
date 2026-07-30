using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaApp.Models;
using PizzaApp.Services;

namespace PizzaApp.Pages.Pizzas;

public class DeleteModel : PageModel
{
    private readonly PizzaApiClient _pizzaApiClient;

    public DeleteModel(PizzaApiClient pizzaApiClient)
    {
        _pizzaApiClient = pizzaApiClient;
    }

    [BindProperty]
    public Pizza Pizza { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var pizza = await _pizzaApiClient.GetByIdAsync(id);
        if (pizza is null)
        {
            return NotFound();
        }

        Pizza = pizza;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _pizzaApiClient.DeleteAsync(id);
        return RedirectToPage("Index");
    }
}
