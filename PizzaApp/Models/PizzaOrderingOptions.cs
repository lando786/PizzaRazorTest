namespace PizzaApp.Models;

public sealed class PizzaOrderingOptions
{
    public const string SectionName = "PizzaApp:Ordering";

    public int MaximumToppings { get; set; }
}
