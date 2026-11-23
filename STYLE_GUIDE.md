# League Manager Style Guide

Version 1.1. "League Manager" is a placeholder name until the project has a real one.

## 1. About this guide

- This guide covers **how** code is written in this repo. **What** gets built and when lives on the GitHub Projects board.
- It's a living document. Sections marked "Later" get filled in when I reach that part of the project, not before.
- If I change how I do something, I update this guide in the same pull request.

## 2. Repo layout

One repo, one folder per piece of the system.

```
league-manager/
  api/                          C# / .NET Web API
    src/LeagueManager.Api/
      Controllers/
      Services/
      Data/                     DbContext, entity configs, migrations
      Models/                   Entities (match database tables)
      Dtos/                     Request and response shapes
    tests/
      LeagueManager.Api.UnitTests/
      LeagueManager.Api.IntegrationTests/
      LeagueManager.Api.TestData/   Shared test data builders (fake data only)
  web/                          Angular admin site (Later)
  scheduler/                    Java schedule generator (Later)
  mobile/                       Coach's sideline app (Later)
  docs/
    decisions/                  Short records of big decisions (see section 9)
  README.md
  STYLE_GUIDE.md
```

## 3. General rules

### How I use AI

- AI tools (Copilot, Claude) can explain concepts, suggest code, review my code, and help me debug.

- I only commit code I can explain line by line. If I can't explain a line, I look it up or ask until I can, or I don't commit it.

- I make the design decisions: what the classes are, where the rules live, what gets tested.

- Business rules are test-first, and I write those tests myself. The tests are where I prove I understand what the code should do.

- In interviews, I'm open about using AI tools, the same way I'd  mention any other tool.

### Git

- `main` always builds and passes its tests.
- All work happens on a branch: `feature/<issue-number>-short-name` or `fix/<issue-number>-short-name`.
  Example: `feature/12-create-team-endpoint`
- Commit messages are short and start with a verb: `Add create team endpoint (#12)`
- Everything goes to `main` through a pull request, even working solo. The PR description says what changed and links the story.
- Branch protection is on for `main`: no direct pushes, and every CI check must pass before merging. This applies to me too.

### Definition of done

A story is done when:

1. The code builds with zero warnings (warnings are treated as errors)
2. All tests pass, and the new behavior has tests
3. Every CI check is green, including coverage thresholds (see section 6)
4. The story's acceptance criteria are met, and each one has at least one test that proves it
5. This guide was followed
6. It's merged to `main` through a pull request

### Athletes' data

This app holds personal information about athletes of any age, including some who may be minors, so:

- Never log names, birthdates, or contact info
- Never use real athletes' data in seed data or tests. Fake data only.
- Secrets (connection strings, keys) never go in the repo

## 4. C# API

### Basics

- .NET 10 (the current long-term support version), ASP.NET Core Web API using **controllers**
- Nullable reference types turned on
- `TreatWarningsAsErrors` turned on for every project, including test projects
- One class per file, and the file is named after the class

### Naming

| Thing | Style | Example |
|---|---|---|
| Classes, methods, properties | PascalCase | `TeamService`, `GetTeamAsync` |
| Interfaces | Start with I | `ITeamService` |
| Local variables, parameters | camelCase | `teamId` |
| Private fields | _camelCase | `_teamService` |
| Async methods | End in Async | `CreateTeamAsync` |

### Layers

Requests flow one way: **Controller → Service → Data**

- **Controllers** stay thin. They take the request, call a service, and return the response. Controllers never touch the DbContext.
- **Services** hold the business rules (a player can only be on one team per season, for example). Each service has an interface and is registered for dependency injection.
- **Data** is the EF Core DbContext and entity configuration. Only services use it.

### Entities and DTOs

- Entities match database tables and never leave the API.
- Requests and responses always use DTOs.
  Naming: `CreateTeamRequest`, `UpdateTeamRequest`, `TeamResponse`

### Routes

- Plural nouns, lowercase, kebab-case, versioned: `/api/v1/leagues/{leagueId}/teams`
- Use the right HTTP verb: GET reads, POST creates, PUT updates, DELETE removes

