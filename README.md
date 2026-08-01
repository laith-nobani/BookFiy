# BookFiy

BookFiy is a **multi-tenant SaaS booking/appointment platform** (built with ASP.NET Core, EF Core, and ASP.NET Identity). Each business (tenant) manages its own employees, services, and bookings, isolated from other tenants.

## Core Business

Users book time slots with a specific **Service** (with a duration and price) within a **Tenant** (a business/organization). The system prevents double-booking, tracks booking status, and keeps an audit trail of changes.

## Main Features


## Clean Architecture

The project is split into layers. Each layer only depends on the layer(s) inside it, never the other way around.

Domain – The core. Entities (Tenant, Employee, Service, Booking, Otp, RefreshToken, ApplicationUser) and repository interfaces. No dependency on anything else.

Application – Business logic. Services (BookingService, EmployeeService, TenantService, OtpService, EmailService) that use the Domain interfaces, plus DTOs and validators. Depends on Domain.

Infrastructure – Implements the Domain's repository interfaces using EF Core. Contains the DbContext, entity configurations, database migrations, seed data, and external service implementations such as Redis caching and email services. Depends on the Domain and Application layers.

API – Controllers and middleware. The entry point that receives HTTP requests and calls Application services. Depends on Application.

Tests – xUnit tests that call Application services directly, using mocked repositories.

## API Overview

###  Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/register/confirm`
- `POST /api/auth/otp/resend`
- `POST /api/auth/token/refresh`
- `POST /api/auth/logout`

---

###  Admin
- `GET /api/admin`
- `POST /api/admin`
- `PUT /api/admin/{id}`

---

###  Tenants
- `GET /api/tenant`
- `GET /api/tenant/{slug}`
- `POST /api/tenant`

---

###  Employees
- `POST /api/employee`
- `GET /api/employee`
- `PUT /api/employee/{id}`
- `DELETE /api/employee/{id}`

---

###  Services
- `POST /api/services`
- `GET /api/services`
- `GET /api/services/employee/{id}`
- `PUT /api/services/{id}`
- `DELETE /api/services/{id}`

---

###  Bookings
- `POST /api/booking`
- `GET /api/booking/user/{id}`
- `GET /api/booking/employee/{id}`
- `PUT /api/booking/{id}`
- `DELETE /api/booking/{id}`



### 1. Multi-Tenancy
- Every `ApplicationUser`, `Employee`, `Service`, and `Booking` is scoped by `TenantId`.
- `ITenantProvider` / `TenantProvider` holds the "current tenant" for a request (resolved via middleware, e.g. from subdomain, header, or JWT claim), and every repository/service query filters by it.
- `TenantService` manages tenant CRUD, with a unique `Slug` per tenant.

### 2. Roles & Authorization
- Built on **ASP.NET Identity** (`ApplicationUser : IdentityUser<Guid>`, `RoleManager<IdentityRole<Guid>>`).
- Roles: `SuperAdmin`, `Admin`, `Employee`, `Customer` (seeded at startup via `RoleSeeder`/`DbSeeder`).
- Endpoints are protected with `[Authorize(Roles = "...")]`, plus custom middleware to resolve/inject the tenant context before authorization runs.
- Soft-delete pattern (`IsDeleted` flag) instead of hard deletes for users/employees.

### 3. Authentication (Login/Register + OTP + JWT Refresh Tokens)
- **Register**: creates an `ApplicationUser` (Identity) with a temporary/generated password, assigns a role, sends credentials by email.
- **OTP verification** (`OtpService`):
  - Generates a 6-digit numeric code using `RandomNumberGenerator` (cryptographically secure).
  - Stores only a **SHA-256 hash** of the code (never the raw code) with an expiry (`ExpiresAt`) and `IsUsed` flag.
  - `VerifyOtpAsync` fetches the latest non-expired OTP, hashes the input, compares, and marks it used (single-use).
- **Email delivery** (`EmailService`): sends OTPs and temporary passwords via SMTP (MailKit/MimeKit), configurable Host/Port/SSL via `SmtpSettings`.
- **JWT + Refresh Tokens**: `RefreshToken` entity/table persists refresh tokens so access tokens can be short-lived and silently renewed without forcing re-login; refresh tokens are revocable/rotatable server-side.

### 4. Employee Management
- `EmployeeService` creates the Identity user + `Employee` profile together, generates a temporary password, emails it, and assigns the `Employee` role — keeping user identity and business profile in sync.

### 5. Service Catalog
- `ServiceService` provides CRUD for bookable services (name, description, duration, price), each optionally tied to a specific employee, and always scoped to a tenant.

