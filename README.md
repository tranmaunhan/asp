# Busline Project API

ASP.NET Core Web API for managing users, roles, vehicles, routes, trips, bookings, and tickets.

**Base URL (local):** `https://localhost:5001`  
If your port is different, update the Postman variable `baseUrl`.

## Prerequisites

- .NET SDK (net8.0)
- SQL Server (or your configured connection in `busline_project/appsettings.json`)

## Run

```bash
dotnet restore
dotnet run --project busline_project/busline_project.csproj
```

Swagger is enabled in Development: `https://localhost:5001/swagger`

## Controllers & Routes

All controllers use `[Route("[controller]")]`, so endpoint paths match controller names:

- `Users`
- `Roles`
- `UserRoles` (composite key)
- `VehicleTypes`
- `Vehicles`
- `SeatTemplates`
- `Locations`
- `Routes`
- `RouteStops`
- `Trips`
- `TripSeats`
- `Bookings`
- `Tickets`

### Standard CRUD

Each controller (except `UserRoles`) supports:

- `GET /<Controller>` – list
- `GET /<Controller>/{id}` – get by id
- `POST /<Controller>` – create
- `PUT /<Controller>/{id}` – update (body must include matching `Id`)
- `DELETE /<Controller>/{id}` – delete

### UserRoles (composite key)

- `GET /UserRoles/{userId}/{roleId}`
- `PUT /UserRoles/{userId}/{roleId}`
- `DELETE /UserRoles/{userId}/{roleId}`
- `POST /UserRoles` with body `{ "userId": 1, "roleId": 2 }`

## Enums (numeric values)

- `UserStatus`: `0=Active`
- `VehicleStatus`: `0=Active`, `1=Maintenance`
- `LocationType`: `0=Station`, `1=City`, `2=Stop`
- `TripStatus`: `0=Scheduled`, `1=InProgress`, `2=Completed`, `3=Cancelled`
- `TripSeatStatus`: `0=Available`, `1=Reserved`, `2=Booked`, `3=Blocked`
- `BookingStatus`: `0=Pending`, `1=Confirmed`, `2=Cancelled`, `3=Refunded`

## Postman

Import the collection file:

`busline_project.postman_collection.json`

Then update the collection variable `baseUrl` if needed.
