# ─────────────────────────────────────────────
# Etapa 1: Build
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar csproj y restaurar dependencias primero (cache de capas)
COPY ["inmobiliaria_api.csproj", "./"]
RUN dotnet restore "inmobiliaria_api.csproj"

# Copiar todo el código fuente
COPY . .

# Publicar en modo Release
RUN dotnet publish "inmobiliaria_api.csproj" -c Release -o /app/publish

# ─────────────────────────────────────────────
# Etapa 2: Runtime (imagen mucho más liviana)
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Instalar curl para health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copiar binarios desde etapa build
COPY --from=build /app/publish .

# El backend siempre corre en HTTP dentro del contenedor
# El proxy reverso externo maneja TLS
ENV ASPNETCORE_URLS=http://+:2000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:2000/api/health || exit 1

EXPOSE 2000

ENTRYPOINT ["dotnet", "inmobiliaria_api.dll"]
