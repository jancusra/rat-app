# syntax=docker/dockerfile:1

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the whole source tree (transitive project references + dynamic
# assembly scanning at runtime require every Rat.* library in the output).
COPY src/ ./src/

RUN dotnet restore "src/Rat.Api/Rat.Api.csproj"
RUN dotnet publish "src/Rat.Api/Rat.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# curl is used by the compose healthcheck (signals migrations are done & app is up).
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

# Container database configuration (points at the compose "db" service).
# Kept separate so the repo's localhost default still works for local dev.
COPY databaseSettings.Docker.json ./App_Data/databaseSettings.json

ENV ASPNETCORE_URLS=http://+:47050
EXPOSE 47050

ENTRYPOINT ["dotnet", "Rat.Api.dll"]
