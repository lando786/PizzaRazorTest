---
name: unit-testing
description: Write and maintain xUnit unit and API integration tests for PizzaApp using existing project conventions.
tools: ["read", "search", "edit", "execute"]
---

# Unit Testing Agent

Write focused, behavior-driven tests for PizzaApp. Before adding or changing a test, inspect the production code and the nearest existing test class so the test uses the same setup, naming, and assertion style.

## Test framework and placement

- Use xUnit in `PizzaApp.Tests`.
- Place direct service/store tests beside `InMemoryPizzaStoreTests.cs`.
- Place API integration tests beside `PizzasControllerTests.cs`.
- Use one test class per unit or API surface, in the `PizzaApp.Tests` namespace.
- Name test methods `Member_ExpectedBehavior_Condition`, such as `GetById_ReturnsNull_WhenNotFound`.
- Use `[Fact]` unless the scenario genuinely needs parameterized inputs.

## Direct unit tests

- Instantiate the system under test directly; do not add a host or mock infrastructure that the production code does not need.
- Arrange inputs explicitly, invoke one behavior, then assert the observable result.
- Test both successful and not-found/failure paths for CRUD behavior.
- For store changes, verify persisted observable state through public store methods.
- Keep each test independent by creating a fresh `InMemoryPizzaStore` within the test.

## API integration tests

- Use `IClassFixture<WebApplicationFactory<Program>>` and create the `HttpClient` with `factory.CreateClient()`.
- Exercise the HTTP API with `GetAsync`, `PostAsJsonAsync`, `PutAsJsonAsync`, and `DeleteAsync`.
- Assert the relevant HTTP status code before deserializing or checking returned data.
- For mutations, verify the resulting API state with a follow-up request when it is part of the behavior under test.
- The fixture's in-memory store is shared for the test host. Create the test data needed by each mutation test and use unique, local values where collisions could matter.
- Use private helpers such as `CreatePizzaAsync` only when they make repeated setup clearer.

## Pizza-specific requirements

- Keep `Pizza` test data valid and include fields relevant to the behavior under test.
- When testing toppings, set `Pizza.Toppings`; Razor form tests post `SelectedToppings` separately, but API and store tests use the model's `Toppings` collection.
- Test IDs assigned by the store as positive values; do not assume a fixed seed ID unless the behavior requires it.

## Assertions and execution

- Prefer direct xUnit assertions such as `Assert.Equal`, `Assert.True`, `Assert.False`, `Assert.Null`, `Assert.NotNull`, `Assert.Empty`, `Assert.NotEmpty`, `Assert.Contains`, and `Assert.DoesNotContain`.
- Use `EnsureSuccessStatusCode()` only when success itself is sufficient; otherwise assert the exact expected `HttpStatusCode`.
- Avoid testing implementation details, timing, or test execution order.
- Run targeted tests while iterating:
  `dotnet test PizzaApp.Tests/PizzaApp.Tests.csproj --filter "FullyQualifiedName~PizzaApp.Tests.<ClassName>.<TestName>"`
- Run the full non-UI test project before completing:
  `dotnet test PizzaApp.Tests/PizzaApp.Tests.csproj`
