using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using PizzaApp.Models;
using PizzaApp.Services;

namespace PizzaApp.Controllers.Api;

[ApiController]
[Route("api/pizzas")]
public class PizzasController : ControllerBase
{
    private readonly IPizzaStore _store;
    private readonly IOptionsSnapshot<PizzaOrderingOptions> _orderingOptions;

    public PizzasController(
        IPizzaStore store,
        IOptionsSnapshot<PizzaOrderingOptions> orderingOptions)
    {
        _store = store;
        _orderingOptions = orderingOptions;
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
    [FeatureGate(FeatureFlags.PizzaManagement)]
    public ActionResult<Pizza> Create(Pizza pizza)
    {
        if (!HasValidToppingCount(pizza))
        {
            return BadRequest($"A pizza may have no more than {_orderingOptions.Value.MaximumToppings} toppings.");
        }

        var created = _store.Add(pizza);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [FeatureGate(FeatureFlags.PizzaManagement)]
    public IActionResult Update(int id, Pizza pizza)
    {
        if (!HasValidToppingCount(pizza))
        {
            return BadRequest($"A pizza may have no more than {_orderingOptions.Value.MaximumToppings} toppings.");
        }

        return _store.Update(id, pizza) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [FeatureGate(FeatureFlags.PizzaManagement)]
    public IActionResult Delete(int id) =>
        _store.Delete(id) ? NoContent() : NotFound();

    private bool HasValidToppingCount(Pizza pizza) =>
        (pizza.Toppings?.Count ?? 0) <= _orderingOptions.Value.MaximumToppings;
}
