namespace PizzaApp.Models;

public class Pizza
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public PizzaSize Size { get; set; } = PizzaSize.Medium;
    public List<string> Toppings { get; set; } = [];
    public string ImageUrl { get; set; } = string.Empty;
}
