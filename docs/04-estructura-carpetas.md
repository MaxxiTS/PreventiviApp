# Estructura de Carpetas del Monorepo

Este documento detalla la organización física del monorepo de **Preventivi App**: un backend en **.NET 9** con **Clean Architecture**, un frontend en **Flutter** con patrón **feature-first + MVVM**, y los recursos compartidos (esquema de base de datos, importador DCF, contratos, documentación, scripts y CI/CD).

El objetivo de la estructura es separar responsabilidades con claridad, respetar la **regla de dependencia hacia adentro** (Domain → Application → Infrastructure → Presentation) en el backend, y permitir el **desarrollo aislado por funcionalidad** en el frontend para escalar el equipo y el producto.

## Vista General del Monorepo

```text
PreventiviApp/
├── backend/                  # Solución .NET 9 (Clean Architecture)
│   ├── PreventiviApp.Domain/
│   ├── PreventiviApp.Application/
│   ├── PreventiviApp.Infrastructure/
│   ├── PreventiviApp.Api/
│   ├── PreventiviApp.Tests.Unit/
│   ├── PreventiviApp.Tests.Integration/
│   └── PreventiviApp.sln
├── frontend/                 # App Flutter (Desktop + Mobile + Web)
│   ├── lib/
│   │   ├── core/
│   │   ├── features/
│   │   └── shared/
│   ├── test/
│   └── pubspec.yaml
├── database/                 # Esquema, migraciones y datos semilla
│   ├── schema/
│   ├── migrations/
│   └── seeds/
├── tools/
│   └── dcf-importer/         # Herramienta de importación de preciosarios DCF
├── src/
│   └── shared/
│       └── contracts/        # Contratos compartidos (DTOs/OpenAPI/JSON Schema)
├── docs/                     # Documentación (español)
├── scripts/                  # Scripts de build, despliegue y utilidades
└── .github/
    └── workflows/            # Pipelines CI/CD (GitHub Actions)
```

| Carpeta | Responsabilidad |
| --- | --- |
| `backend/` | API REST en .NET 9 con Clean Architecture y CQRS (MediatR). |
| `frontend/` | Aplicación Flutter multiplataforma con Riverpod y arquitectura feature-first. |
| `database/` | Definición canónica del esquema, migraciones SQL y datos de carga inicial. |
| `tools/dcf-importer/` | Parser e importador del formato DCF a la estructura de dominio. |
| `src/shared/contracts/` | Contratos de datos compartidos entre backend y frontend. |
| `docs/` | Toda la documentación técnica del proyecto (en español). |
| `scripts/` | Automatización local (setup, lint, build, migraciones, generación de código). |
| `.github/workflows/` | Integración y entrega continua. |

---

## Backend — .NET 9 con Clean Architecture

La solución se divide en proyectos independientes que materializan las capas de Clean Architecture. Cada proyecto solo referencia a las capas más internas, garantizando que el **Dominio no dependa de nada** y que la infraestructura sea un detalle reemplazable.

