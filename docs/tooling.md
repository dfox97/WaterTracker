# In-repo tooling

Two Claude Code artifacts live in `.claude/` to assist during development.

## Skills

**`.claude/skills/ts-to-csharp/`** — invoke via `/ts-to-csharp` when translating TypeScript, Angular, NestJS, or RxJS patterns into idiomatic C#/.NET, or to sanity-check a C# attempt for non-idiomatic patterns. Carries a canonical mapping table so translations stay consistent across the session.

## Agents

**`.claude/agents/solid-reviewer.md`** — a `solid-reviewer` subagent that reviews recently-changed code through an interview-defender lens: what SOLID signals are visible, what a reviewer would probe, and what TS idioms have leaked into the C#. Invoke after each meaningful slice and once before submission. It will not propose architecture changes that contradict the decisions in `docs/architecture.md`.

## TS/Angular → C# mental model

| TypeScript / Angular       | C# / ASP.NET                         |
|----------------------------|---------------------------------------|
| Angular DI                 | ASP.NET Core DI                       |
| Zod                        | FluentValidation                      |
| Jest                       | xUnit + FluentAssertions + NSubstitute|
| Prisma                     | EF Core                               |
| RxJS streams               | `async`/`await Task<T>`               |
| Angular route guards       | `[Authorize]`                         |
| NestJS service             | Application service                   |
