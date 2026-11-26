# league-manager
Sports league app

## Running locally

### What you need

- .NET 10 SDK
- Node.js 24
- Docker Desktop (on Apple Silicon, turn on "Use Rosetta" in Docker's settings)

### 1. Start SQL Server

The first time, create the container:

```bash
docker run -d --name league-sql --platform linux/amd64 \
  -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=<your password>' \
  -p 1433:1433 mcr.microsoft.com/mssql/server:2025-latest
```

After that, start it with `docker start league-sql`.

### 2. Run the API

From `api/`, the first time:

```bash
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:LeagueManager" "Server=localhost,1433;Database=LeagueManager;User Id=sa;Password=<your password>;TrustServerCertificate=True" --project src/LeagueManager.Api
dotnet ef database update --project src/LeagueManager.Api
```

Then:

```bash
dotnet run --project src/LeagueManager.Api
```

The API runs at `http://localhost:5179`.

### 3. Run the Angular app

In a second terminal, from `web/`:

```bash
npm install
npm start
```

The site runs at `http://localhost:4200`.

### Running the tests

- API: `dotnet test` from `api/` (Docker must be running)
- Angular: `npm test` from `web/`