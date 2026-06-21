# syntax=docker/dockerfile:1

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0.422 AS build
WORKDIR /src

# Copy the whole source tree (transitive project references + dynamic
# assembly scanning at runtime require every Rat.* library in the output).
COPY src/ ./src/

RUN dotnet restore "src/Rat.Api/Rat.Api.csproj"
RUN dotnet publish "src/Rat.Api/Rat.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0.28 AS final
WORKDIR /app

# curl is used by the compose healthcheck (signals migrations are done & app is up).
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

# Entrypoint generates App_Data/databaseSettings.json from environment variables
# at startup, so no database password is stored in any committed file.
# sed strips CR in case the script was checked out with CRLF line endings.
COPY entrypoint.sh ./entrypoint.sh
RUN sed -i 's/\r$//' ./entrypoint.sh && chmod +x ./entrypoint.sh

ENV ASPNETCORE_URLS=http://+:47050
EXPOSE 47050

ENTRYPOINT ["/app/entrypoint.sh"]
