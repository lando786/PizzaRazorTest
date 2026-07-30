using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PizzaApp.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Pizzas/Index");
}
