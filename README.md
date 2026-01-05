# 📅 Stanza - Event Planning System

[![.NET 10 Build & Test](https://github.com/kuma4ka/event-planning-system/actions/workflows/ci.yml/badge.svg)](https://github.com/kuma4ka/event-planning-system/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![Docker](https://img.shields.io/badge/Docker-Enabled-blue)
![License](https://img.shields.io/badge/License-MIT-green)

**Stanza** is a modern, production-ready web application for managing events, venues, and guest lists. Built with **.NET 10** and following **Clean Architecture** principles, it demonstrates how to build scalable, maintainable, and secure ASP.NET Core applications.

---

## 🚀 Key Features

*   **Event Management**: Create, edit, and delete events with rich descriptions, date validation, and venue allocation.
*   **Venue Integration**: Select venues from a supported list (seeded automatically in Dev).
*   **Guest Management**: Invite guests to events, track participation, and manage guest profiles.
*   **Secure Authentication**: Full registration and login system using ASP.NET Core Identity.
*   **Performance**:
    *   **Caching**: `CachedEventService` uses the Decorator pattern to cache event reads using `IMemoryCache`.
    *   **Rate Limiting**: Protects content creation endpoints against abuse.
*   **Production Ready**:
    *   **Data Protection**: Persisted keys for resilient container deployments.
    *   **Secrets Management**: Secure handling of credentials via Environment Variables.
    *   **Health Checks**: Built-in probes for App and Database health.

## 🛠️ Tech Stack

*   **Backend**: .NET 10 (LTS), Entity Framework Core, SQL Server 2022.
*   **Frontend**: ASP.NET Core MVC, Bootstrap 5, jQuery Validation.
*   **Testing**: xUnit, Moq, **Testcontainers** (for true integration testing).
*   **DevOps**: Docker, Docker Compose, GitHub Actions (CI/CD).

## 🐳 Getting Started

The easiest way to run the application is using Docker Compose. This spins up both the Web App and the SQL Server with zero configuration.

### Option A: Docker Compose (Recommended)

1.  **Clone the repository**
    ```bash
    git clone https://github.com/kuma4ka/event-planning-system.git
    cd event-planning-system
    ```

2.  **Run with Compose**
    ```bash
    docker-compose up -d
    ```

3.  **Access the App**
    *   Open `http://localhost:8080` in your browser.
    *   The app will automatically apply migrations and seed valid sample data (since `ASPNETCORE_ENVIRONMENT=Development` is default in compose for local runs, or if set to Production, seeding is skipped).

### Option B: Manual Development (.NET CLI)

If you prefer to run the app on your host machine (e.g., for debugging):

1.  **Start SQL Server**: You still need a database. Run the database container:
    ```bash
    docker-compose up -d db
    ```

2.  **Configure Secrets**:
    ```bash
    cd src/EventPlanning.Web
    dotnet user-secrets init
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=EventPlanningDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
    dotnet user-secrets set "Seed:AdminEmail" "admin@stanza.com"
    dotnet user-secrets set "Seed:AdminPassword" "Admin123!"
    dotnet user-secrets set "Seed:OrganizerEmail" "organizer@stanza.com"
    dotnet user-secrets set "Seed:OrganizerPassword" "Organizer123!"
    ```

3.  **Run the App**:
    ```bash
    dotnet run
    ```
    Access at `https://localhost:7073`.

## ⚙️ Configuration & Production

The application is designed to be configurable via **Environment Variables**, making it compatible with Docker, Kubernetes, and Cloud PasS (Azure App Service).

### Production Settings
The project includes an `appsettings.Production.json` file which acts as a template. In a real production environment, **DO NOT** edit this file with real secrets. Instead, override the values using Environment Variables:

| Variable Name | Description | Example Value |
| :--- | :--- | :--- |
| `ConnectionStrings__DefaultConnection` | SQL Server Connection String | `Server=prod-db;Database=...` |
| `Seed__AdminEmail` | Initial Admin Email | `admin@stanza.com` |
| `Seed__AdminPassword` | Initial Admin Password | `ComplexPassword!23` |
| `EmailSettings__Server` | SMTP Server Host | `smtp.gmail.com` |
| `EmailSettings__Port` | SMTP Port | `587` |
| `EmailSettings__SenderName` | Display Name for Emails | `Stanza Team` |
| `EmailSettings__SenderEmail` | Email Address for Outgoing Mail | `notifications@stanza.com` |
| `EmailSettings__Username` | SMTP Username | `notifications@stanza.com` |
| `EmailSettings__Password` | SMTP Password/Key | `xyz-api-key` |
| `EmailSettings__BaseUrl` | Public URL (for links in emails) | `https://stanza-events.com` |
| `GoogleMaps__ApiKey` | Google Maps Embed API Key | `AIzaSy...` |
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` | `Production` |

> [!IMPORTANT]
> When `ASPNETCORE_ENVIRONMENT` is set to `Production`, the automatic **Data Seeding** (Venues/Events) is **DISABLED** to prevent overwriting your production data. Only essential Roles and System Users are ensured.

## 🧪 Running Tests

The solution includes both Unit Tests (logical isolation) and Integration Tests (real database interaction).

### Unit Tests
```bash
dotnet test tests/EventPlanning.Tests/EventPlanning.Tests.csproj
```

### Integration Tests
These tests use **Testcontainers** to spin up a real SQL Server in Docker for the duration of the tests. **Docker must be running.**
```bash
dotnet test tests/EventPlanning.IntegrationTests/EventPlanning.IntegrationTests.csproj
```

## 📂 Project Structure

The solution follows **Clean Architecture**:

*   `src/EventPlanning.Domain`: **Core**. Entities, Enums, Interfaces. No dependencies.
*   `src/EventPlanning.Application`: **Logic**. Services, DTOs, Validators. Depends on Domain.
*   `src/EventPlanning.Infrastructure`: **Implementation**. EF Core, Repositories, Identity, Email. Depends on Application.
*   `src/EventPlanning.Web`: **UI**. MVC Controllers, Views, `Program.cs`. Depends on Application & Infrastructure.

---