```text
backend/
├── PreventiviApp.sln
│
├── PreventiviApp.Domain/                 # Capa más interna — sin dependencias externas
│   ├── Entities/
│   │   ├── Proyecto.cs
│   │   ├── Cliente.cs
│   │   ├── Preciosario.cs
│   │   ├── Capitulo.cs
│   │   ├── Partida.cs
│   │   ├── Descompuesto.cs
│   │   ├── Recurso.cs
│   │   ├── Presupuesto.cs
│   │   ├── Medicion.cs
│   │   └── LineaMedicion.cs
│   ├── ValueObjects/
│   │   ├── Dinero.cs                      # importe + moneda
│   │   ├── Codigo.cs                      # código normalizado de partida/capítulo
│   │   └── Formula.cs                     # expresión de línea de medición
│   ├── Events/
│   │   ├── PresupuestoCreadoEvent.cs
│   │   ├── PartidaActualizadaEvent.cs
│   │   └── PreciosarioImportadoEvent.cs
│   ├── Interfaces/
│   │   ├── IProyectoRepository.cs
│   │   ├── IPresupuestoRepository.cs
│   │   ├── IUnitOfWork.cs
│   │   └── IDomainEvent.cs
│   └── Common/
│       ├── Entity.cs                      # base con Id (UUID v7)
│       ├── AggregateRoot.cs
│       └── Result.cs                      # Result Pattern
│
├── PreventiviApp.Application/             # Casos de uso — orquesta el dominio
│   ├── UseCases/
│   │   ├── Presupuestos/
│   │   │   ├── Commands/
│   │   │   │   ├── CrearPresupuesto/
│   │   │   │   │   ├── CrearPresupuestoCommand.cs
│   │   │   │   │   ├── CrearPresupuestoHandler.cs
│   │   │   │   │   └── CrearPresupuestoValidator.cs
│   │   │   │   └── ActualizarPartidaPresupuesto/
│   │   │   └── Queries/
│   │   │       ├── ObtenerPresupuesto/
│   │   │       │   ├── ObtenerPresupuestoQuery.cs
│   │   │       │   └── ObtenerPresupuestoHandler.cs
│   │   │       └── ListarPresupuestos/
│   │   ├── Proyectos/
│   │   ├── Preciosarios/
│   │   └── Mediciones/
│   ├── DTOs/
│   │   ├── PresupuestoDto.cs
│   │   ├── PartidaDto.cs
│   │   └── MedicionDto.cs
│   ├── Mappers/
│   │   ├── PresupuestoMappingConfig.cs    # Mapster/AutoMapper
│   │   └── PartidaMappingConfig.cs
│   ├── Validators/
│   │   ├── CrearPresupuestoValidator.cs   # FluentValidation
│   │   └── LineaMedicionValidator.cs
│   ├── Behaviors/                         # Pipeline de MediatR (cross-cutting)
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   └── TransactionBehavior.cs
│   └── Common/
│       └── DependencyInjection.cs         # AddApplication()
│
├── PreventiviApp.Infrastructure/          # Detalles técnicos — implementa interfaces del Dominio
│   ├── Persistence/
│   │   └── EFCore/
│   │       ├── AppDbContext.cs
│   │       └── Configurations/
│   │           ├── PartidaConfiguration.cs
│   │           └── PresupuestoConfiguration.cs
│   ├── Repositories/
│   │   ├── ProyectoRepository.cs
│   │   ├── PresupuestoRepository.cs
│   │   └── UnitOfWork.cs
│   ├── Migrations/                        # Migraciones EF Core
│   │   └── 20260101_InitialCreate.cs
│   ├── Sync/                              # Sincronización offline-first
│   │   ├── ColaSincronizacionService.cs   # change log
│   │   ├── SyncHub.cs                     # SignalR/WebSockets
│   │   └── ConflictResolver.cs
│   ├── DcfImport/
│   │   ├── DcfParser.cs
│   │   └── DcfImportService.cs
│   ├── Documents/
│   │   ├── PdfExporter.cs                 # QuestPDF
│   │   ├── ExcelExporter.cs               # ClosedXML
│   │   └── CsvJsonXmlExporter.cs
│   ├── Ai/
│   │   ├── EmbeddingService.cs            # pgvector
│   │   ├── BusquedaSemanticaService.cs
│   │   └── ClaudeLlmClient.cs             # claude-opus-4-8 / sonnet / haiku
│   ├── Services/                          # Servicios externos
│   │   ├── DateTimeProvider.cs
│   │   └── EmailSender.cs
│   └── DependencyInjection.cs             # AddInfrastructure()
│
├── PreventiviApp.Api/                     # Capa de presentación (host HTTP)
│   ├── Controllers/
│   │   ├── PresupuestosController.cs
│   │   ├── ProyectosController.cs
│   │   ├── PreciosariosController.cs
│   │   └── SyncController.cs
│   ├── Middlewares/
│   │   ├── ExceptionHandlingMiddleware.cs # error handling centralizado
│   │   └── RequestLoggingMiddleware.cs
│   ├── DI/
│   │   └── DependencyInjection.cs         # AddApi() + composición de capas
│   ├── appsettings.json
│   └── Program.cs                         # arranque, Serilog, MediatR, SignalR
│
├── PreventiviApp.Tests.Unit/             # Pruebas de Dominio y Application
│   ├── Domain/
│   │   └── PresupuestoTests.cs
│   └── Application/
│       └── CrearPresupuestoHandlerTests.cs
│
└── PreventiviApp.Tests.Integration/      # Pruebas API + BD real (Testcontainers)
    ├── Api/
    │   └── PresupuestosEndpointTests.cs
    └── Persistence/
        └── PresupuestoRepositoryTests.cs
```

