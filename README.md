# Travel Request Management System

A role-based web application for managing employee business travel: raising requests, multi-level approval, booking, trip extensions, cancellations and a full audit trail.

Built with **ASP.NET Core (.NET 10)**, **Blazor (Server + WebAssembly)**, **Entity Framework Core** and **SQL Server**, using ASP.NET Core Identity for authentication and authorization.

---

## Features

- **Travel requests** – create, save as draft, edit, submit, cancel (Domestic / International)
- **Two-level approval workflow** – Manager, then Department Head, each with approve / reject and comments
- **Trip extensions** – request an extension on an approved trip, which goes through approval again
- **Bookings** – Travel Admin books Flight, Hotel, Train and Cab against approved requests and updates booking status
- **Audit log** – every action (created, submitted, approved, rejected, booked, cancelled, completed...) is recorded
- **Admin panel** – dashboard, user management (create / edit / activate / deactivate / delete), department management
- **Role-based access** – policies enforced on API endpoints and UI pages
- **Employee self-service** – dashboard, my requests, profile and change password

## Roles

| Role | Can do |
|------|--------|
| **Employee** | Create and submit travel requests, view own requests, request extension or cancellation |
| **Manager** | Everything an Employee can, plus approve or reject requests from their team (first level) |
| **DepartmentHead** | Everything an Employee can, plus approve or reject requests (second level) |
| **TravelAdmin** | Manage users and departments, view all requests, create and manage bookings, view history |

## Request Lifecycle

```
Draft -> PendingManager -> PendingDeptHead -> Approved -> Completed
                |                |               |
                v                v               +--> ExtensionRequested --> (re-approval)
             Rejected         Rejected           +--> Cancelled
```

Statuses: `Draft`, `PendingManager`, `PendingDeptHead`, `Approved`, `Rejected`, `ExtensionRequested`, `Cancelled`, `Completed`.

## Tech Stack

- .NET 10 / ASP.NET Core
- Blazor Web App (interactive Server and WebAssembly render modes)
- Entity Framework Core 10 (SQL Server, code-first migrations)
- ASP.NET Core Identity (`IdentityRole<int>`)
- Mapperly (compile-time object mapping)
- Bootstrap

## Solution Structure

```
TravelRequestManagementSystem/
├── TravelRequestManagement/            # ASP.NET Core host: API controllers, startup, DI
├── TravelRequestManagement.Client/     # Blazor UI: pages, components, API client services
├── TravelMangement.Services/           # Business logic: services, workflow rules, mappers
├── TravelManagement.Data/              # EF Core DbContext, entities, repositories, migrations, seed data
├── TravelManagement.Services.Shared/   # DTOs, enums, constants shared by API and UI
└── TravelRequestManagement.slnx
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express, Developer or LocalDB)
- Optional: Visual Studio 2022+ or VS Code

### 1. Clone

```bash
git clone https://github.com/<your-username>/<repo-name>.git
cd <repo-name>
```

### 2. Configure the database

Edit the connection string in `TravelRequestManagement/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=TravelManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Change `Server=` to match your SQL Server instance. To avoid committing your own settings, use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables instead:

```bash
cd TravelRequestManagement
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your connection string>"
```

### 3. Run

```bash
cd TravelRequestManagement
dotnet run
```

The app runs at:

- https://localhost:7082
- http://localhost:5103

On startup the app applies pending EF Core migrations and seeds the roles and the default admin user. If this fails (for example, SQL Server unreachable), the error is logged and the app still starts, so check the console output.

To apply migrations manually:

```bash
dotnet ef database update --project TravelManagement.Data --startup-project TravelRequestManagement
```

### Default admin account

| Field | Value |
|-------|-------|
| Email | `admin@travel.local` |
| Password | `Admin@12345` |

> **Change this password immediately after first login.** These credentials are for local development only. Do not deploy with the seeded password.

## API Overview

| Area | Base route | Access |
|------|-----------|--------|
| Auth | `/auth` (login, logout, change-password) | Public / authenticated |
| Current user | `/api/me` | Authenticated |
| Requests | `/api/requests` (mine, create, update, submit, cancel, extension, book) | Authenticated (create actions: Employee, Manager, DepartmentHead) |
| Manager approvals | `/api/manager` (pending, approved, rejected, all, approve, reject) | Manager |
| Department Head approvals | `/api/department-head` (pending, approved, rejected, all, approve, reject) | DepartmentHead |
| Admin | `/api/admin` (dashboard, users, departments, requests, bookings) | TravelAdmin |

## Database Migrations

Add a new migration after changing entities:

```bash
dotnet ef migrations add <MigrationName> --project TravelManagement.Data --startup-project TravelRequestManagement
dotnet ef database update --project TravelManagement.Data --startup-project TravelRequestManagement
```

## Security Notes

- Do not commit real connection strings, passwords or secrets. Use user secrets or environment variables.
- Change the seeded admin password before any real deployment.
- Password policy: minimum 8 characters, unique email required.

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Commit your changes: `git commit -m "Add my feature"`
4. Push the branch: `git push origin feature/my-feature`
5. Open a pull request


