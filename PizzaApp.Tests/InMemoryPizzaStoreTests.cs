using PizzaApp.Models;
using PizzaApp.Services;
using Xunit;

namespace PizzaApp.Tests;

public class InMemoryPizzaStoreTests
{
    [Fact]
    public void Constructor_SeedsSampleData()
    {
        var store = new InMemoryPizzaStore();

        var all = store.GetAll();

        Assert.NotEmpty(all);
    }

    [Fact]
    public void GetAll_ReturnsItemsOrderedById()
    {
        var store = new InMemoryPizzaStore();

        var all = store.GetAll();

        Assert.Equal(all.OrderBy(p => p.Id).Select(p => p.Id), all.Select(p => p.Id));
    }

    [Fact]
    public void GetById_ReturnsNull_WhenNotFound()
    {
        var store = new InMemoryPizzaStore();

        var result = store.GetById(-1);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_ReturnsPizza_WhenFound()
    {
        var store = new InMemoryPizzaStore();
        var seeded = store.GetAll()[0];

        var result = store.GetById(seeded.Id);

        Assert.NotNull(result);
        Assert.Equal(seeded.Name, result!.Name);
    }

    [Fact]
    public void Add_AssignsIncrementingId()
    {
        var store = new InMemoryPizzaStore();
        var countBefore = store.GetAll().Count;

        var added = store.Add(new Pizza { Name = "Test Pizza" });

        Assert.True(added.Id > 0);
        Assert.Equal(countBefore + 1, store.GetAll().Count);
    }

    [Fact]
    public void Update_ModifiesExistingPizza()
    {
        var store = new InMemoryPizzaStore();
        var added = store.Add(new Pizza { Name = "Original" });

        var updated = store.Update(added.Id, new Pizza { Name = "Updated" });

        Assert.True(updated);
        Assert.Equal("Updated", store.GetById(added.Id)!.Name);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenNotFound()
    {
        var store = new InMemoryPizzaStore();

        var updated = store.Update(-1, new Pizza { Name = "Nope" });

        Assert.False(updated);
    }

    [Fact]
    public void Delete_RemovesPizza()
    {
        var store = new InMemoryPizzaStore();
        var added = store.Add(new Pizza { Name = "Temp" });

        var deleted = store.Delete(added.Id);

        Assert.True(deleted);
        Assert.Null(store.GetById(added.Id));
    }

    [Fact]
    public void Delete_ReturnsFalse_WhenNotFound()
    {
        var store = new InMemoryPizzaStore();

        var deleted = store.Delete(-1);

        Assert.False(deleted);
    }
}
