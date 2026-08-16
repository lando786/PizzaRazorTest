using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using PizzaApp.Models;
using PizzaApp.Services;

namespace PizzaApp.Pages.Pizzas;

[FeatureGate(FeatureFlags.PizzaManagement)]
public class EditModel : PageModel
{
    private readonly PizzaApiClient _pizzaApiClient;
    private readonly IOptionsSnapshot<PizzaOrderingOptions> _orderingOptions;

    public EditModel(
        PizzaApiClient pizzaApiClient,
        IOptionsSnapshot<PizzaOrderingOptions> orderingOptions)
    {
        _pizzaApiClient = pizzaApiClient;
        _orderingOptions = orderingOptions;
    }

    [BindProperty]
    public Pizza Pizza { get; set; } = new();

    [BindProperty]
    public List<string> SelectedToppings { get; set; } = [];

    public IReadOnlyList<string> AvailableToppings => ToppingOptions.All;

    public int MaximumToppings => _orderingOptions.Value.MaximumToppings;

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
        if (!ModelState.IsValid || !HasValidToppingCount())
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

    private bool HasValidToppingCount()
    {
        if (SelectedToppings.Count <= MaximumToppings)
        {
            return true;
        }

        ModelState.AddModelError(
            nameof(SelectedToppings),
            $"Select no more than {MaximumToppings} toppings.");
        return false;
    }
}
