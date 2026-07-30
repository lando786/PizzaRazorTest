using System.Collections.Concurrent;
using PizzaApp.Models;

namespace PizzaApp.Services;

public class InMemoryPizzaStore : IPizzaStore
{
    private readonly ConcurrentDictionary<int, Pizza> _pizzas = new();
    private int _nextId;

    public InMemoryPizzaStore()
    {
        foreach (var pizza in CreateSeedData())
        {
            Add(pizza);
        }
    }

    public IReadOnlyList<Pizza> GetAll() =>
        _pizzas.Values.OrderBy(p => p.Id).ToList();

    public Pizza? GetById(int id) =>
        _pizzas.TryGetValue(id, out var pizza) ? pizza : null;

    public Pizza Add(Pizza pizza)
    {
        pizza.Id = Interlocked.Increment(ref _nextId);
        _pizzas[pizza.Id] = pizza;
        return pizza;
    }

    public bool Update(int id, Pizza pizza)
    {
        if (!_pizzas.ContainsKey(id))
        {
            return false;
        }

        pizza.Id = id;
        _pizzas[id] = pizza;
        return true;
    }

    public bool Delete(int id) => _pizzas.TryRemove(id, out _);

    private static IEnumerable<Pizza> CreateSeedData() =>
    [
        new()
        {
            Name = "Margherita",
            Description = "Classic tomato sauce, fresh mozzarella, and basil.",
            Price = 9.99m,
            Size = PizzaSize.Medium,
            Toppings = ["Extra Cheese"],
            ImageUrl = "https://picsum.photos/seed/margherita/400/300"
        },
        new()
        {
            Name = "Pepperoni",
            Description = "Loaded with pepperoni and mozzarella.",
            Price = 11.99m,
            Size = PizzaSize.Medium,
            Toppings = ["Pepperoni", "Extra Cheese"],
            ImageUrl = "https://picsum.photos/seed/pepperoni/400/300"
        },
        new()
        {
            Name = "Hawaiian",
            Description = "Ham, pineapple, and mozzarella.",
            Price = 12.49m,
            Size = PizzaSize.Large,
            Toppings = ["Bacon"],
            ImageUrl = "https://picsum.photos/seed/hawaiian/400/300"
        },
        new()
        {
            Name = "Veggie Delight",
            Description = "Mushroom, onion, peppers, and olives.",
            Price = 10.99m,
            Size = PizzaSize.Medium,
            Toppings = ["Mushroom", "Onion", "Peppers", "Olives"],
            ImageUrl = "https://picsum.photos/seed/veggie/400/300"
        },
        new()
        {
            Name = "BBQ Chicken",
            Description = "BBQ sauce, grilled chicken, red onion.",
            Price = 13.49m,
            Size = PizzaSize.Large,
            Toppings = ["Onion"],
            ImageUrl = "https://picsum.photos/seed/bbqchicken/400/300"
        },
        new()
        {
            Name = "Meat Lovers",
            Description = "Pepperoni, sausage, and bacon.",
            Price = 14.99m,
            Size = PizzaSize.Large,
            Toppings = ["Pepperoni", "Sausage", "Bacon"],
            ImageUrl = "https://picsum.photos/seed/meatlovers/400/300"
        },
        new()
        {
            Name = "Four Cheese",
            Description = "Mozzarella, parmesan, gorgonzola, and provolone.",
            Price = 12.99m,
            Size = PizzaSize.Small,
            Toppings = ["Extra Cheese"],
            ImageUrl = "https://picsum.photos/seed/fourcheese/400/300"
        }
    ];
}
