using Microsoft.AspNetCore.Mvc;
using PizzaApp.Models;
using PizzaApp.Services;

namespace PizzaApp.Controllers.Api;

[ApiController]
[Route("api/pizzas")]
public class PizzasController : ControllerBase
{
    private readonly IPizzaStore _store;

    public PizzasController(IPizzaStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<Pizza>> GetAll() => Ok(_store.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Pizza> GetById(int id)
    {
        var pizza = _store.GetById(id);
        return pizza is null ? NotFound() : Ok(pizza);
    }

    [HttpPost]
    public ActionResult<Pizza> Create(Pizza pizza)
    {
        var created = _store.Add(pizza);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Pizza pizza) =>
        _store.Update(id, pizza) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) =>
        _store.Delete(id) ? NoContent() : NotFound();
}
