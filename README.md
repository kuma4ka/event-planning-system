# 📅 Event Planning System

[![Build & Test](https://github.com/kuma4ka/EventPlanningSystem/actions/workflows/ci.yml/badge.svg)](https://github.com/kuma4ka/EventPlanningSystem/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![Docker](https://img.shields.io/badge/Docker-Enabled-blue)
![License](https://img.shields.io/badge/License-MIT-green)

A modern, scalable web application for managing events, venues, and guest lists. Built with **.NET 10** following **Clean Architecture** principles.

---

## 🚀 Features

* **Event Management:** Create, edit, and delete events with rich descriptions.
* **Venue Integration:** Select venues from a predefined list (seeded via DbInitializer).
* **Guest Management:** Invite guests to specific events and manage the guest list.
* **Secure Authentication:** Full registration and login system using ASP.NET Core Identity.
* **Data Security:** User Secrets for local development, preventing credential leaks.
* **Validation:** Robust server-side validation using FluentValidation.

## 🏗️ Architecture & Tech Stack

This project follows **Clean Architecture** to ensure separation of concerns and testability:

* **Core (Domain):** Pure business entities and interfaces. No external dependencies.
* **Application:** Business logic, DTOs, Services, and Validators (FluentValidation).
* **Infrastructure:** EF Core implementation, SQL Server, Identity, and Repositories.
* **Web:** ASP.NET Core MVC (UI Layer).

**Technologies:**
* **.NET 10 (LTS)**
* **Entity Framework Core** (Code-First)
* **SQL Server** (running via Docker)
* **XUnit & Moq** (Unit Testing)
* **Docker** (Containerization)
* **Bootstrap 5** (UI Styling)

## 🛠️ Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Installation

1.  **Clone the repository**
    ```bash
    git clone https://github.com/kuma4ka/EventPlanningSystem.git
    cd EventPlanningSystem
    ```

2.  **Start SQL Server (Docker)**
    ```bash
    docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 -d --name sql_server mcr.microsoft.com/mssql/server:2022-latest
    ```

3.  **Configure Secrets**
    Set up the connection string and admin credentials securely:
    ```bash
    dotnet user-secrets init --project src/EventPlanning.Web/EventPlanning.Web.csproj
    
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=EventPlanningDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;" --project src/EventPlanning.Web/EventPlanning.Web.csproj
    
    dotnet user-secrets set "Seed:AdminEmail" "admin@eventplanning.com" --project src/EventPlanning.Web/EventPlanning.Web.csproj
    dotnet user-secrets set "Seed:AdminPassword" "SuperS3cure!2025" --project src/EventPlanning.Web/EventPlanning.Web.csproj
    ```

4.  **Run the Application**
    ```bash
    dotnet run --project src/EventPlanning.Web
    ```
    *The application will automatically apply migrations and seed initial data.*

5.  **Access the App**
    Open `http://localhost:5000` (or the port shown in your terminal).

## 🧪 Running Tests

The project includes Unit Tests for the Application layer logic.

```bash
dotnet test