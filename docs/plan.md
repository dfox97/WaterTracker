# WaterTracker — bite-sized TDD-first plan

## Context

You've got a take-home from Everflow due **Tue 12 May 17:00**, today is **Wed 7 May**, and you're out **Mon 11**. You're a TS/Angular dev being graded primarily on **SOLID** and **testable code**, not feature breadth.

Identity scaffolding is done; everything else is empty. The plan below splits the remaining work into **very small slices** — one concept at a time. Each CRUD operation is its own slice. Tests come **before** implementation (red → green → commit). The Blazor UI is deliberately last so all logic is already proven by tests when you start clicking buttons.

Order of concerns, from earliest to latest:
1. Database & migrations
2. Service layer (TDD: one CRUD op at a time)
3. Validation
4. API endpoints (one at a time, behind auth)
5. Blazor UI (last — it's just a thin client over the working API/service)
6. CI + README

If a slice goes over its time-box by more than 50%, **stop, commit what works, and move on**. The deadline is firmer than any single slice.

---

## Phase A — Database foundation (Wed 7 May AM)

### A1. Swap to SQLite (~20 min)
- `WaterTracker.csproj`: replace `Microsoft.EntityFrameworkCore.SqlServer` with `Microsoft.EntityFrameworkCore.Sqlite`.
- `Program.cs:27`: `UseSqlServer` → `UseSqlite`.
- `appsettings.json` (both): `"DefaultConnection": "Data Source=watertracker.db"`.
- Delete contents of `Data/Migrations/`.
- `dotnet build` → green. Commit.

### A2. Regenerate Identity migration (~15 min)
- `dotnet ef migrations add InitialIdentity`
- `dotnet ef database update`
- `dotnet run` → register a user, log in, log out. Confirm SQLite db file appears.
- Commit.

### A3. Add `WaterIntakeEntry` entity (~20 min)
- `Data/WaterIntakeEntry.cs`: `class` (not record — EF tracking) with `required` props: `Id` (Guid), `UserId` (string), `AmountMl` (int), `RecordedAt` (DateTimeOffset), `Notes` (string?).
- `ApplicationDbContext`: add `DbSet<WaterIntakeEntry>`.
- Commit.

### A4. Configure relationship + index + migrate (~25 min)
- `OnModelCreating`: FK `WaterIntakeEntry.UserId` → `ApplicationUser.Id` with `OnDelete(Cascade)`. Index `(UserId, RecordedAt)`.
- `dotnet ef migrations add AddWaterIntakeEntry`
- `dotnet ef database update`
- Commit.

---

## Phase B — Test project (Wed 7 May PM)

### B1. Create test project (~20 min)
- `dotnet new xunit -o WaterTracker.Tests`, add to solution, reference main project.
- Commit.

### B2. Add test packages (~10 min)
- `FluentAssertions`, `NSubstitute`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.AspNetCore.Mvc.Testing`.
- Commit.

### B3. Write the in-memory DB helper (~30 min)
- A small helper that opens `Data Source=:memory:`, keeps the connection alive, calls `EnsureCreated()`, returns a configured `ApplicationDbContext`.
- One sanity test: open it, assert `WaterIntakeEntries.Count() == 0`. `dotnet test` → green.
- Commit. **This helper is what every service test will use** — get it right once.

---

## Phase C — Service layer, TDD, one operation at a time (Thu 8 May)

Pattern for every slice in this phase:
1. Add a method to `IWaterIntakeService` (or stub it with `throw new NotImplementedException`).
2. Write the test → red.
3. Implement → green.
4. Commit.

### C1. Create empty service skeleton (~15 min)
- `Features/WaterIntake/IWaterIntakeService.cs` — interface, no methods yet.
- `Features/WaterIntake/WaterIntakeService.cs` — `class WaterIntakeService(ApplicationDbContext db) : IWaterIntakeService { }`.
- `Features/WaterIntake/WaterIntakeModels.cs` — empty file.
- Register in `Program.cs`: `builder.Services.AddScoped<IWaterIntakeService, WaterIntakeService>();`.
- Build green. Commit.

### C2. **Create** — test first (~45 min)
- Add `record CreateIntakeRequest` (required `AmountMl`, `RecordedAt`, optional `Notes`).
- Test: `AddAsync` for user A persists a row owned by user A.
- Implement `AddAsync(string userId, CreateIntakeRequest req, CancellationToken ct)`.
- Green. Commit.

### C3. **Read (list for user)** — test first (~45 min)
- Add `record IntakeResponse`.
- Test: seed 2 users with 2 entries each; `GetForUserAsync` for user A returns only A's entries.
- Implement.
- Green. Commit.

### C4. **Update — happy path** — test first (~45 min)
- Add `record UpdateIntakeRequest`.
- Test: user A updates their own entry; new values are persisted.
- Implement `UpdateAsync(string userId, Guid entryId, UpdateIntakeRequest req, CancellationToken ct)`.
- Green. Commit.

### C5. **Update — ownership guard** — test first (~30 min)
- Test: user B tries to update user A's entry → fails.
- Green. Commit. **This is the test that scores you SOLID points.**

### C6. **Delete — happy path** — test first (~30 min)
- Test: user A deletes own entry → row is gone.
- Implement `DeleteAsync(string userId, Guid entryId, CancellationToken ct)`.
- Green. Commit.

### C7. **Delete — ownership guard** — test first (~20 min)
- Test: user B can't delete user A's entry.
- Green. Commit.

### C8. **Read by id** — test first (~20 min)
- Test: user A gets their own entry by id; user B gets `null`.
- Implement `GetByIdAsync`.
- Green. Commit.

End of Phase C: ~7–8 service tests, all CRUD covered, ownership enforced.

---

## Phase D — Validation (Fri 9 May AM, ~1.5 hours)

### D1. Add FluentValidation package + DI (~15 min)
- `FluentValidation.AspNetCore` to main project.
- `builder.Services.AddValidatorsFromAssemblyContaining<WaterIntakeValidator>();` in `Program.cs`.
- Commit.

### D2. **`AmountMl` rule** — test first (~20 min)
- `WaterIntakeValidator.cs` validating `CreateIntakeRequest`.
- Tests: 0 fails, negative fails, 10001 fails, 250 passes.
- Commit.

### D3. **`RecordedAt` rule** — test first (~20 min)
- Test: future date (>1 min ahead) fails; now passes; yesterday passes.
- Commit.

### D4. **`Notes` rule** — test first (~15 min)
- Test: 501 chars fails; 500 passes; null passes.
- Commit.

---

## Phase E — API endpoints, one verb at a time (Fri 9 May PM)

### E1. Endpoint group skeleton (~20 min)
- `Features/WaterIntake/WaterIntakeEndpoints.cs`: extension method `MapWaterIntakeEndpoints`.
- `app.MapGroup("/api/intake").RequireAuthorization()` — empty group.
- Add `public partial class Program;` at bottom of `Program.cs`.
- Build green. Commit.

### E2. **GET /api/intake** (~30 min)
- Maps to `GetForUserAsync` using `ClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)`.
- Manual test: log in, hit `/api/intake` → 200 empty array.
- Commit.

### E3. **POST /api/intake** (~40 min)
- Validate via injected `IValidator<CreateIntakeRequest>`; return `Results.ValidationProblem` on failure.
- Calls `AddAsync`. Returns 201 with the new resource.
- Commit.

### E4. **PUT /api/intake/{id}** (~30 min)
- Validate; call `UpdateAsync`. 404 if not found/not owned, 204 on success.
- Commit.

### E5. **DELETE /api/intake/{id}** (~20 min)
- Call `DeleteAsync`. 404 / 204.
- Commit.

### E6. Endpoint integration tests (~75 min, **hard cap**)
- `WebApplicationFactory<Program>` with SQLite override.
- Cover: anonymous GET → 401; authenticated POST → 201; invalid body → 400.
- **If over 75 min, stop.** Service tests cover the logic; note the gap in README.
- Commit.

---

## Phase F — Blazor UI (Sat 10 May)

Inject `IWaterIntakeService` directly — no need to call the HTTP API from Blazor Server.

### F1. Tracker page skeleton (~30 min)
- `Components/Pages/Tracker.razor` with `@page "/tracker"` and `@rendermode InteractiveServer`.
- Inject service and `AuthenticationStateProvider`. Render "Hello, {userId}".
- Add nav link in `Components/Layout/NavMenu.razor`.
- Commit.

### F2. List today's entries (~30 min)
- `OnInitializedAsync` calls `GetForUserAsync`.
- Table with amount, time, notes. Show "Total today: X ml".
- Commit.

### F3. Add-entry form (~45 min)
- Amount input + notes textarea. On submit: call `AddAsync`, refresh list.
- Commit.

### F4. Delete button per row (~30 min)
- Button + simple confirmation; call `DeleteAsync`; refresh.
- Commit.

### F5. Inline edit (~45 min)
- Toggle row to edit mode; submit calls `UpdateAsync`.
- Commit.

### F6. Manual end-to-end smoke (~15 min)
- Register → login → add 3 → edit 1 → delete 1 → logout → login → entries still there.
- Commit.

---

## Phase G — Ship-ready (Sat 10 May late / Tue 12 morning)

### G1. CI workflow (~30 min)
- `.github/workflows/ci.yml`: setup-dotnet 10, restore, build, test.
- Push, confirm green.

### G2. README (~45 min)
- How to run, architecture summary, test strategy, tech-choice justifications (interview talking points).

### G3. Final QA + submit (Tue 12 morning)
- Fresh clone walkthrough. `dotnet test` green. No secrets in `appsettings.json`.
- Push final, email Peter the repo URL **before lunch**.

### Stretch (only if ahead of schedule)
- Admin user list — brief calls it optional. Easy time sink, skip unless comfortable.
- bUnit component tests — CLAUDE.md defers these.

---

## Critical files

| File | Action |
|------|--------|
| `WaterTracker.csproj` | Package swaps, add FluentValidation |
| `Program.cs` | `UseSqlite`, DI registrations, `MapWaterIntakeEndpoints`, `partial class Program` |
| `appsettings.json` + Development variant | Connection string |
| `Data/ApplicationDbContext.cs` | `DbSet<WaterIntakeEntry>`, `OnModelCreating` |
| `Data/WaterIntakeEntry.cs` | New entity |
| `Data/Migrations/` | Regenerate after SQLite swap |
| `Features/WaterIntake/` | Entire folder — new |
| `Components/Pages/Tracker.razor` | New page |
| `Components/Layout/NavMenu.razor` | Add nav link |
| `WaterTracker.Tests/` | New project |
| `.github/workflows/ci.yml` | New |
| `README.md` | Fill in during G2 |

## Top risks

- **Migrations go sideways under SQLite** → delete `Data/Migrations/` + `watertracker.db`, regenerate from scratch.
- **WebApplicationFactory + Identity auth** → respect the 75-min cap on E6.
- **Blazor buttons don't respond** → missing `@rendermode InteractiveServer`.
- **Scope creep on admin view** → it's optional, skip it.
