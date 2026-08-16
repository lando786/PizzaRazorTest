using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using PizzaApp.Models;
using PizzaApp.Services;

namespace PizzaApp.Pages.Pizzas;

[FeatureGate(FeatureFlags.PizzaManagement)]
public class CreateModel : PageModel
{
    private readonly PizzaApiClient _pizzaApiClient;
    private readonly IOptionsSnapshot<PizzaOrderingOptions> _orderingOptions;

    public CreateModel(
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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || !HasValidToppingCount())
        {
            return Page();
        }

        Pizza.Toppings = SelectedToppings;
        await _pizzaApiClient.CreateAsync(Pizza);
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
