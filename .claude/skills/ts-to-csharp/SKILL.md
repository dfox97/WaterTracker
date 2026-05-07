---
name: ts-to-csharp
description: Translate TypeScript / Angular / NestJS / RxJS code into idiomatic C# / .NET 10 / Blazor / EF Core, OR sanity-check a C# attempt for non-idiomatic patterns. Invoke when the user pastes TS code asking for the C# equivalent, asks "how do I write X in C#", or asks whether their C# is idiomatic.
---

# ts-to-csharp

A focused translator + idiom-checker for a TypeScript/Angular developer learning C#/.NET on this WaterTracker project.

## Two modes

Pick the mode from the user's framing:

- **Translate** — input is TS/Angular/Nest/RxJS. Output: idiomatic C#/.NET equivalent + a *short* "what changed and why."
- **Idiom-check** — input is the user's C# attempt. Output: what's idiomatic, what isn't, with the fix shown inline.

If the request is ambiguous, ask one short question. Don't both at once.

## Output shape

Keep it tight. Preferred structure:

```
**C#:**
<code block>

**Why:** 2-4 bullets, max one sentence each. Cover the non-obvious mapping decisions.
**Watch out for:** 0-2 bullets only if there's a real footgun (skip otherwise).
```

Never lecture. Never restate what the TS does. Never include "this is just like Angular's..." filler — the user knows.

## Canonical mappings

Apply consistently across the session:

| TS / Angular / Nest | C# / .NET |
|---|---|
| Angular DI / NestJS providers | `services.AddScoped/Singleton/Transient<I, Impl>()` in `Program.cs` |
| `@Injectable({ providedIn: 'root' })` | `AddScoped` — request-scoped is the safe default for Blazor Server |
| Reactive form + Zod | `EditForm` + FluentValidation validator class |
| Angular guard / Nest `@UseGuards` | `[Authorize]` attribute or `.RequireAuthorization()` on endpoint groups |
| RxJS `Observable<T>` | usually `Task<T>` (`async`/`await`). Only reach for `IAsyncEnumerable<T>` if it's genuinely a stream |
| RxJS `pipe(map, filter)` | LINQ `.Select(...).Where(...)` (sync) or `await foreach` over `IAsyncEnumerable` |
| `Promise<T>` | `Task<T>` |
| `Promise.all([...])` | `await Task.WhenAll(...)` |
| Prisma / TypeORM repository | EF Core `DbContext` + `DbSet<T>` (no separate repo on this project) |
| `prisma.user.findMany({ where })` | `db.Users.Where(u => ...).ToListAsync(ct)` |
| Jest / Vitest | xUnit + FluentAssertions |
| `expect(x).toBe(y)` | `x.Should().Be(y)` |
| ts-mockito / `jest.mock` | NSubstitute (`Substitute.For<IFoo>()`, `.Returns(...)`) |
| TS `interface` (data shape) | `record` (immutable DTO) |
| TS `interface` (contract) | C# `interface` (PascalCase, `I`-prefixed) |
| TS `class` (service) | C# `class`, with extracted `interface` for the seam |
| `null` vs `undefined` | C# has `null` only; nullable reference types (`T?`) carry the "maybe absent" signal |
| `??` nullish coalescing | C# `??` (same) |
| `?.` optional chaining | C# `?.` (same) |
| `JSON.stringify` / `JSON.parse` | `System.Text.Json.JsonSerializer.Serialize/Deserialize` |
| `fetch` | `HttpClient` (inject via `IHttpClientFactory`) |
| `setTimeout` | `await Task.Delay(...)` |
| `Date` | `DateTimeOffset` (prefer over `DateTime`) |
| `Map<K, V>` / `Set<T>` | `Dictionary<K, V>` / `HashSet<T>` |
| `Array.map/filter/reduce` | LINQ `.Select` / `.Where` / `.Aggregate` |
| `as const` / readonly array | `IReadOnlyList<T>` / `ImmutableArray<T>` |
| Angular `*ngIf` | Razor `@if` |
| Angular `*ngFor` | Razor `@foreach` |
| Angular `[(ngModel)]` | Blazor `@bind-Value` |
| `@Input()` | `[Parameter]` on a Razor component |
| `@Output()` `EventEmitter` | `[Parameter] public EventCallback<T> OnX { get; set; }` |

## Project-specific defaults

This is a Blazor Server + EF Core + SQLite + Identity project. When in doubt, prefer:

- `record` over `class` for DTOs.
- Service-with-interface (`IWaterIntakeService` / `WaterIntakeService`) — *no separate repository* in this codebase. Service depends on `ApplicationDbContext` directly.
- `DateTimeOffset.UtcNow` directly — *no `IClock`* in this codebase.
- All async methods take `CancellationToken ct` and end with `Async`.
- FluentValidation validators in `Validation/`, request DTOs in `Dtos/`.
- DI registration goes in `Program.cs`. New service → register there alongside the others.
- Endpoints live in `Endpoints/WaterIntakeEndpoints.cs` as a `MapWaterIntake(this IEndpointRouteBuilder)` extension, called from `Program.cs`.

If the user's input contradicts these defaults (e.g. asks to write a repository), follow the project convention and note the divergence in one line — don't silently invent a repository.

## Idiom-check checklist

When the user pastes C# and asks if it's idiomatic, scan for:

- **Naming**: PascalCase for types/methods/properties; `I`-prefix on interfaces; `_camelCase` for private fields; `Async` suffix on async methods.
- **Async hygiene**: missing `await`, missing `CancellationToken`, `.Result` / `.Wait()` (deadlock risk), `async void` outside event handlers.
- **Nullability**: `!` overrides used to silence the compiler instead of fixing the design; missing `?` on parameters that can be null.
- **Records vs classes**: DTOs/value objects should be `record`; services/entities should be `class`.
- **EF Core shape**: client-side evaluation via `.ToList()` mid-query, missing `.AsNoTracking()` on read-only queries, N+1 from missing `.Include`.
- **DI lifetime**: `Singleton` capturing a `Scoped` dependency (DbContext leak).
- **TS habits leaking**: `string.IsNullOrEmpty` where `string.IsNullOrWhiteSpace` is meant; `==` on reference types where `Equals` / records are intended; throwing strings in `catch`.

Report only what's *actually* present. Don't invent issues to fill space.

## Examples of the desired output

**User:** "translate this Angular service to C#: `@Injectable({providedIn:'root'}) class WaterService { constructor(private http: HttpClient) {} list() { return this.http.get<Intake[]>('/api/intake'); } }`"

**Good response:**

```csharp
public interface IWaterClient
{
    Task<IReadOnlyList<IntakeDto>> ListAsync(CancellationToken ct = default);
}

public sealed class WaterClient(HttpClient http) : IWaterClient
{
    public async Task<IReadOnlyList<IntakeDto>> ListAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<IReadOnlyList<IntakeDto>>("/api/intake", ct) ?? [];
}
```

**Why:**
- Extracted an interface so consumers depend on the abstraction (DIP), matching the project's service-with-interface convention.
- `Task<T>` replaces RxJS — Blazor doesn't lean on streams; `await` is idiomatic.
- `GetFromJsonAsync` returns `null` on 204; `?? []` keeps the contract non-null.
- Primary constructor `(HttpClient http)` is the C# 12 equivalent of TS constructor injection.

---

Avoid output longer than the input. Brevity is a feature.
