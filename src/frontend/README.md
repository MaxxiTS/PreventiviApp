# Frontend — Preventivi App (Flutter)

App multiplataforma (escritorio / web / móvil) de **presupuestos y mediciones de
obra**, *offline-first*, que consume la API REST .NET del backend
(`../backend`). Inspiración UX: Notion / Linear (sidebar + panel central,
dark/light).

> Este es el **arranque (scaffold)**: consumo de API + UI. La persistencia local
> (Drift / SQLite) y la sincronización se añadirán en incrementos posteriores.

## Stack

- **Flutter 3.x** (Material 3, dark/light por `ThemeMode.system`).
- **flutter_riverpod 2.x** — gestión de estado (`Provider`, `FutureProvider`).
- **go_router 14** — navegación con shell persistente (NavigationRail).
- **dio 5** — cliente HTTP contra la API REST.
- **intl** — formateo de importes y fechas en `es-ES` (€).

Los modelos son **clases Dart planas con `fromJson` manual** (sin codegen).

## Estructura

```
lib/
  core/
    config/app_config.dart        # URL base de la API (--dart-define)
    network/dio_provider.dart     # Provider<Dio> (baseUrl, JSON, timeouts)
    theme/app_theme.dart          # Temas claro/oscuro (Material 3)
    router/app_router.dart        # Provider<GoRouter> + rutas
    router/app_shell.dart         # Shell con sidebar (NavigationRail)
  shared/
    utils/formato.dart            # Formato es-ES (€, fechas)
    widgets/                      # AsyncValueWidget, ErrorView, PageHeader
  features/
    home/                         # Inicio / accesos rápidos
    proyectos/                    # domain · data · presentation
    preciosarios/                 # domain · data · presentation
    presupuestos/                 # domain · data · presentation
  main.dart                       # ProviderScope + MaterialApp.router
```

Cada feature sigue el mismo patrón **domain / data / presentation**.

## Rutas

| Ruta | Pantalla |
|------|----------|
| `/` | Inicio / dashboard |
| `/proyectos` | Lista de proyectos (+ crear) |
| `/proyectos/:id` | Detalle de proyecto y sus presupuestos |
| `/preciosarios` | Catálogo de precios (capítulos → partidas → análisis) |
| `/presupuestos/:id` | Resumen económico (PEM, baja, base, IVA, TOTAL) + árbol |

## Ejecutar

```bash
cd src/frontend

# 1) Resolver dependencias
flutter pub get

# 2) Lanzar (elige plataforma)
flutter run -d chrome      # Web
flutter run -d windows     # Escritorio Windows
flutter run -d macos       # Escritorio macOS
flutter run -d linux       # Escritorio Linux
```

### Configurar la URL de la API

Por defecto se usa `http://localhost:5000`. Para apuntar a otra dirección, pasa
`--dart-define`:

```bash
flutter run -d chrome --dart-define=API_BASE_URL=http://192.168.1.50:5000
```

Arranca primero el backend:

```bash
cd ../backend
dotnet run --project PreventiviApp.Api
```

> **Nota CORS / web:** para ejecutar en Chrome contra el backend, asegúrate de
> que la API permita el origen del frontend (CORS) o usa una build de escritorio
> durante el desarrollo.

## Endpoints consumidos

- `GET  /api/proyectos`, `GET /api/proyectos/{id}`, `POST /api/proyectos`
- `GET  /api/preciosarios`
- `GET  /api/preciosarios/{id}/capitulos?padreId=`
- `GET  /api/preciosarios/capitulos/{id}/partidas`
- `GET  /api/preciosarios/partidas/{id}/analisis`
- `GET  /api/presupuestos?proyectoId=`, `GET /api/presupuestos/{id}`

El backend serializa JSON en **camelCase** y los enums como **string** (p. ej.
`estado: "Borrador"`), tal como esperan los `fromJson` de cada modelo.
