# EventosVivos

Plataforma de reservas en línea para eventos culturales, conferencias y talleres. Stack fullstack con .NET y Angular.

## Tecnologías

- Backend: .NET 10, ASP.NET Core Web API
- Frontend: Angular 19
- Base de datos: SQL Server + Entity Framework Core
- Tests: xUnit, FluentAssertions

## Arquitectura

Backend con arquitectura hexagonal:

- **Domain**: entidades y reglas de negocio
- **Application**: casos de uso, DTOs, validadores y puertos
- **Infrastructure**: EF Core, repositorios, SQL Server
- **Api**: controladores REST y manejo de errores

La capa de Application depende de abstracciones (interfaces), no de la infraestructura. Las reglas de negocio viven en el dominio, lo que facilita pruebas unitarias aisladas y el reemplazo de la persistencia sin afectar los casos de uso.

## Requisitos

- .NET 10 SDK
- Node.js 20+
- SQL Server (ej. `localhost\SQLEXPRESS`)

## Configuración de base de datos

Editar la cadena de conexión en `backend/src/EventosVivos.Api/appsettings.Development.json`:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EventosVivos;Trusted_Connection=True;TrustServerCertificate=True;"
```

Aplicar migraciones:

```bash
cd backend
dotnet ef database update --project src/EventosVivos.Infrastructure --startup-project src/EventosVivos.Api
```

## Ejecución local

**Backend:**

```bash
cd backend
dotnet run --project src/EventosVivos.Api
```

API: `http://localhost:5142`

**Frontend:**

```bash
cd frontend
npm install
npm start
```

App: `http://localhost:4200`

> Ambos deben estar corriendo al mismo tiempo.

## Seguridad

La confirmación de pago (`POST /api/reservations/{id}/confirm-payment`) requiere la cabecera `X-Admin-Key`. Configúrala en:

- Backend: `Admin:ApiKey` en `appsettings.Development.json`
- Frontend: `adminApiKey` en `frontend/src/environments/environment.ts`

Las reservas y cancelaciones permanecen públicas según el enunciado (RF-03 y RF-05).

## Tests

```bash
cd backend
dotnet test
```

- `EventosVivos.Application.Tests`: pruebas unitarias del dominio
- `EventosVivos.Api.IntegrationTests`: pruebas de integración de la API

## Endpoints principales

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/venues` | Venues de referencia |
| POST | `/api/events` | Crear evento |
| GET | `/api/events` | Listar eventos (filtros opcionales) |
| GET | `/api/events/{id}` | Obtener evento por id |
| GET | `/api/events/{id}/occupancy-report` | Reporte de ocupación |
| POST | `/api/reservations` | Crear reserva |
| POST | `/api/reservations/{id}/confirm-payment` | Confirmar pago |
| POST | `/api/reservations/{id}/cancel` | Cancelar reserva |

## Estructura del proyecto

```
backend/
  src/
    EventosVivos.Domain/
    EventosVivos.Application/
    EventosVivos.Infrastructure/
    EventosVivos.Api/
  tests/
    EventosVivos.Application.Tests/
    EventosVivos.Api.IntegrationTests/
frontend/
  src/app/
```
