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
  - [Dentists](#dentists)
  - [Patients](#patients)
  - [Services](#services)
  - [Availability](#availability)
  - [Appointments](#appointments)
- [Validation Rules](#validation-rules)
- [CORS](#cors)
- [Domain Events](#domain-events)
- [Testing](#testing)

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
- **Dentist** `{ Id: Guid, FullName: string }` — methods: `Rename(...)`
- **Patient** `{ Id: Guid, FullName: string, Email: string }` — methods: `Rename(...)`, `ChangeEmail(...)`, `Update(...)`
- **Service** `{ Id: Guid, Name: string, DurationMinutes: int }` — methods: `Rename(...)`, `ChangeDuration(...)`, `Update(...)`
- **Appointment** `{ Id, DentistId, PatientId, ServiceId, StartUtc (DateTimeOffset), DurationMinutes, Status }`  
  - `Status`: `Scheduled` | `Cancelled`  
  - `EndUtc` = `StartUtc + DurationMinutes`  
  - Methods: `Cancel(reason)`, `Reschedule(newStartUtc)`

**Domain Events**
- `AppointmentBookedEvent(AppointmentId, DentistId, PatientId, StartUtc, DurationMinutes)`  
- `AppointmentRescheduledEvent(AppointmentId, NewStartUtc)`  
- `AppointmentCancelledEvent(AppointmentId, Reason)`

**Business rules**
- Clinic hours 09:00–17:00 **UTC**.  
- Start times align to **15-minute grid**.  
- **No overlapping** appointments for the same dentist (enforced in domain logic).

---

## Project Structure

```
  Domain/
    Appointments/Appointment.cs (+ Events/)
    Dentists/Dentist.cs
    Patients/Patient.cs
    Services/Service.cs
    Common/BaseEntity.cs, BaseEvent.cs
  Application/
    Common/Behaviors/ValidationBehavior.cs
    DTOs/AppointmentListItemDto.cs, DentistDto.cs, PatientDto.cs, ServiceDto.cs
    Features/Appointments/
      BookAppointment.cs
      CancelAppointment.cs
      RescheduleAppointment.cs
      GetAvailability.cs            // filters past slots when day = today
      ListAppointmentsByDentist.cs  // projects patient/service names
    Features/Dentists/DentistsCrud.cs
    Features/Patients/PatientsCrud.cs
    Features/Services/ServicesCrud.cs
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
      DentistsController.cs
      PatientsController.cs
      ServicesController.cs
    Program.cs (Swagger, CORS, DI, Auto-Migrate)
  UnitTests/
    BookAppointmentValidatorTests.cs
    RescheduleOverlapTests.cs
docker-compose.yml
```

---

## Getting Started

### Run with Docker Compose (API + SQL Server)

1) Create `.env` at repo root:
```
MSSQL_SA_PASSWORD=N0vaDent-Clinic#2025+SQL
```

2) `docker-compose.yml`:
```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: dentist-db
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "${MSSQL_SA_PASSWORD}"
    ports:
      - "1433:1433"
    volumes:
      - mssqldata:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '${MSSQL_SA_PASSWORD}' -No -Q 'SELECT 1' -C"]
      interval: 10s
      timeout: 3s
      retries: 10

  api:
    image: ${DOCKER_REGISTRY-}api
    build:
      context: .
      dockerfile: Api/Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__Default: "Server=db,1433;Database=DentistBooking;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=true;"
    ports:
      - "8080:8080"
    depends_on:
      db:
        condition: service_healthy

volumes:
  mssqldata:
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

2) Configure **`Api/appsettings.json`**:
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

---

## Configuration

**`appsettings.json`**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=DentistBooking;User Id=sa;Password=N0vaDent-Clinic#2025+SQL;TrustServerCertificate=true;Encrypt=false"
  }
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

**Auto-migrate on startup**
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
```

---

## API Reference

Base URL: `http://localhost:8080/api` (Swagger at `/swagger`).

### Dentists

`GET /dentists` → `200 OK`
```json
[ { "id": "guid", "fullName": "Dr. Jane Doe" } ]
```

`POST /dentists` → `200 OK` (returns new id)
```json
{ "fullName": "Dr. Jane Doe" }
```

`PUT /dentists/{id}` → `204 No Content`
```json
{ "fullName": "Dr. Jane Smith" }
```

`DELETE /dentists/{id}` → `204 No Content`

---

### Patients

`GET /patients` → `200 OK`
```json
[ { "id": "guid", "fullName": "John Roe", "email": "john@example.com" } ]
```

`POST /patients` → `200 OK` (returns new id)
```json
{ "fullName": "John Roe", "email": "john@example.com" }
```

`PUT /patients/{id}` → `204 No Content`
```json
{ "fullName": "John Roe", "email": "john.roe@example.com" }
```

`DELETE /patients/{id}` → `204 No Content`

---

### Services

`GET /services` → `200 OK`
```json
[ { "id": "guid", "name": "Cleaning", "durationMinutes": 30 } ]
```

`POST /services` → `200 OK` (returns new id)
```json
{ "name": "Cleaning", "durationMinutes": 30 }
```

`PUT /services/{id}` → `204 No Content`
```json
{ "name": "Deep Cleaning", "durationMinutes": 45 }
```

`DELETE /services/{id}` → `204 No Content`

---

### Availability

`GET /availability?dentistId={guid}&date=YYYY-MM-DD` → `200 OK`  
> **Only future slots are returned** if the date equals “today” in UTC.

```json
[
  { "startUtc": "2025-09-01T09:00:00Z", "endUtc": "2025-09-01T09:15:00Z" },
  { "startUtc": "2025-09-01T09:15:00Z", "endUtc": "2025-09-01T09:30:00Z" }
]
```

---

### Appointments

**Book**
`POST /appointments/book` → `200 OK` (returns appointment id)
```json
{
  "dentistId": "11111111-1111-1111-1111-111111111111",
  "patientId": "22222222-2222-2222-2222-222222222222",
  "serviceId": "33333333-3333-3333-3333-333333333333",
  "startUtc": "2025-09-01T09:00:00Z"
}
```

**List by dentist & day** (returns names)
`GET /appointments/by-dentist?dentistId={guid}&date=YYYY-MM-DD` → `200 OK`
```json
[
  {
    "id": "guid",
    "startUtc": "2025-09-01T09:00:00Z",
    "durationMinutes": 30,
    "status": 0,
    "patientId": "guid",
    "patientName": "John Roe",
    "serviceId": "guid",
    "serviceName": "Cleaning"
  }
]
```

**Reschedule**
`POST /appointments/{id}/reschedule` → `204 No Content`
```json
{ "newStartUtc": "2025-09-01T09:30:00Z" }
```

**Cancel**
`POST /appointments/{id}/cancel` → `204 No Content`
```json
{ "reason": "Schedule change" }
```

---

## Validation Rules

- IDs must be non-empty GUIDs.  
- Times must be **future**, on **15-minute grid**, within **09:00–17:00 UTC**.  
- No dentist overlap (domain invariant).

Implemented with **FluentValidation** in each Command/Query; invariants also enforced in the `Appointment` aggregate.

---

## CORS

Dev “allow all”:
```csharp
builder.Services.AddCors(o => o.AddPolicy("AllowAll",
  p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
app.UseCors("AllowAll");
```

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