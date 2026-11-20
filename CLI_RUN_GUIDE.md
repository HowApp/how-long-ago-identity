# How Long Ago (`how_app`) — CLI Setup & Run Guide

**How Long Ago** is a .NET 8 microservices application for tracking events, live elapsed time counters, chronological occurrence records, image galleries, likes/dislikes, bookmarks ("Saved"), and shared collaborator access.

---

## 1. Architecture & Ports

| Service | Repository | URL(s) | Purpose |
| :--- | :--- | :--- | :--- |
| **`HowCommon`** | `how-long-ago-common` | Local NuGet (`.local-nuget`) | Shared contracts, MassTransit messages, certificate manager, helpers |
| **`HowIdentity`** | `how-long-ago-identity` | `https://localhost:5001` | Duende IdentityServer 7, ASP.NET Core Identity, SuperAdmin portal |
| **`How.Server`** | `how-long-ago` | `https://localhost:7060` (REST/SignalR)<br>`https://localhost:7061` (mTLS gRPC) | Main CQRS API, Dapper/EF Core PostgreSQL, WebP Image Worker |
| **`HowClient`** | `how-long-ago-client` | `https://localhost:7560`<br>`http://localhost:5560` | Blazor WebAssembly SPA Frontend |
| **Artifacts** | `how-artifacts` | N/A | Local `dotnet user-secrets` JSON templates |

---

## 2. Prerequisites

Make sure the following are installed and running on your machine:

1. **.NET 8 SDK** (`dotnet --list-sdks`)
2. **PostgreSQL** listening on `localhost:5432`
3. **RabbitMQ** listening on `localhost:5672` (Management UI on `http://localhost:15672`)

### Quick Infrastructure Check (CLI)
```bash
ss -tulpn | grep -E '5432|5672'
```

---

## 3. First-Time Setup from CLI

Run all commands below from the workspace root (`/home/mykhailo/dev/pet/how_app`):

### Step 3.1: Pack the Shared `HowCommon` NuGet Package Locally
```bash
dotnet pack how-long-ago-common/HowCommon.csproj -c Release -o .local-nuget
dotnet pack how-long-ago-common/HowCommon.csproj -c Release -p:PackageVersion=1.0.7 -o .local-nuget
```

### Step 3.2: Load Development `user-secrets` for Identity & Main API
```bash
cat how-artifacts/how-identity.json | dotnet user-secrets set --project how-long-ago-identity/HowIdentity/HowIdentity.csproj
cat how-artifacts/how-api.json | dotnet user-secrets set --project how-long-ago/Server/How.Server.csproj
```

> **Note**: If your local RabbitMQ uses the default `guest` / `guest` credentials instead of `admin` / `admin`, override them in `user-secrets`:
> ```bash
> dotnet user-secrets set "RabbitMqConfiguration:User" "guest" --project how-long-ago-identity/HowIdentity/HowIdentity.csproj
> dotnet user-secrets set "RabbitMqConfiguration:Password" "guest" --project how-long-ago-identity/HowIdentity/HowIdentity.csproj
> dotnet user-secrets set "RabbitMqConfiguration:User" "guest" --project how-long-ago/Server/How.Server.csproj
> dotnet user-secrets set "RabbitMqConfiguration:Password" "guest" --project how-long-ago/Server/How.Server.csproj
> ```

### Step 3.3: Ensure ASP.NET Core HTTPS Dev Certificate Exists
```bash
dotnet dev-certs https --check || dotnet dev-certs https
```

---

## 4. Build All Projects from CLI

```bash
dotnet build how-long-ago-common/HowCommon.csproj
dotnet build how-long-ago-identity/HowIdentity/HowIdentity.csproj
dotnet build how-long-ago/How.sln
dotnet build how-long-ago-client/HowClient.sln
```

---

## 5. Run All Services from CLI

Open **3 terminals** (or run them in the background) from `/home/mykhailo/dev/pet/how_app`:

### Terminal 1 — Identity Provider (`https://localhost:5001`)
> **Important**: Start `HowIdentity` first! On startup, it automatically creates/migrates the `how-identity` PostgreSQL database, seeds the `SuperAdmin` account (`SuperAdmin` / `String1!`), and generates the shared mTLS `.pfx` certificate in `~/dev-cert/HowApp/how_app_microservices.pfx`.

```bash
dotnet run --project how-long-ago-identity/HowIdentity/HowIdentity.csproj
```

### Terminal 2 — Main Backend API & gRPC Server (`https://localhost:7060` & `7061`)
> On startup, `How.Server` automatically creates/migrates the `how-dev-temp` database (`public` and `temporary` schemas) and seeds the SuperAdmin user record.

```bash
dotnet run --project how-long-ago/Server/How.Server.csproj
```

### Terminal 3 — Blazor WebAssembly Client (`https://localhost:7560`)
```bash
dotnet run --project how-long-ago-client/HowClient.csproj
```

---

## 6. Default Credentials & Useful Endpoints

- **Web App**: `https://localhost:7560`
- **Identity Portal**: `https://localhost:5001`
  - **SuperAdmin Username**: `SuperAdmin` (Email: `bobr@example.com`)
  - **SuperAdmin Password**: `String1!`
- **Main API Swagger UI**: `https://localhost:7060/swagger`
- **OIDC Discovery Document**: `https://localhost:5001/.well-known/openid-configuration`

---

## 7. Stopping All Services from CLI

If running in background terminals or if ports `5001`, `7060`, `7061`, or `7560` are occupied:

```bash
pkill -f "HowIdentity" || true
pkill -f "How.Server" || true
pkill -f "HowClient" || true
```