### Responsabilidad de cada proyecto

| Proyecto | Capa | Depende de | Contenido |
| --- | --- | --- | --- |
| `PreventiviApp.Domain` | Dominio | — (ninguna) | Entidades, Value Objects, eventos de dominio, interfaces de repositorio y reglas de negocio puras. |
| `PreventiviApp.Application` | Aplicación | Domain | Casos de uso (Commands/Queries con MediatR), DTOs, mappers, validadores y behaviors del pipeline. |
| `PreventiviApp.Infrastructure` | Infraestructura | Application, Domain | EF Core, repositorios, migraciones, sync, importación DCF, exportación de documentos, IA y servicios externos. |
| `PreventiviApp.Api` | Presentación | Application, Infrastructure | Controllers, middlewares, composición de DI y `Program.cs`. |
| `PreventiviApp.Tests.Unit` | Pruebas | Domain, Application | Tests unitarios sin dependencias externas. |
| `PreventiviApp.Tests.Integration` | Pruebas | Api | Tests de extremo a extremo contra una BD real. |

### Por qué separar en proyectos en .NET

La separación física en proyectos (no solo en carpetas) **fuerza la regla de dependencia en tiempo de compilación**: el compilador impide que el Dominio referencie a EF Core o a la Web API. Esto aporta:

- **Aislamiento real del dominio**: la lógica de negocio se prueba sin base de datos ni framework web.
- **Reemplazabilidad de la infraestructura**: cambiar SQLite por PostgreSQL, o QuestPDF por otro generador, no toca el Dominio ni la Application.
- **Tiempos de compilación y test más rápidos** por capa, y límites de equipo más claros.
- **CQRS limpio**: los handlers de MediatR viven en Application y son la única vía de entrada al dominio desde la API.

---

## Frontend — Flutter feature-first + MVVM

El frontend se organiza por **funcionalidades** (`features/`), no por tipo técnico. Cada feature es un módulo autocontenido con sus propias capas `data` / `domain` / `presentation`, lo que permite trabajar en `presupuestos` sin tocar `mediciones`. Lo transversal vive en `core/` y lo reutilizable de UI/modelos en `shared/`.

