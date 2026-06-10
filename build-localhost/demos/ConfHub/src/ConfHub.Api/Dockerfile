# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore (cached layer) — copy project file first
COPY ConfHub.Api.csproj ./
RUN dotnet restore ConfHub.Api.csproj

# Copy the rest and publish
COPY . ./
RUN dotnet publish ConfHub.Api.csproj -c Release -o /app --no-restore

# ── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app ./

# Container Apps routes ingress to port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ConfHub.Api.dll"]
