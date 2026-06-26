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

#### Endpoints
| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/clientes` | Crear cliente |
| `GET` | `/api/clientes` | Listar clientes |
| `POST` | `/api/proyectos` | Crear proyecto |
| `GET` | `/api/proyectos` | Listar proyectos |
| `GET` | `/api/proyectos/{id}` | Obtener proyecto |
| `PATCH` | `/api/proyectos/{id}/estado` | Cambiar estado (transición validada) |
| `POST` | `/api/presupuestos/calcular` | Calcular totales de un presupuesto (sin persistir) |
| `GET` | `/health` | Health check |

Ejecutar la API: `dotnet run --project PreventiviApp.Api` (crea `preventivi.db` en dev y
expone Swagger en `/swagger`).

## Pendiente (siguientes incrementos del MVP)
- Persistencia completa del **árbol de presupuesto** (Presupuesto/Capítulo/Partida/Medición)
  con EF Core (mapeo de colecciones, owned types y árbol jerárquico).
- **Importador DCF** (ver [`../../docs/11-importador-dcf.md`](../../docs/11-importador-dcf.md)).
- Generación de **PDF** (QuestPDF) y navegación en el **frontend Flutter**.

## Estructura
```
PreventiviApp.Domain         # Entidades, value objects, servicios de cálculo (sin dependencias)
PreventiviApp.Application     # Casos de uso, DTOs, mappers (→ Domain)
PreventiviApp.Infrastructure  # EF Core, repos, DCF, documentos (→ Application)
PreventiviApp.Api             # Web API, controllers, DI (→ Infrastructure)
PreventiviApp.Tests.Unit      # Tests de dominio/cálculo (→ Domain)
PreventiviApp.Tests.Integration
```
