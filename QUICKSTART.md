# 🚀 Guía de arranque local — Preventivi App

Pasos para levantar el backend y el frontend en tu máquina y empezar a ver el estado actual.

## Requisitos
- **.NET SDK 9.0+** (`dotnet --version`)
- **Flutter 3.27+ / Dart 3.6+** (`flutter --version`)

> ⚠️ El código se ha desarrollado en un entorno sin SDKs (no se ha compilado allí). Es
> probable que `dotnet build` / `flutter analyze` señalen algún error la primera vez:
> corrígelos y/o pásamelos y los arreglamos.

---

## 1. Backend (.NET 9 + SQLite)

```bash
cd src/backend
dotnet restore
dotnet run --project PreventiviApp.Api
```

- API en **http://localhost:5000** · Swagger en **http://localhost:5000/swagger**
- En el **primer arranque** crea `preventivi.db` (en `src/backend/PreventiviApp.Api/`) y
  **siembra datos de ejemplo**: un cliente, un proyecto, un preciosario importado de un DCF
  demo y un presupuesto con mediciones. Así ves contenido al instante.
- **CORS** abierto en desarrollo (necesario para el frontend web).

Tests:
```bash
dotnet test          # ejecuta los tests unitarios del motor de cálculo y servicios
```

Reiniciar los datos: borra `src/backend/PreventiviApp.Api/preventivi.db` y vuelve a arrancar.

---

## 2. Frontend (Flutter)

```bash
cd src/frontend
flutter pub get
flutter run -d chrome        # o -d windows / -d macos / -d linux
```

- Por defecto apunta a **http://localhost:5000**. Para otra URL:
  ```bash
  flutter run -d chrome --dart-define=API_BASE_URL=http://localhost:5000
  ```
- Análisis estático: `flutter analyze`

---

## 3. Qué puedes ver hoy
- **Proyectos**: lista (incluye el demo) y crear proyecto.
- **Preciosarios**: navegar capítulos → partidas → análisis de precios; **Importar DCF**
  (puedes usar `tools/dcf-importer/ejemplo.dcf`).
- **Presupuesto** demo: árbol de capítulos/partidas + resumen económico (PEM, baja, base,
  IVA, total). Acciones: añadir capítulos/partidas con mediciones, editar/bloquear precios,
  actualizar precios desde el preciosario, duplicar, nueva versión, comparar versiones y
  **descargar el PDF**.

## Atajos
```bash
./scripts/run-backend.sh     # arranca la API
./scripts/run-frontend.sh    # flutter pub get + run (chrome por defecto)
```

## Solución de problemas
- *El frontend no conecta*: confirma que la API está en `:5000` y que arrancó en
  `Development` (CORS activo). Mira la consola del backend.
- *Errores de compilación .NET*: ejecuta `dotnet build src/backend/PreventiviApp.sln` y
  comparte la salida.
- *Errores de Flutter*: ejecuta `flutter analyze` en `src/frontend` y comparte la salida.