### 6. Booking & Race Condition Handling
- `Booking` has `StartTime`/`EndTime`, `EmployeeId`, `ServiceId`, `StatusId` (Pending, Confirmed, Cancelled, Completed, No Show), and `Notes`.
- To prevent **two Users double-booking the same employee/time slot**:
  - Before create/update, `HasConflictAsync(tenantId, employeeId, start, end, excludeBookingId)` checks for overlapping bookings for that employee.
  - This check + the write should run inside a **DB transaction with a unique constraint / index on (EmployeeId, TimeRange)** or use **optimistic concurrency tokens** (e.g. a `RowVersion` column) so that if two requests race, the second commit fails and is retried/rejected — the conflict check alone isn't atomic without a transaction or DB-level constraint backing it.
  - On conflict, the service throws `InvalidOperationException`, which the API maps to a `409 Conflict`.

### 7. Paging & Sorting
- Booking (and other list) endpoints accept `page`, `pageSize`, and `sortBy`/`sortDirection` query parameters; repositories apply `Skip()`/`Take()` and `OrderBy()` at the query level (not in memory) so large tenants' booking lists stay performant.

## Testing (xUnit)

Unit tests target the service layer in isolation using **xUnit** + **Moq** + EF Core's **InMemory provider**:

- **Mocked repositories/dependencies** — `IBookingRepository`, `IBookingStatusRepository`, `IServiceRepository`, `IEmployeeRepository`, `ITenantProvider`, and `UserManager<ApplicationUser>` are all mocked with `Mock<T>`, so tests exercise only `BookingService`'s logic, not real DB/EF/Identity behavior.
- **In-memory `AppDbContext`** — a fresh `UseInMemoryDatabase(Guid.NewGuid().ToString())` context is created per test class instance so tests don't share state or hit a real SQL Server.
- **Arrange–Act–Assert** structure — e.g. `UpdateBooking_ShouldUpdateStatus`, `UpdateBooking_ShouldUpdateTime_WhenNoConflict`, `UpdateBooking_ShouldThrow_WhenTimeConflictExists`, `DeleteBooking_ShouldSucceed`.


## Auditing (BookingAudit table)

- `BookingAudit` logs every state-changing event (create/update/cancel) with a JSON `Data` payload for traceability.

| Column | Purpose |
|---|---|
| `Id` | Audit record's own identity |
| `BookingId` | Which booking the event belongs to |
| `TenantId` | Denormalized for fast tenant-scoped audit queries without a join |
| `EventType` | e.g. `Created`, `StatusChanged`, `TimeChanged`, `Cancelled` |
| `Data` | JSON payload — typically the diff/snapshot (old vs. new values) for that event |
| `CreatedBy` | User who performed the action (nullable — e.g. system/background jobs) |
| `CreatedAt` | Timestamp, defaulted at the DB level (`GETUTCDATE()`) |


## Database (EF Core + Identity)

| Table | Purpose |
|---|---|
| `AspNetUsers` (`ApplicationUser`) | Identity users, extended with `TenantId`, `FullName`, soft-delete |
| `AspNetRoles` / `AspNetUserRoles` | Identity roles (SuperAdmin, Admin, Employee, Customer) |
| `Tenants` | Businesses/organizations (Name, Slug) |
| `Employees` | Business staff profile linked 1:1 to a user |
| `Services` | Bookable service catalog per tenant |
| `Bookings` | Appointments (times, status, employee, service, user) |
| `BookingStatuses` | Lookup: Pending / Confirmed / Cancelled / Completed / No Show |
| `BookingAudit` | Change history for bookings |
| `Otp` | Hashed OTP codes with expiry for email verification |
| `RefreshToken` | Persisted JWT refresh tokens |

`AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` wires all of the above together via `IEntityTypeConfiguration<T>` classes (Fluent API) and seeds lookup data (`BookingStatus`) and roles/admin on startup.

## Design Patterns Used 

- **Repository Pattern** – `I*Repository` interfaces abstract EF Core data access from services.
- **Service Layer / Application Layer** – business logic lives in `*Service` classes, not controllers.
- **DTO Pattern** – requests/responses use dedicated DTOs, decoupled from entities.
- **Dependency Injection** – services/repos/`UserManager`/`IConfiguration` injected via constructors.
- **Multi-Tenancy (Tenant Context) Pattern** – `ITenantProvider` carries the current tenant through a request.
- **Factory Method** – static `Create(...)` methods on entities (`Employee.Create`, `ApplicationUser.Create`) centralize valid construction.
- **Soft Delete Pattern** – `IsDeleted` + `SoftDelete()` instead of physical deletes.
- **Options Pattern** – `SmtpSettings` bound from configuration.
- **Validator Pattern** – FluentValidation validators (e.g. `CreateBookingValidator`) separate input validation from business logic.
- **Optimistic Concurrency / Conflict Check Pattern** – `HasConflictAsync` guards booking overlaps (race conditions).

