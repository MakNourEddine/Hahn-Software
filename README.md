# Dentist Booking — Backend (ASP.NET Core 9, Clean Architecture, DDD, CQRS)

A small yet realistic API demonstrating **Clean Architecture** (Domain / Application / Infrastructure / API), **CQRS** with MediatR, **EF Core** (SQL Server) + migrations, **FluentValidation**, **domain events**, **Swagger/OpenAPI**, **unit tests**, and **Docker Compose** (API + DB).

---

## Table of Contents

- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Domain Model](#domain-model)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Run with Docker Compose (API + SQL Server)](#run-with-docker-compose-api--sql-server)
  - [Run API from Visual Studio + SQL Server in Docker](#run-api-from-visual-studio--sql-server-in-docker)
  - [Database Migrations](#database-migrations)
- [Configuration](#configuration)
- [API Reference](#api-reference)
- [Validation Rules](#validation-rules)
- [CORS](#cors)
- [Domain Events](#domain-events)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)
- [Production Notes](#production-notes)

---

## Tech Stack

- **.NET 9** / ASP.NET Core Web API  
- **Clean Architecture** (Domain / Application / Infrastructure / API)  
- **CQRS** with **MediatR**  
- **EF Core** (SQL Server) + **Migrations**  
- **FluentValidation**  
- **Domain Events** bridged to MediatR notifications  
- **Swagger/OpenAPI** via Swashbuckle  
- **xUnit** + FluentAssertions (unit tests)  
- **Docker Compose** (API + SQL Server 2022)  
- Optional: **Serilog** for structured logging

---

## Architecture

**Layers**

- **Domain** – Entities, value objects, enums, domain events, business rules. No dependencies on EF/web.
- **Application** – Use-cases as **Commands/Queries** (MediatR). Orchestrates domain, validation, and transactions.
- **Infrastructure** – EF Core `DbContext`, entity configurations, repositories, domain-event dispatcher, external services.
- **API** – Controllers, DI setup, Swagger, CORS. No business logic.

**Request flow (example: Book Appointment)**

`POST /api/appointments/book` → Controller creates `BookAppointment.Command` → MediatR Handler:
1. Validates input (FluentValidation)  
2. Loads aggregates (Dentist, Patient, Service)  
3. Instantiates `Appointment` (enforces invariants)  
4. Saves via EF Core (transaction)  
5. Emits `AppointmentBookedEvent` → handled as notification for side effects/logging

---

## Domain Model

**Entities**
- **Dentist** `{ Id: Guid, FullName: string }`
- **Patient** `{ Id: Guid, FullName: string, Email: string }`
- **Service** `{ Id: Guid, Name: string, DurationMinutes: int }`
- **Appointment** `{ Id, DentistId, PatientId, ServiceId, StartUtc, DurationMinutes, Status }`  
  - `Status`: `Scheduled` | `Cancelled`  
  - `EndUtc` = `StartUtc + DurationMinutes`  
  - Methods: `Cancel(reason)`, `Reschedule(newStartUtc)`

**Domain Events**
- `AppointmentBookedEvent(AppointmentId, DentistId, PatientId, StartUtc, DurationMinutes)`  
- `AppointmentRescheduledEvent(AppointmentId, NewStartUtc)`  
- `AppointmentCancelledEvent(AppointmentId, Reason)`

**Business rules**
- Clinic hours 09:00–17:00 **UTC** (configurable).  
- Start times must align to **15-minute grid**.  
- No overlapping appointments for the same dentist.

---

## Project Structure

```
src/
  Domain/
    Appointments/Appointment.cs (+ Events/)
    Dentists/Dentist.cs
    Patients/Patient.cs
    Services/Service.cs
    Common/BaseEntity.cs, BaseEvent.cs
  Application/
    Common/Behaviors/ValidationBehavior.cs
    Features/Appointments/
      BookAppointment.cs
      CancelAppointment.cs
      RescheduleAppointment.cs
      GetAvailability.cs
      ListByDentist.cs
  Infrastructure/
    Persistence/
      ApplicationDbContext.cs
      Configurations/*.cs
    Events/DomainEventDispatcher.cs
    DependencyInjection.cs
  Api/
    Controllers/
      AppointmentsController.cs
      AvailabilityController.cs
    Program.cs (Swagger, CORS, DI, Auto-Migrate)
  Tests/
    UnitTests/
      Domain/*
      Application/*
docker-compose.yml
```

---

## Getting Started

### Run with Docker Compose (API + SQL Server)

1) Create `.env` at repo root:
```
MSSQL_SA_PASSWORD=N0vaDent-Clinic#2025+SQL
```

2) `docker-compose.yml` (summary):
```yaml
version: "3.9"
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "${MSSQL_SA_PASSWORD}"
    ports: ["1433:1433"]
    volumes: ["mssqldata:/var/opt/mssql"]
  api:
    build: .
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__Default: "Server=db,1433;Database=DentistBooking;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=true;"
    ports: ["8080:8080"]
    depends_on: ["db"]
volumes: { mssqldata: {} }
```

3) Run:
```bash
docker compose up --build
```
Swagger → `http://localhost:8080/swagger`

### Run API from Visual Studio + SQL Server in Docker

1) Start SQL Server in Docker:
```powershell
docker run -d --name sqlserver `
  -e "ACCEPT_EULA=Y" `
  -e "MSSQL_SA_PASSWORD=N0vaDent-Clinic#2025+SQL" `
  -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

2) Configure **`Api/appsettings.Development.json`**:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=DentistBooking;User Id=sa;Password=N0vaDent-Clinic#2025+SQL;TrustServerCertificate=true;Encrypt=false"
  }
}
```

3) Apply migrations (PMC: Startup=Api, Default project=Infrastructure):
```powershell
Update-Database -Project Infrastructure -StartupProject Api
```

4) F5 the API → `/swagger`

### Database Migrations

Create/update migration:
```powershell
Add-Migration InitialCreate -Project Infrastructure -StartupProject Api
Update-Database -Project Infrastructure -StartupProject Api
```

> Seed with **fixed GUIDs** (no `Guid.NewGuid()` in `HasData`) to keep migrations deterministic.

---

## Configuration

**`appsettings.Development.json`**
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=DentistBooking;User Id=sa;Password=YOUR_PWD;TrustServerCertificate=true;Encrypt=false"
  },
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "AllowedHosts": "*"
}
```

**CORS (dev) in `Program.cs`**
```csharp
builder.Services.AddCors(o => o.AddPolicy("AllowAll",
  p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
app.UseCors("AllowAll");
```

**Swagger schema ids (avoid nested type collisions)**
```csharp
builder.Services.AddSwaggerGen(o =>
  o.CustomSchemaIds(t => (t.FullName ?? t.Name).Replace("+",".")));
```

**Auto-migrate on startup (optional)**
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    for (var i = 1; i <= 5; i++)
    {
        try { await db.Database.MigrateAsync(); break; }
        catch when (i < 5) { await Task.Delay(2000 * i); }
    }
}
```

---

## API Reference

Base URL: `http://localhost:8080/api` (Swagger at `/swagger`).

### Availability
`GET /availability?dentistId={guid}&date=YYYY-MM-DD`  
**200**
```json
[ { "startUtc": "2025-09-01T09:00:00Z", "endUtc": "2025-09-01T09:15:00Z" } ]
```

### Book appointment
`POST /appointments/book`  
```json
{
  "dentistId": "11111111-1111-1111-1111-111111111111",
  "patientId": "22222222-2222-2222-2222-222222222222",
  "serviceId": "33333333-3333-3333-3333-333333333333",
  "startUtc": "2025-09-01T09:00:00Z"
}
```
**200**: `"<appointmentId-guid>"`

### List by dentist & day
`GET /appointments/by-dentist?dentistId={guid}&date=YYYY-MM-DD`  
**200**
```json
[ { "id":"...", "startUtc":"...", "durationMinutes":30, "status":0, "patientId":"...", "serviceId":"..." } ]
```

### Reschedule
`POST /appointments/{id}/reschedule`  
```json
{ "newStartUtc": "2025-09-01T09:30:00Z" }
```
**204**

### Cancel
`POST /appointments/{id}/cancel`  
```json
{ "reason": "Schedule change" }
```
**204**

---

## Validation Rules

- IDs must be non-empty GUIDs.  
- Times must be **future**, on **15-minute grid**, within **09:00–17:00 UTC**.  
- No dentist overlap.

Implemented with **FluentValidation** in each Command/Query; invariants also enforced in the `Appointment` aggregate.

---

## CORS

Dev “allow all”:
```csharp
builder.Services.AddCors(o => o.AddPolicy("AllowAll",
  p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
app.UseCors("AllowAll");
```
> For cookies/credentials use `.SetIsOriginAllowed(_ => true).AllowCredentials()` instead (don’t mix with `AllowAnyOrigin`).

---

## Domain Events

`BaseEntity` carries a `List<BaseEvent>`; `ApplicationDbContext` ignores it for EF.  
Infrastructure publishes domain events to **MediatR** after `SaveChanges` so handlers can perform side effects (logging, notifications, etc.).

---

## Testing

- **Domain**: appointment rules (overlap, reschedule, cancel).  
- **Validators**: command/query validation.  
- **Handlers**: happy paths and conflict cases.

Run:
```bash
dotnet test
```

For integration tests, use `WebApplicationFactory<Api.Program>` with **SQLite in-memory** or **Testcontainers** for SQL Server.

---

## Troubleshooting

- **TCP Provider, error: 40** – Ensure SQL container is running and port mapping (1433) matches your connection string (`Server=localhost,1433`).  
- **PendingModelChangesWarning** – Remove dynamic values from `HasData` (use fixed GUIDs/DateTimes).  
- **`BaseEvent` requires a primary key** – `modelBuilder.Ignore<BaseEvent>();` and `b.Ignore(x => x.DomainEvents);` in configurations.  
- **Swagger schema collisions for nested `…+Command`** – Use `CustomSchemaIds` (see above).  
- **CORS blocked** – Ensure `UseCors` is before `MapControllers` and policy matches your frontend origin(s).

---

## Production Notes

- Lock down CORS to allowed origins.  
- Use HTTPS (remove `Encrypt=false`); configure a real certificate.  
- Add **Serilog** + sinks (Seq/ELK).  
- Add **health checks** and Docker `HEALTHCHECK`.  
- Run migrations as a deployment step rather than on each startup.  
- Store secrets in a secret manager (Key Vault, AWS SM, etc.).

---

## Seeded IDs (for quick testing)

- Dentist: `11111111-1111-1111-1111-111111111111`  
- Patient: `22222222-2222-2222-2222-222222222222`  
- Service (Cleaning): `33333333-3333-3333-3333-333333333333`

Use these in Swagger or the React frontend to book and list appointments.
