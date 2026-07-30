# Pizza CRUD Test Bed

A small pizza-ordering CRUD app used as a practice ground for writing UI tests (e.g. Playwright, Selenium). The Razor Pages front end makes real HTTP calls to an in-process "fake" backend — a REST API backed by static, in-memory data (no database).

## Prerequisites

- **.NET 10 SDK**

  Install via Homebrew (macOS):
  ```
  brew install --cask dotnet-sdk
  ```
  This installs a `.pkg` and requires your admin password, so run it in a real terminal (not through automation) where `sudo` can prompt you.

  Alternatively, download the installer directly from [the official .NET downloads page](https://dotnet.microsoft.com/download/dotnet/10.0).

  Verify the install:
  ```
  dotnet --list-sdks
  ```
  You should see a `10.x` entry.

## Build

From the repo root:
```
dotnet build PizzaApp.slnx
```

## Run

```
dotnet run --project PizzaApp
```

The console output will print the URL to browse to (typically `https://localhost:5001` or `http://localhost:5000`). The home page redirects to the Pizzas list, where you can add, edit, and delete pizzas.

## Run the unit tests

```
dotnet test PizzaApp.slnx
```

## API endpoints

The Razor Pages UI talks to these endpoints over real HTTP (useful if you want to hit them directly, e.g. to seed data for a UI test):

| Method | Route              | Description               |
|--------|---------------------|----------------------------|
| GET    | `/api/pizzas`        | List all pizzas            |
| GET    | `/api/pizzas/{id}`   | Get a single pizza         |
| POST   | `/api/pizzas`        | Create a pizza             |
| PUT    | `/api/pizzas/{id}`   | Update a pizza             |
| DELETE | `/api/pizzas/{id}`   | Delete a pizza             |

Data resets to the seeded sample pizzas every time the app restarts.

## Project layout

- `PizzaApp/` — Razor Pages UI (`Pages/Pizzas/`) + API controller (`Controllers/Api/`) + in-memory data store (`Services/`)
- `PizzaApp.Tests/` — xUnit tests covering the in-memory store and the API endpoints

## Notes for UI testing

Key elements carry `data-testid` attributes (e.g. `pizza-row-{id}`, `edit-pizza-{id}`, `delete-pizza-{id}`, `pizza-name`, `save-pizza`, `confirm-delete`) for stable test locators.