## Frontend (React.js)

BookFiy includes a simple React.js frontend used to consume the ASP.NET Core Web API.
The frontend allows users to authenticate, browse services, and create bookings.

## Project Onboarding Flow

1-SuperAdmin Login
Email: laithalnobane323@gmail.com
Password: Admin@Z1234

2- SuperAdmin creates a new Tenant (business) and its Admin account → Admin gets their password by email.
3- Admin logs in and creates Employee accounts for that tenant → each Employee gets their password by email.
4- Employee logs in and creates the Services they offer (name, duration, price).
5- user registers themselves using email + password (with OTP email verification).
6- user logs in, browses services, and creates a Booking.
7- System checks for time conflicts before confirming the booking.


## Design Decisions & Reflections

### A. How overlapping bookings are prevented and enforced

Overlap prevention happens in two layers, not one:

1. **Application-level check** — before creating or updating a booking, `BookingService` calls `HasConflictAsync(tenantId, employeeId, start, end, excludeBookingId)`, which queries existing bookings for that employee/tenant and checks for any overlapping time range (`existing.StartTime < newEnd && existing.EndTime > newStart`).

2. **Database-level guarantee** — the conflict check alone is not atomic under concurrent requests (see below), so the create/update runs inside a **DB transaction**, and a **unique constraint / index on `(EmployeeId, StartTime, EndTime)`** (or a `RowVersion` concurrency token on the `Booking` row) backs it up. If two requests race past the application check simultaneously, the second write fails at the database level rather than silently succeeding.

3. On conflict, `BookingService` throws `InvalidOperationException`, which the API layer maps to `409 Conflict`.


### B. Assumptions about concurrency

- Single SQL Server instance, no multi-writer/multi-region setup — the unique constraint approach assumes one source of truth.

- Conflicts are expected  (two customers booking the exact same employee/slot at the same instant), so optimistic concurrency (fail-and-reject) was chosen over pessimistic locking (lock-and-wait), since it doesn't hold DB locks under normal load.

- No client-side retry/queueing was assumed — if a `409` happens, the frontend simply shows the user the slot is taken and asks them to pick another.

- Each request is assumed to be scoped to a single tenant/employee combination — no cross-tenant transactions are needed.

### C. What would break at scale, and the first bottleneck

- **First bottleneck**: the booking write path — `HasConflictAsync` + insert, both against a single relational database. As tenants and employees grow, this table gets hot on the `(EmployeeId, StartTime)` index, and every booking attempt still requires a round trip + transaction.

- **Secondary bottleneck**: synchronous SMTP email sending (OTPs, temp passwords) inside the request/response cycle — this blocks a thread per request and doesn't scale well under bursty registration traffic.

- **List/read endpoints** (`GET /api/booking/employee/{id}`, etc.) would degrade as `BookingAudit` and `Bookings` grow per tenant, since paging/sorting is pushed to the DB but there's no caching layer or read replica yet.

- **Multi-tenancy** itself isn't a bottleneck by design (every query is tenant-scoped), but a single shared database means one very large tenant can still degrade performance for others (noisy neighbor problem).

### D. Evolving this into a distributed system

- Move email/OTP sending off the request thread into a **background worker / message queue** (e.g. Azure Service Bus, RabbitMQ) so registration and employee creation don't block on SMTP.

- Introduce an **outbox pattern** for `BookingAudit` so audit writes are eventually consistent and don't compete with the booking write itself.

- **Shard or partition by `TenantId`** once a single database becomes a bottleneck, since all queries are already tenant-scoped — this is the natural partition key.
- Add **read replicas** for list/paging endpoints, keeping writes (booking creation, conflict checks) on the primary.

- Replace the DB-level unique constraint with a **distributed lock or reservation service** (e.g. Redis-based) per `(EmployeeId, TimeSlot)` if the system moves to multiple database instances, since a cross-shard unique constraint isn't possible.

- Emit booking events (`Created`, `Cancelled`, etc.) to an event stream so downstream services (notifications, analytics) can consume without coupling to `BookingService` directly.

### E. Tradeoff prioritized: correctness over performance, with simplicity as a constraint

Correctness was prioritized first — the layered conflict check + DB constraint approach exists specifically to guarantee no double-booking ever slips through, even under race conditions, because a booking system's core trust promise is "if it's confirmed, it's yours." Simplicity came second — soft deletes, DTOs, and a straightforward layered architecture were chosen over more complex patterns (e.g. CQRS, event sourcing) since the scope didn't justify that complexity yet. Performance was treated as the thing to optimize *later*: the design deliberately avoids premature optimizations (caching, sharding, async messaging) until the actual bottleneck (see C) is observed under real load.