### Errors

- Return the right status code: 400 bad input, 404 not found, 409 conflict (like a double booked field), 500 for unexpected errors
- Error responses use the built-in ProblemDetails format
- One global exception handler. Stack traces never get sent to the client.

### Async

- Use async/await for anything that touches the database or network
- Never use `.Result` or `.Wait()`

### Time

- Never call `DateTime.Now` or `DateTime.UtcNow` directly. Inject `TimeProvider` and ask it for the time.
- This keeps anything that depends on the date (season dates, game day, age cutoffs, registration windows) testable.

### Outside services

- Anything outside the API (the Java scheduler, AWS, email, push notifications) sits behind an interface, like `IScheduleGenerator`. Services depend on the interface, never the concrete client.

### Logging

- Use `ILogger<T>` injected through the constructor
- Use placeholders, not string interpolation, so logs stay searchable (this matters later for Splunk):
  - Yes: `_logger.LogInformation("Created team {TeamId}", team.Id);`
  - No: `_logger.LogInformation($"Created team {team.Id}");`

## 5. SQL Server

### Setup

- SQL Server runs locally in Docker
- The schema is managed with EF Core code-first migrations
- Every schema change is a migration. Never change the database by hand.
- Migration names describe the change: `AddTeamsTable`, `AddJerseyNumberToPlayers`

### Naming

| Thing | Style | Example |
|---|---|---|
| Tables | Plural, PascalCase | `Teams`, `Players` |
| Columns | PascalCase | `FirstName` |
| Primary key | `Id` | `Id` |
| Foreign key | Singular table name + Id | `TeamId` |
| Join tables | Both table names | `TeamPlayers` |

### Rules

- Every table has `CreatedAt` and `UpdatedAt`, stored in UTC
- All dates and times are stored in UTC
- Use database constraints, not just C# checks: NOT NULL for required columns, foreign keys always, unique constraints where needed (team name unique within a season)
- Add indexes on foreign keys and on columns that get searched often
- Default to EF Core and LINQ for queries. Use a stored procedure for heavier reporting queries. Standings will be the first one.

## 6. Testing

The goal: if a test suite is green, I trust the code. If I can't trust a green run, the tests need fixing before anything else.

### Tools

| Tool | Job |
|---|---|
| xUnit | C# test framework |
| Testcontainers | Starts a fresh SQL Server in Docker for integration tests, locally and in CI |
| Respawn | Wipes data between integration tests without rebuilding the database |
| Coverlet | Measures code coverage |
| `FakeTimeProvider` | Lets tests set and move the clock |
| Stryker.NET | Mutation testing (checks whether my tests actually catch bugs) |
| Vitest | Angular tests (Later) |

### What gets tested

- **Unit tests** cover business rules. Rules like standings math or roster limits live in plain classes so they can be tested without a database.
- **Integration tests** cover API endpoints from request to database, using `WebApplicationFactory` and a real SQL Server in Docker.
- Don't use the EF Core in-memory database for tests. It doesn't enforce constraints, can't run stored procedures, and runs queries differently than SQL Server, so tests can pass when the real thing would fail. (See `docs/decisions/0005-real-sql-server-for-tests.md`.)
- Every business rule and endpoint gets at least one success test and one failure test.
- Every acceptance criterion on a story gets at least one test that proves it.
- **Stored procedures** get integration tests against real SQL Server, including the awkward cases: no games played yet, ties, forfeits, a team with zero points.
- **Database constraints** get tested on purpose. Try to break them (duplicate team name in a season, a game with no field) and assert the database refuses.
- **Migrations**: CI applies every migration to an empty database from scratch on every PR.
- **Error responses** get tested too: check the status code and that the body is ProblemDetails.

### Test first

- For business rules, write the failing test before the code. Watch it fail, make it pass, then clean up.
- For bug fixes, write a test that reproduces the bug first and watch it fail. Then fix the bug. The test stays forever.

### Tests must be reliable

