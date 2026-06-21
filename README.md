# EventosVivos

Plataforma de reservas en línea para eventos culturales, conferencias y talleres. Stack fullstack con .NET y Angular.

## Tecnologías

- Backend: .NET 10, ASP.NET Core Web API
- Frontend: Angular 19
- Base de datos: SQL Server + Entity Framework Core
- Contenedores: Docker Compose (SQL Server, API, frontend)
- Tests: xUnit, FluentAssertions

## Arquitectura

Backend con arquitectura hexagonal:

- **Domain**: entidades y reglas de negocio
- **Application**: casos de uso, DTOs, validadores y puertos
- **Infrastructure**: EF Core, repositorios, SQL Server
- **Api**: controladores REST y manejo de errores

La capa de Application depende de abstracciones (interfaces), no de la infraestructura. Las reglas de negocio viven en el dominio, lo que facilita pruebas unitarias aisladas y el reemplazo de la persistencia sin afectar los casos de uso.

## Requisitos

**Ejecución local (sin Docker):**

- .NET 10 SDK
- Node.js 20+
- SQL Server (ej. `localhost\SQLEXPRESS`)

**Ejecución con Docker:**

- Docker Desktop (o Docker Engine + Docker Compose v2)

## Configuración de base de datos

### Local (SQL Server instalado)

Editar la cadena de conexión en `backend/src/EventosVivos.Api/appsettings.Development.json`:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EventosVivos;Trusted_Connection=True;TrustServerCertificate=True;"
```

Aplicar migraciones:

```bash
cd backend
dotnet ef database update --project src/EventosVivos.Infrastructure --startup-project src/EventosVivos.Api
```

### Solo SQL Server en Docker (híbrido)

Si prefieres correr backend y frontend en local pero la base de datos en Docker:

```bash
docker compose up sqlserver -d
```

Usa esta cadena de conexión en `appsettings.Development.json`:

```json
"DefaultConnection": "Server=localhost,1433;Database=EventosVivos;User Id=sa;Password=EventosVivos_Dev123!;TrustServerCertificate=True;Encrypt=False"
```

> La contraseña debe coincidir con `MSSQL_SA_PASSWORD` en `.env` (copia `.env.example` a `.env` si la cambias).

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

## Ejecución con Docker (un solo comando)

Desde la raíz del repositorio:

```bash
cp .env.example .env   # opcional, usa valores por defecto si no existe
docker compose up --build
```

Servicios:

| Servicio | URL |
|----------|-----|
| Frontend | http://localhost:4200 |
| API | http://localhost:5142 |
| SQL Server | localhost:1433 |

La API aplica migraciones automáticamente al iniciar. Nginx en el contenedor frontend proxy `/api` hacia el backend.

Detener y limpiar:

```bash
docker compose down          # detener contenedores
docker compose down -v       # detener y borrar volumen de SQL Server
```

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
  Dockerfile
  src/
    EventosVivos.Domain/
    EventosVivos.Application/
    EventosVivos.Infrastructure/
    EventosVivos.Api/
  tests/
    EventosVivos.Application.Tests/
    EventosVivos.Api.IntegrationTests/
frontend/
  Dockerfile
  nginx.conf
  src/app/
docker-compose.yml
.env.example
```
