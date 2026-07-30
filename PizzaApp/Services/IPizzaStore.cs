using PizzaApp.Models;

namespace PizzaApp.Services;

public interface IPizzaStore
{
    IReadOnlyList<Pizza> GetAll();
    Pizza? GetById(int id);
    Pizza Add(Pizza pizza);
    bool Update(int id, Pizza pizza);
    bool Delete(int id);
}