- No test depends on another test or on the order tests run in.
- Each integration test sets up its own data and doesn't rely on leftovers.
- No real clock: use `FakeTimeProvider`.
- No random values without a fixed seed.
- No `Thread.Sleep` or waiting on real timers.
- No real network calls in unit tests. Outside services get faked through their interface.
- A flaky test is a bug. Fix it or remove it the same day it shows up. Never rerun CI until it happens to pass.
- Never skip a test to get a PR merged. If a test is skipped, it has a comment linking an open issue.

### Test data

- Use test data builders (`TeamBuilder`, `PlayerBuilder`, `GameBuilder`) in the shared `TestData` project.
- Builders fill in sensible fake defaults, so each test only sets the values it actually cares about.
  Example: `new PlayerBuilder().WithBirthdate(cutoffDate.AddDays(1)).Build()`
- All names, birthdates, and contact info are fake (see section 3).

### How tests are written

- Test names follow `Method_Scenario_ExpectedResult`
  Example: `CalculateStandings_TiedOnPoints_SortsByGoalDifference`
- Each test is laid out as Arrange, Act, Assert
- One behavior per test. Several asserts are fine if they all check that one behavior.
- Test projects mirror the folder structure of the code they test
- The unit test suite stays fast (under about 10 seconds total) so I actually run it before every commit.

### Protecting athletes' data

- One integration test runs the main flows (register a player, add to a roster, record a game) with a logger that captures output, then asserts no fake player names, birthdates, or contact info appear in the logs.

### Coverage

Coverage is a warning light, not a goal. A high number with weak tests is worse than an honest lower one.

- Services and business rule classes: at least 90% line and branch coverage. CI fails below this.
- The whole API project: at least 80%. CI fails below this.
- Controllers get covered by integration tests, not by unit tests that mock everything.
- Don't write tests just to push the number up.

### Mutation testing

- Run Stryker.NET on the business rule classes once per sprint (not on every PR, it's slow).
- Target a mutation score of 80% or higher. Surviving mutants usually mean a missing test.
- Write the score in the sprint notes so I can see the trend.

### Automation

GitHub Actions runs these on every pull request. All must pass to merge:

1. Build with warnings as errors
2. `dotnet format --verify-no-changes` (formatting is consistent)
3. Apply all migrations to an empty database
4. Unit tests
5. Integration tests
6. Coverage thresholds

## 7. Agile workflow

- GitHub Projects board with columns: Backlog, Ready, In Progress, In Review, Done
- Two-week sprints
- Stories are written as: *As a [league organizer / coach / parent], I want [something] so that [reason]*
- Every story has acceptance criteria before it moves to Ready
- Acceptance criteria are written so each one can be turned into a test (specific inputs and expected results, not "works well")
- At the end of each sprint, a few lines of notes: what got done, what didn't, what I'd change, and the mutation score

## 8. Later sections

Filled in when I reach that part of the project.

- **Angular admin site**: folder structure, component and service naming, state handling, Vitest setup
- **Java scheduler**: build tool, project structure, how the C# API calls it, JUnit testing, contract tests so the C# API and Java service agree on request and response shapes
- **Mobile app**: Swift or Kotlin, how it talks to the API, offline score entry
- **AWS**: which services, how deployment works, where secrets live
- **Splunk / monitoring**: what gets logged, dashboards, alerts

## 9. Decision log

Big decisions get a short record in `docs/decisions/`, named like `0001-one-repo.md`. Each one answers three things: what I decided, why, and what else I considered. These also make good interview talking points.

Decisions made so far:

1. One repo with a folder for each piece
2. Layered C# API (Controller → Service → Data) instead of Clean Architecture, to keep the structure simple while I learn C#
3. xUnit for C# tests, Vitest for Angular
4. .NET 10 (long-term support)
5. Integration tests run against real SQL Server in Docker, not the EF Core in-memory database
6. Testcontainers and Respawn for integration test databases, so every run starts clean
7. `TimeProvider` instead of `DateTime.Now`, so date-based rules can be tested
8. Coverage thresholds (90% business rules, 80% overall) enforced in CI, with mutation testing once per sprint
