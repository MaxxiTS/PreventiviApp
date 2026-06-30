#!/usr/bin/env bash
# Lanza el frontend Flutter. Por defecto en Chrome; pasa argumentos para otro target:
#   ./scripts/run-frontend.sh -d windows
set -euo pipefail
cd "$(dirname "$0")/../src/frontend"
flutter pub get
if [ "$#" -eq 0 ]; then
  flutter run -d chrome
else
  flutter run "$@"
fi
