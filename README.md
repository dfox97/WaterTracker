# WaterTracker

A small full-stack water consumption tracker built for the Everflow take-home.
Authenticated users can register, log in, and manage their daily water intake records.

## Quick start

```bash
dotnet restore
dotnet ef database update      # creates watertracker.db (SQLite)
dotnet run                     # http://localhost:5016

dotnet test                    # run the test suite
```

In development, the OpenAPI document is exposed at `/openapi/v1.json` and the Scalar UI at `/scalar/v1` for manually exercising endpoints.

## Stack

| Layer | Choice | Why |
|---|---|---|
| Web framework | .NET 10 + Blazor Server | Single project for UI and API. Auth state and DI flow naturally to the page. |
| Auth | ASP.NET Core Identity (cookie) | Scaffolded out of the box from the Visual Studio template - register, login, logout, password hashing, antiforgery, all wired up. No reason to rebuild a solved problem. |
| Data | EF Core + **SQLite** | File-based, zero setup, no Docker or external service. Reviewer clones the repo, runs `dotnet ef database update`, done. |
| Validation | **FluentValidation** | Same role as `zod` in JS - separates "shape rules" from business logic, easy to test in isolation. |
| API | Minimal API endpoints under `/api/intake` | Lightweight, no controller boilerplate. |
| Tests | **xUnit + FluentAssertions + NSubstitute** | xUnit ≈ Jasmine, FluentAssertions ≈ Jest matchers, NSubstitute ≈ ts-mockito. |
| CI | GitHub Actions | Runs `dotnet test` on every push. |

## Background

I come from a TypeScript / Angular / NestJS / GraphQL / Sequelize / Node stack. This was my first time writing C# / EF Core / Blazor end-to-end, so I leaned on AI to translate patterns I already knew (reactive forms, RxJS, NestJS DI, Sequelize models) into idiomatic .NET 10 equivalents. The architectural decisions below are mine; the syntax help came from there.

Given the deadline I deliberately picked the simpler option at every fork - SQLite over Postgres, scaffolded Identity over a custom auth flow, DataAnnotations + FluentValidation over a heavier validation pipeline. Shipping a clean, tested core mattered more than feature breadth.

## Architecture

```
UI (Tracker.razor)  ─┐
                     ├──►  IWaterIntakeService  ──►  ApplicationDbContext
API endpoints        ─┘
```

One service acts as a layer between the frontend and persistence. Both the Blazor page and the API endpoints depend on `IWaterIntakeService` - never on the `DbContext` directly. That's what the tests target.

**Validators are pure classes** (`WaterIntakeValidator`, `UpdateIntakeValidator`). They live in their own files, take no dependencies, and are tested without DI. The same validators are injected into both the API endpoints *and* the Blazor page so the rules are enforced once, in one place.

```
Components/Pages/Tracker.razor      // UI
Features/WaterIntake/
  IWaterIntakeService.cs            // the seam
  WaterIntakeService.cs             // EF Core CRUD with ownership guards
  WaterIntakeEndpoints.cs           // /api/intake/* minimal API
  WaterIntakeValidator.cs           // FluentValidation rules
  WaterIntakeModel.cs               // request/response records
Data/
  ApplicationDbContext.cs
  WaterIntakeEntry.cs
WaterTracker.Tests/                 // xUnit
```

## Auth & ownership

- Cookie-based Identity. `Components/Routes.razor` wraps every route in `AuthorizeRouteView` - pages are auth-required by default.
- API endpoints sit behind `app.MapGroup("/api/intake").RequireAuthorization()`. The current user is resolved from `ClaimsPrincipal` and passed into the service.
- The service enforces ownership on every mutation: a user can only update or delete entries they own. This is covered by explicit tests.

## Validation

FluentValidation is the single source of truth for input rules (amount range, future-date guard, notes length). It runs:

1. **At the API layer** - endpoints inject `IValidator<T>` and reject invalid bodies with `400 Bad Request`.
2. **In the Blazor page** - the same validator is injected into `Tracker.razor` and called before the service. This is necessary because Blazor Server calls the service directly (not over HTTP), so endpoint-level validation would otherwise be bypassed.

The Blazor `EditForm` also uses lightweight `DataAnnotations` for inline field hints (range, max length) - these are niceties.

## Test strategy

- **Service tests** use SQLite `:memory:` with a kept-alive connection. Closer to real SQL than `UseInMemoryDatabase`, no test doubles around `DbContext`. Covers: ownership guards on update and delete, cross-user isolation, cascade behaviour.
- **Validator tests** instantiate the validator directly. No DI, no fixtures.
- **Component tests** (bUnit) Currently skipped - time constrants but the service tests carry enough of the SOLID/testability story.

## What I'd do with more time

- Admin user management view (the brief calls it optional).
- bUnit component tests for `Tracker.razor`.
- Endpoint integration tests via `WebApplicationFactory<Program>` - `partial class Program` is already in place for it.
- Server-side pagination on the entry list.
