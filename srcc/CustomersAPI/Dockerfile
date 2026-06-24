# ─────────────────────────────────────────────────────────────
#  Stage 1: BUILD
# ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file first and restore dependencies.
# 'dotnet restore' is skipped on rebuild (faster CI/CD).

COPY CustomersAPI.csproj ./
RUN dotnet restore

# Copy remaining source and publish in Release mode.
# -o /app/publish       → output folder
# /p:UseAppHost=false   → no native executable wrapper needed in containers

COPY . ./
RUN dotnet publish CustomersAPI.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ─────────────────────────────────────────────────────────────
#  Stage 2: RUNTIME
# ─────────────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy published output from build stage only
COPY --from=build /app/publish .

# Run as non-root user — security best practice
RUN addgroup --system --gid 1001 appgroup && \
    adduser  --system --uid 1001 --ingroup appgroup appuser
USER appuser

# Kestrel listens on port 80 inside the container.
# This matches containerPort: 80 in the K8s deployment YAML.
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# ── These are DEFAULTS only.
#    Real values come from ConfigMap + Secret at runtime in Kubernetes.
#    Never hardcode real passwords here.
ENV DB_HOST=localhost \
    DB_PORT=5432 \
    DB_NAME=customersdb \
    DB_USER=postgres \
    DB_PASSWORD=changeme \
    ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CustomersAPI.dll"]
