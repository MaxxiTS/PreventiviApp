#!/usr/bin/env bash
# Arranca la API .NET en http://localhost:5000 (Development: crea y siembra la BD).
set -euo pipefail
cd "$(dirname "$0")/../src/backend"
dotnet run --project PreventiviApp.Api
