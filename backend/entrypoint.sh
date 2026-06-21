#!/bin/sh
set -e

# Generate the database settings file from environment variables so the
# connection string (incl. password) is never baked into a committed file.
# Values are centralized in the repo .env and passed in via docker compose.
mkdir -p /app/App_Data
cat > /app/App_Data/databaseSettings.json <<EOF
{
  "DataProvider": "${DB_PROVIDER:-sqlserver}",
  "DataConnectionString": "${DB_CONNECTION_STRING}"
}
EOF

exec dotnet Rat.Api.dll