```text
frontend/
├── pubspec.yaml
├── lib/
│   ├── main.dart
│   │
│   ├── core/                             # Infraestructura transversal de la app
│   │   ├── di/
│   │   │   └── providers.dart            # Riverpod: providers globales
│   │   ├── network/
│   │   │   ├── api_client.dart           # Dio/HTTP + interceptores
│   │   │   └── signalr_client.dart       # canal de sync en tiempo real
│   │   ├── database/
│   │   │   ├── app_database.dart         # SQLite local (FTS5)
│   │   │   └── dao/
│   │   ├── theme/
│   │   │   ├── app_theme.dart
│   │   │   └── colors.dart
│   │   ├── router/
│   │   │   └── app_router.dart           # GoRouter
│   │   └── utils/
│   │       ├── formatters.dart
│   │       └── result.dart
│   │
│   ├── features/
│   │   ├── proyectos/
│   │   │   ├── data/
│   │   │   │   ├── datasources/
│   │   │   │   ├── models/
│   │   │   │   └── repositories/         # implementación
│   │   │   ├── domain/
│   │   │   │   ├── entities/
│   │   │   │   ├── repositories/         # contratos
│   │   │   │   └── usecases/
│   │   │   └── presentation/
│   │   │       ├── viewmodels/           # MVVM (Notifier/Riverpod)
│   │   │       ├── pages/
│   │   │       └── widgets/
│   │   ├── preciosarios/
│   │   │   ├── data/
│   │   │   ├── domain/
│   │   │   └── presentation/
│   │   ├── presupuestos/
│   │   │   ├── data/
│   │   │   ├── domain/
│   │   │   └── presentation/
│   │   ├── mediciones/
│   │   │   ├── data/
│   │   │   ├── domain/
│   │   │   └── presentation/
│   │   ├── buscador/
│   │   │   ├── data/
│   │   │   ├── domain/
│   │   │   └── presentation/
│   │   ├── documentos/
│   │   │   ├── data/
│   │   │   ├── domain/
│   │   │   └── presentation/
│   │   └── sync/
│   │       ├── data/
│   │       ├── domain/
│   │       └── presentation/
│   │
│   └── shared/
│       ├── widgets/                      # componentes UI reutilizables
│       │   ├── app_button.dart
│       │   ├── data_table.dart           # tabla virtualizada (500k+ filas)
│       │   └── empty_state.dart
│       └── models/                       # modelos compartidos entre features
│           └── paginacion.dart
│
└── test/
    ├── core/
    ├── features/
    │   ├── presupuestos/
    │   └── mediciones/
    └── shared/
```

### Capas dentro de cada feature

| Capa | Contenido | Rol |
| --- | --- | --- |
| `data/` | `datasources`, `models`, `repositories` (impl.) | Acceso a API, BD local y mapeo de modelos. |
| `domain/` | `entities`, `repositories` (contratos), `usecases` | Lógica de negocio independiente de framework. |
| `presentation/` | `viewmodels`, `pages`, `widgets` | UI y estado (MVVM con Riverpod). |

### Por qué feature-first en Flutter

- **Cohesión por funcionalidad**: todo lo de `presupuestos` (datos, lógica y UI) vive junto, reduciendo el salto entre carpetas al desarrollar o depurar.
- **Escalabilidad de equipo**: distintas personas trabajan en features distintas con mínimos conflictos de merge.
- **Bajo acoplamiento**: una feature solo se comunica con otras vía contratos en `domain/` o modelos de `shared/`, no por dependencias directas de UI.
- **MVVM con Riverpod**: los `viewmodels` (Notifiers) exponen estado inmutable a las `pages`, separando la lógica de presentación de los widgets y facilitando las pruebas.
- **Replicabilidad**: cada feature sigue la misma plantilla `data/domain/presentation`, lo que hace predecible dónde encontrar o añadir código.

---

## Recursos Compartidos del Monorepo

### `database/`

```text
database/
├── schema/
│   └── schema.sql                # definición canónica de tablas (singular snake_case)
├── migrations/
│   ├── postgres/                 # migraciones servidor (pg_trgm, tsvector, pgvector)
│   └── sqlite/                   # migraciones BD local (FTS5)
└── seeds/
    ├── unidades.sql              # m, m2, m3, ud, kg, h...
    ├── roles_permisos.sql        # RBAC
    └── iva.sql
```

Fuente única de verdad del modelo de datos: esquema, migraciones por motor y datos semilla iniciales.

### `tools/dcf-importer/`

