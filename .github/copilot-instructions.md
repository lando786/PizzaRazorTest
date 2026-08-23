# PizzaRazorTest Copilot Instructions

## Build and test

- This is a .NET 10 solution (`PizzaApp.slnx`); use the .NET 10 SDK.
- Build the full solution: `dotnet build PizzaApp.slnx`
- Run the app: `dotnet run --project PizzaApp`
- Run all tests: `dotnet test PizzaApp.slnx`
- Run one xUnit unit/API test: `dotnet test PizzaApp.Tests/PizzaApp.Tests.csproj --filter "FullyQualifiedName~PizzaApp.Tests.PizzasControllerTests.Create_ThenGetById_ReturnsCreatedPizza"`
- Run the Selenium suite: `dotnet test PizzaApp.UITests/PizzaApp.UITests.csproj`
- Run one Selenium test: `dotnet test PizzaApp.UITests/PizzaApp.UITests.csproj --filter "FullyQualifiedName~PizzaApp.UITests.PizzaCrudUiTests.Create_AddsPizzaWithSelectedToppings"`

The repository has no configured lint command. Selenium tests require Google Chrome; Selenium Manager downloads a compatible ChromeDriver. On a UI test failure, inspect `PizzaApp.UITests/bin/**/TestResults/selenium/` for the screenshot, page source, URL, and browser logs.

## Architecture

- `PizzaApp` is an ASP.NET Core Razor Pages frontend plus a controller API in the same process. `Program.cs` maps both Razor Pages and controllers and registers `IPizzaStore` as a singleton `InMemoryPizzaStore`.
- Razor Page models under `Pages/Pizzas/` must use `PizzaApiClient`, which makes real HTTP calls to `/api/pizzas`; do not bypass the API by injecting `IPizzaStore` into page models. The HTTP client base address is derived from the current request so this works both in app hosting and in-process tests.
- `Controllers/Api/PizzasController` owns the REST boundary and delegates CRUD to `IPizzaStore`. The store is thread-safe, assigns IDs, and is populated with seed pizzas at construction. Data is process-local and resets whenever the app restarts.
- `PizzaApp.Tests` combines direct store tests and API integration tests using `WebApplicationFactory<Program>`. API tests share the test host's singleton store, so additions made within those tests persist for that host.
- `PizzaApp.UITests` launches the app itself on an available loopback port, drives it with headless Chrome, and disables test parallelization because the in-memory store is shared throughout the app process. The fixture owns application startup and shutdown; UI tests should inherit `SeleniumTestBase` and use the existing collection.

## Repository conventions

- Keep `Pizza` changes consistent across the model, REST API, `PizzaApiClient`, Razor form binding, seed data, and tests. Toppings are posted separately as `SelectedToppings` and assigned to `Pizza.Toppings` before create/update calls.
- Treat `data-testid` values as the stable UI-test contract. Preserve existing IDs and add similarly named IDs for interactive elements or rendered values that new UI tests need. Dynamic pizza IDs use suffixes such as `pizza-name-{id}`, `edit-pizza-{id}`, and `delete-pizza-{id}`; topping IDs remove spaces, e.g. `pizza-toppings-ExtraCheese`.
- UI tests use `FindByTestId`/`FindAllByTestIdPrefix` and explicit waits from `SeleniumTestBase`, not CSS classes or implicit waits. New UI scenarios should use unique pizza names and wrap their bodies in `CaptureDiagnosticsOnFailureAsync`.
- Keep UI tests serial and retain `[Collection(SeleniumCollection.Name)]` for any test that uses the shared `PizzaAppFixture`.

## Custom agents

- Use the `unit-testing` custom agent for unit or API integration test work. Its instructions in `.github/agents/unit-testing.agent.md` define the repository's xUnit setup, naming, assertion, and execution conventions.
- Prefer direct store tests for `InMemoryPizzaStore` behavior and `WebApplicationFactory<Program>` tests for controller API behavior. Keep Selenium UI testing work in the existing UI-test conventions instead.

## Skills

- Use the `appsettings-sync` skill before changing `PizzaApp/appsettings*.json` files or when reviewing application configuration. The skill audits environment-specific JSON leaf-key paths against `appsettings.Development.json`.
- The `appsettings-sync` skill is audit-only: report configuration-path drift, but do not modify, reconcile, or copy configuration values while using it.
