# Backend — Preventivi App (.NET 9)

Backend en **Clean Architecture** (ver [`../../docs/02-arquitectura.md`](../../docs/02-arquitectura.md)).

## Estado del MVP

Primer incremento del MVP centrado en el **núcleo de dominio y el motor de cálculo**
(hito M2 del [roadmap](../../docs/18-roadmap.md): *Presupuesto calculado*), que el
roadmap marca como **riesgo crítico** y exige validar con tests.

### Implementado (`PreventiviApp.Domain`)
- **Common**: `Entity`, `AuditableEntity` (auditoría + control de sync), `Result`/`Result<T>`
  (Result Pattern), `Error`, `Redondeo` (reglas de redondeo del doc 12 §2.3).
- **Enums**: `EstadoProyecto`, `EstadoPresupuesto`, `TipoRecurso`.
- **Entidades**: catálogo (`Preciosario`, `Capitulo`, `Partida`, `Descompuesto`, `Recurso`,
  `Unidad`, `Precio`) y presupuesto (`Proyecto`, `Cliente`, `Presupuesto`,
  `CapituloPresupuesto`, `PartidaPresupuesto`, `Medicion`, `LineaMedicion`, `Iva`).
- **Servicios de cálculo**:
  - `EvaluadorFormula` — analizador descendente recursivo de fórmulas (`+ - * /`,
    paréntesis, variables, `PI`, funciones `sqrt/pow/abs/round/min/max/sin/cos`).
  - `EvaluadorMedicion` — parcial dimensional o por fórmula + coeficiente; total de medición.
  - `CalculadoraPresupuesto` — precio desde descompuesto, importe de partida, subtotal de
    capítulo (con dirty tracking) y cierre económico (PEM → baja → descuentos → IVA → total).

### Tests (`PreventiviApp.Tests.Unit`)
Validan el motor **contra los ejemplos numéricos documentados** en
[`docs/12-motor-presupuestos-mediciones.md`](../../docs/12-motor-presupuestos-mediciones.md):
- `EvaluadorFormulaTests` — aritmética, precedencia, funciones, variables y errores.
- `EvaluadorMedicionTests` — parciales dimensionales, neutro, comentario, coeficiente,
  fórmula y totales (ejemplos §1.3 y §3.5).
- `CalculadoraPresupuestoTests` — precio de partida (§2.2 → 37,64), importes (§1.3 → 608,85;
  §3.5 → 2.204,68), subtotales con caché y cierre económico (§4.5 → total 104.350,40).

> ⚠️ **Verificación pendiente de ejecución.** El SDK de .NET no está disponible en el
> entorno donde se generó este código (la descarga está bloqueada por la política de red
> del entorno remoto), por lo que **estos tests no se han compilado ni ejecutado aquí**.
> El código está escrito para .NET 9 y revisado manualmente. Ejecuta localmente:
> ```bash
> cd src/backend
> dotnet test
> ```

### Aplicación, infraestructura y API (vertical slice)
- **`PreventiviApp.Application`** (CQRS con MediatR + FluentValidation + Result Pattern):
  - Abstracciones de persistencia (`IUnitOfWork`, `IClienteRepository`, `IProyectoRepository`).
  - Casos de uso: Clientes (`CrearCliente`, `ListarClientes`), Proyectos (`CrearProyecto`,
    `ListarProyectos`, `ObtenerProyecto`, `CambiarEstadoProyecto`) y `CalcularPresupuesto`
    (sin estado, reutiliza el motor de dominio).
  - `ValidationBehavior` (pipeline) y `AddApplication` (DI).
- **`PreventiviApp.Infrastructure`** (EF Core + SQLite):
  - `AppDbContext` (implementa `IUnitOfWork`), configuraciones de `Cliente`/`Proyecto`,
    repositorios, `AppDbContextFactory` (migraciones) y `AddInfrastructure`.
