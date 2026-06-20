# EventosVivos

Sistema de reservas para eventos culturales. Prueba técnica fullstack con .NET y Angular.

## Tecnologías

- Backend: .NET 10, ASP.NET Core Web API
- Frontend: Angular 19 (pendiente)
- Base de datos: SQL Server + Entity Framework Core (pendiente)
- Tests: xUnit, FluentAssertions

## Arquitectura

Backend con arquitectura hexagonal:

- **Domain**: entidades y reglas de negocio
- **Application**: casos de uso, DTOs, validadores y puertos
- **Infrastructure**: EF Core, repositorios, SQL Server
- **Api**: controladores REST y manejo de errores

La capa de Application depende de abstracciones (interfaces), no de la infraestructura. Las reglas de negocio viven en el dominio.

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
```