```text
tools/dcf-importer/
├── src/
│   ├── parser/                   # lectura del formato DCF
│   ├── mapper/                   # DCF → entidades (Capitulo, Partida, Descompuesto)
│   └── cli.ts
└── samples/                      # ficheros DCF de ejemplo
```

Herramienta independiente para parsear e importar preciosarios DCF (con vistas futuras a BC3/FIEBDC-3). Se mantiene fuera del backend para poder ejecutarse de forma autónoma sobre lotes grandes.

### `src/shared/contracts/`

```text
src/shared/contracts/
├── openapi/
│   └── api.yaml                  # contrato OpenAPI de la API .NET
├── dtos/                         # esquemas de DTOs compartidos
└── events/                       # esquemas de eventos de sync
```

Contratos de datos compartidos entre backend y frontend, evitando divergencias entre cliente y servidor.

### `docs/`, `scripts/` y `.github/workflows/`

```text
docs/
├── 01-vision-producto.md
├── 02-arquitectura.md
├── 03-modelo-dominio.md
└── 04-estructura-carpetas.md

scripts/
├── setup.sh                      # instalación de dependencias
├── db-migrate.sh                 # aplicar migraciones
├── generate-contracts.sh         # generar tipos desde OpenAPI
└── lint.sh

.github/workflows/
├── backend-ci.yml                # build + test .NET 9
├── frontend-ci.yml               # build + test Flutter
└── release.yml                   # empaquetado y despliegue
```

- **`docs/`**: documentación técnica en español, un fichero numerado por tema.
- **`scripts/`**: automatización reproducible de tareas locales y de CI.
- **`.github/workflows/`**: pipelines de integración y entrega continua, separados por proyecto.

---

## Convenciones de Nombrado y Organización

### Base de datos

| Elemento | Convención | Ejemplo |
| --- | --- | --- |
| Tablas | singular, `snake_case` | `partida`, `linea_medicion` |
| Columnas | `snake_case` | `precio_unitario`, `creado_en` |
| Claves primarias | `id` (UUID v7) | `id` |
| Claves foráneas | `<entidad>_id` | `capitulo_id`, `proyecto_id` |

### Backend (.NET / C#)

| Elemento | Convención | Ejemplo |
| --- | --- | --- |
| Proyectos | `PreventiviApp.<Capa>` | `PreventiviApp.Application` |
| Clases / Entidades | `PascalCase` | `Presupuesto`, `LineaMedicion` |
| Interfaces | `I` + `PascalCase` | `IPresupuestoRepository` |
| Commands / Queries | `<Acción><Entidad>Command/Query` | `CrearPresupuestoCommand` |
| Handlers | `<Comando>Handler` | `CrearPresupuestoHandler` |
| DTOs | `<Entidad>Dto` | `PresupuestoDto` |
| Carpeta por caso de uso | una carpeta agrupa command + handler + validator | `CrearPresupuesto/` |

### Frontend (Flutter / Dart)

| Elemento | Convención | Ejemplo |
| --- | --- | --- |
| Ficheros y carpetas | `snake_case` | `presupuesto_page.dart` |
| Clases | `PascalCase` | `PresupuestoViewModel` |
| Providers (Riverpod) | `camelCase` + sufijo `Provider` | `presupuestoProvider` |
| ViewModels | `<Feature>ViewModel` | `MedicionViewModel` |
| Carpetas de feature | `snake_case`, en plural | `presupuestos/`, `mediciones/` |

### Reglas transversales

- **Una funcionalidad, un directorio**: tanto en backend (carpeta por caso de uso) como en frontend (carpeta por feature).
- **Contratos hacia adentro**: las dependencias siempre apuntan hacia capas más internas; nunca al revés.
- **Lo compartido sube de nivel**: si dos features o capas necesitan algo común, vive en `shared/`, `core/` o `src/shared/contracts/`, nunca duplicado.
- **Tests reflejan la estructura**: la jerarquía de `test/` y de los proyectos `Tests.*` replica la del código que prueban.
