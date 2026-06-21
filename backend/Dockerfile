FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/src/EventosVivos.Domain/EventosVivos.Domain.csproj backend/src/EventosVivos.Domain/
COPY backend/src/EventosVivos.Application/EventosVivos.Application.csproj backend/src/EventosVivos.Application/
COPY backend/src/EventosVivos.Infrastructure/EventosVivos.Infrastructure.csproj backend/src/EventosVivos.Infrastructure/
COPY backend/src/EventosVivos.Api/EventosVivos.Api.csproj backend/src/EventosVivos.Api/

RUN dotnet restore backend/src/EventosVivos.Api/EventosVivos.Api.csproj

COPY backend/src/ backend/src/

RUN dotnet publish backend/src/EventosVivos.Api/EventosVivos.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventosVivos.Api.dll"]