- **`PreventiviApp.Api`** (ASP.NET Core):
  - `ClientesController`, `ProyectosController`, `PresupuestosController`.
  - `ApiControllerBase` (traduce `Result` → 200/404/409/400 ProblemDetails) y
    `ValidationExceptionHandler`. Wiring en `Program.cs` (Swagger, EnsureCreated en dev).

### Catálogo (preciosario), importador DCF y documentos
- **Persistencia EF Core** del catálogo completo (Preciosario, Capitulo, Partida, Descompuesto,
  Recurso, Unidad, Precio) y del **árbol de presupuesto** (Presupuesto, CapituloPresupuesto
  jerárquico, PartidaPresupuesto, Medicion, LineaMedicion) — incl. mapeo de colecciones por
  backing field, one-to-one Partida↔Medición, autorreferencia de capítulos y reconstrucción
  del árbol en el repositorio.
- **Importador DCF** (`DcfImporter`): parser de texto por líneas, en streaming, tolerante a
  errores, con progreso, cancelación y hash SHA256; mapea a las entidades canónicas. Formato
  documentado y `tools/dcf-importer/ejemplo.dcf` de muestra.
- **Generación de PDF** del presupuesto con **QuestPDF** (`GeneradorPresupuestoPdf`): capítulos,
  tablas de partidas, subtotales y resumen económico, reutilizando el motor de cálculo.

#### Endpoints
| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/clientes` · `GET` `/api/clientes` | Crear / listar clientes |
| `POST` `/api/proyectos` · `GET` `/api/proyectos` · `GET` `/api/proyectos/{id}` | Proyectos |
| `PATCH` | `/api/proyectos/{id}/estado` | Cambiar estado (transición validada) |
| `POST` | `/api/preciosarios/importar` | Importar preciosario DCF (multipart) |
| `GET` | `/api/preciosarios` | Listar preciosarios |
| `GET` | `/api/preciosarios/{id}/capitulos?padreId=` | Navegar capítulos/subcapítulos |
| `GET` | `/api/preciosarios/capitulos/{id}/partidas` | Partidas de un capítulo |
| `GET` | `/api/preciosarios/partidas/{id}/analisis` | Análisis de precios (descompuesto) |
| `POST` | `/api/presupuestos` | Crear presupuesto |
| `GET` | `/api/presupuestos?proyectoId=` | Listar presupuestos del proyecto |
| `GET` | `/api/presupuestos/{id}` | Presupuesto con importes y totales |
| `POST` | `/api/presupuestos/{id}/capitulos` | Añadir capítulo |
| `POST` | `/api/presupuestos/capitulos/{id}/partidas` | Añadir partida + mediciones |
| `GET` | `/api/presupuestos/{id}/pdf` | Generar PDF del presupuesto |
| `POST` | `/api/presupuestos/calcular` | Calcular totales sin persistir |
| `GET` | `/health` | Health check |

Ejecutar la API: `dotnet run --project PreventiviApp.Api` (crea `preventivi.db` en dev y
expone Swagger en `/swagger`).

## Pendiente (siguientes incrementos del MVP)
- Edición/duplicado de partidas, bloqueo de precios y *Actualizar desde nuevo DCF*.
- Versionado y comparación de presupuestos; exportación Excel/CSV.
- **Frontend Flutter** (escritorio/web/móvil) y sincronización cloud (v1.0).
- Migraciones EF Core formales (hoy se usa `EnsureCreated` en desarrollo).

## Estructura
```
PreventiviApp.Domain         # Entidades, value objects, servicios de cálculo (sin dependencias)
PreventiviApp.Application     # Casos de uso, DTOs, mappers (→ Domain)
PreventiviApp.Infrastructure  # EF Core, repos, DCF, documentos (→ Application)
PreventiviApp.Api             # Web API, controllers, DI (→ Infrastructure)
PreventiviApp.Tests.Unit      # Tests de dominio/cálculo (→ Domain)
PreventiviApp.Tests.Integration
```
