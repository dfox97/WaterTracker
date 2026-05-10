# WaterTracker — bonus tasks

These are all optional. The app is submittable without any of them.
Work through them in order — each one adds more value than the next.

---

## Bonus 1 — Endpoint integration tests (Tonight ~2 hours)

**Why:** The brief grades on testable code. Service and validator tests are done.
Endpoint tests prove the auth boundary and validation gate work end-to-end.
The README already advertises `partial class Program` is in place — a reviewer will notice if the test file is still empty.

**What to build:**
- `WaterTracker.Tests/Features/WaterIntake/WaterIntakeEndpointTests.cs`
- `WebApplicationFactory<Program>` with SQLite override
- Three tests is enough:
  - `GET /api/intake` unauthenticated → 401
  - `POST /api/intake` with invalid body → 400
  - `POST /api/intake` with valid body (authenticated) → 201

**The fiddly bit:** getting a valid auth cookie inside a test.
The cleanest approach for Blazor + Identity is to create a user directly in the test DB,
then POST to `/Account/Login` to get the cookie, then attach it to subsequent requests.

**Files to touch:**
- `WaterTracker.Tests/Features/WaterIntake/WaterIntakeEndpointTests.cs` (new)
- `Program.cs` — confirm `public partial class Program;` is at the bottom (already done)

---

## Bonus 2 — Admin panel (~2 hours)

**Why:** The brief explicitly says "a way to manage users and their data."
It calls the admin view optional but it is in the spec — having it ticks another box
and gives the reviewer something to click through.

**What to build:**
- New Blazor page: `Components/Pages/Admin/Users.razor` at `/admin/users`
- Read-only table: all users, their email, registration date, entry count
- Gate it — either a simple `[Authorize(Roles = "Admin")]` or a config-driven
  admin email check. Simplest: a hardcoded allowed-email list read from `appsettings.json`.
- Nav link visible only to that user

**Service layer:**
- Add `GetAllUsersAsync()` to a new `IUserSummaryService` (or extend existing service)
- Query `ApplicationDbContext.Users` joined to `WaterIntakeEntries` for counts

**Keep it simple:** read-only is fine. No delete-user UI needed for the take-home.

---

## Bonus 3 — Final QA (Tuesday morning, ~1 hour)

**Do this before anything else on Tuesday — not after.**

Checklist:
- [ ] Fresh clone into a new folder
- [ ] `dotnet restore`
- [ ] `dotnet ef database update`
- [ ] `dotnet run` → http://localhost:5016
- [ ] Register a new user
- [ ] Log in
- [ ] Add 3 entries
- [ ] Edit one (change amount + timestamp)
- [ ] Delete one
- [ ] Log out
- [ ] Log back in → entries still there
- [ ] `GET /api/intake` in browser (unauthenticated) → redirects or 401
- [ ] `dotnet test` from solution root → all green
- [ ] CI badge on GitHub is green

---

## Bonus 4 — Submit (Tuesday, before 17:00)

- [ ] Push final state to GitHub
- [ ] Confirm CI workflow is green in the Actions tab
- [ ] Check `appsettings.json` — no secrets committed
- [ ] Email Peter the repo URL **well before 17:00** — not at 16:58
