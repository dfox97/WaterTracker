# Architecture

WaterTracker is a take-home demo of C#, EF Core, Blazor Server, and SOLID principles. Authenticated users log and manage daily water intake.

## Stack

**.NET 10 / Blazor Server** — SQLite persistence, ASP.NET Core Identity. Started from `dotnet new blazor --auth Individual`. Identity is scaffolded; the water-tracking domain sits on top in plain folders — no DDD layering.

## Dependency rule

```
Endpoints  ──┐
              ├──▶  IWaterIntakeService  ──▶  ApplicationDbContext
Razor pages ──┘
```

Endpoints and Razor pages call `IWaterIntakeService`. Only the service touches `ApplicationDbContext`. Validators are pure.

## SOLID decisions

**No `IWaterIntakeRepository`.** EF Core's `DbContext` is already a Unit of Work + repository. Wrapping it in another interface for a single entity is ceremony, not SOLID. The service injects `ApplicationDbContext` directly.

**No `IClock`.** `DateTimeOffset.UtcNow` is used directly. If a test genuinely needs to control time, the built-in `TimeProvider` (.NET 8+) is the right reach — not a hand-rolled interface.

**`IWaterIntakeService` is the seam (DIP).** The service is what tests target. It has one job: water-intake use cases. No HTTP, no rendering.

**FluentValidation validators are their own classes.** Testable in isolation, swappable, no validation logic leaking into the service.
