# ✅ Lista de tareas — Preventivi App

Checklist global del proyecto. Estado a **2026-06-26**.

Leyenda: `[x]` completado · `[~]` parcial / en curso · `[ ]` pendiente

> ⚠️ **Nota de verificación**: todo el código (backend .NET 9 y frontend Flutter) está
> escrito y revisado por inspección, pero **no se ha compilado ni ejecutado** en este entorno
> (no hay SDK de .NET ni de Flutter, y su descarga está bloqueada por la política de red del
> entorno remoto). Pendiente validar en local con `dotnet build`/`dotnet test` y
> `flutter pub get`/`flutter run`.

---

## 1. Documentación técnica
- [x] Visión y alcance (`docs/00`)
- [x] Análisis del dominio / DDD (`docs/01`)
- [x] Arquitectura (Clean Architecture, módulos) (`docs/02`)
- [x] Modelo ER y base de datos (`docs/03`)
- [x] Estructura de carpetas (`docs/04`)
- [x] Casos de uso (`docs/05`)
- [x] Historias de usuario (`docs/06`)
- [x] Mockups UX/UI (`docs/07`)
- [x] Flujo de navegación (`docs/08`)
- [x] Diseño de API REST (`docs/09`)
- [x] Sincronización offline/online (`docs/10`)
- [x] Importador DCF — especificación (`docs/11`)
- [x] Motor de presupuestos y mediciones (`docs/12`)
- [x] Generación de documentos (`docs/13`)
- [x] Buscador inteligente (`docs/14`)
- [x] Inteligencia Artificial (`docs/15`)
- [x] Plan de pruebas (`docs/16`)
- [x] Estrategia de despliegue (`docs/17`)
- [x] Roadmap (`docs/18`)
- [x] Seguridad (`docs/19`)
- [x] Rendimiento y escalabilidad (`docs/20`)
- [x] ADRs 0001–0007 (`docs/adr`)

## 2. Estructura del repositorio
- [x] Esqueleto de carpetas (backend, frontend, database, tools, scripts, CI)
- [x] README raíz + README backend
- [x] `.gitignore`
- [x] Workflow CI de GitHub Actions (esqueleto)

---

## 3. Backend — MVP (.NET 9)

### 3.1. Dominio + motor de cálculo
- [x] Common: `Entity`, `AuditableEntity`, `Result`/`Result<T>`, `Error`, `Redondeo`
- [x] Enums: `EstadoProyecto`, `EstadoPresupuesto`, `TipoRecurso`
- [x] Entidades de catálogo (Preciosario, Capitulo, Partida, Descompuesto, Recurso, Unidad, Precio)
- [x] Entidades de presupuesto (Proyecto, Cliente, Presupuesto, CapituloPresupuesto, PartidaPresupuesto, Medicion, LineaMedicion, Iva)
- [x] `EvaluadorFormula` (parser de fórmulas)
- [x] `EvaluadorMedicion` (parciales y totales)
- [x] `CalculadoraPresupuesto` (precio, importe, subtotales, cierre económico, dirty tracking)

### 3.2. Tests
- [x] Tests unitarios del motor de cálculo (fórmulas, mediciones, presupuesto) validados contra el doc 12
- [x] Test del handler `CalcularPresupuesto` (DTO→dominio→motor)
- [x] Tests de gestión de precios (bloqueo + actualización masiva desde preciosario)
- [x] Tests de conciliación de reimportación DCF (alta/actualizar/respetar/obsoleta)
- [x] Tests de duplicado (copia profunda), nueva versión y comparación (diff + delta)
- [ ] Ejecutar la suite (`dotnet test`) y confirmar verde — **pendiente (sin SDK aquí)**
- [ ] Tests de integración de la API (WebApplicationFactory / Testcontainers)
- [ ] Tests del importador DCF (archivos de muestra y casos de error)

### 3.3. Application (CQRS)
- [x] Abstracciones de persistencia (UnitOfWork + repositorios)
- [x] `ValidationBehavior` + `AddApplication` (DI)
- [x] Clientes: crear, listar
- [x] Proyectos: crear, listar, obtener, cambiar estado
- [x] Preciosarios: importar DCF, listar, navegar capítulos/partidas, análisis de precios
- [x] Presupuestos: crear, añadir capítulo, añadir partida (+mediciones), obtener árbol con totales, listar por proyecto, generar PDF, calcular sin persistir

### 3.4. Infrastructure (EF Core + SQLite)
- [x] `AppDbContext` + `IUnitOfWork`
- [x] Configuraciones de catálogo y árbol de presupuesto
- [x] Repositorios (Cliente, Proyecto, Preciosario, Presupuesto)
- [x] `AddInfrastructure` (DI) + `AppDbContextFactory`
- [ ] Migraciones EF Core formales (hoy `EnsureCreated` en dev)
- [ ] Configurar proveedor PostgreSQL para servidor (cadena/entorno)

### 3.5. Importador DCF
- [x] Contrato `IImportadorPreciosario` + modelos (progreso, errores, resultado)
- [x] `DcfImporter` (parser de texto, streaming, tolerante, hash, cancelación)
- [x] Archivo de ejemplo `tools/dcf-importer/ejemplo.dcf`
- [ ] Soporte de formato real DCF binario / BC3 (FIEBDC-3) vía Strategy
- [~] Reimportación respetando precios bloqueados (conciliación de precios/obsolescencia hecha; alta de nuevos pendiente)

### 3.6. Documentos
- [x] `IGeneradorPresupuestoPdf` + `GeneradorPresupuestoPdf` (QuestPDF)
- [ ] Otros informes (mediciones, descompuestos, listados de materiales/mano de obra)
- [ ] Exportación Excel (ClosedXML), CSV, JSON, XML

### 3.7. API REST
- [x] `ApiControllerBase` (Result → HTTP) + `ValidationExceptionHandler`
- [x] Controladores: Clientes, Proyectos, Preciosarios, Presupuestos
- [x] `Program.cs` (Swagger, enums string, EnsureCreated en dev)
- [ ] Autenticación JWT + autorización por roles (RBAC)
- [ ] Paginación/keyset en listados grandes
- [ ] Versionado de API y documentación OpenAPI pulida

---

## 4. Funcionalidad de presupuestos (avanzada)
- [x] Editar precio de partida
- [x] Duplicar partidas y capítulos (copia profunda)
- [ ] Copiar partidas/capítulos **entre** presupuestos distintos
- [x] Bloquear / desbloquear precios (doc 12 §5.3)
- [x] Actualizar precios de un presupuesto desde un preciosario, respetando bloqueados (doc 12 §5.2)
- [x] Versionado de presupuestos (nueva versión / snapshot, doc 12 §7.1)
- [x] Comparar versiones de presupuestos (diff por código, doc 12 §7.2)
- [~] *Actualizar desde nuevo DCF*: reimportación del catálogo (doc 12 §6) — precios y
  obsolescencia hechos y testeados; alta automática de partidas/capítulos nuevos pendiente
- [ ] Comparar versiones de **DCF** (diff de catálogo)
- [ ] Undo/redo y autoguardado
- [ ] Plantillas, favoritos, etiquetas, comentarios, adjuntos
- [ ] Buscador inteligente (FTS5 local / tsvector + pg_trgm servidor)

---

## 5. Frontend (Flutter — scaffold + flujo de presupuestos interactivo)
- [x] Esqueleto feature-first + Riverpod + go_router + tema dark/light (Material 3)
- [x] Shell con NavigationRail (Proyectos / Preciosarios) + home
- [x] Cliente HTTP (Dio) + manejo de carga/error con `AsyncValue` + errores ProblemDetails
- [x] Lista de proyectos + crear (POST) + abrir detalle
- [x] Navegación de preciosarios: capítulos → partidas → análisis de precios
- [x] Vista de presupuesto: resumen económico + árbol de capítulos/partidas
- [x] Crear presupuesto desde la UI (nombre, IVA, costes indirectos, baja, descuentos)
- [x] Añadir capítulos/subcapítulos y partidas con mediciones multi-línea (uds/largo/ancho/alto/coef/fórmula)
- [x] Gestión de precios en UI (editar / bloquear-desbloquear / actualizar desde preciosario)
- [x] Duplicar partidas y capítulos, crear nueva versión y comparar versiones (tabla diff + delta)
- [x] Descargar PDF del presupuesto
- [~] Importar DCF desde la UI (selección de archivo + resumen hecho; progreso/cancelación pendiente)
- [~] Editor de partidas/mediciones (alta hecha; edición de líneas existentes pendiente)
- [ ] BD local (Drift/SQLite) y capa de datos offline-first
- [ ] Virtual scrolling para preciosarios grandes
- [ ] Previsualización embebida del PDF
- [ ] Empaquetado Desktop (Win/macOS/Linux), Web y Mobile

> ⚠️ El frontend tampoco se ha compilado aquí (sin Flutter SDK). Validar con
> `cd src/frontend && flutter pub get && flutter run -d chrome` (o `-d windows`).

---

## 6. Sincronización cloud (v1.0 — pendiente)
- [ ] `ColaSincronizacion` (change log) + control de versiones
- [ ] Motor de sync (push/pull delta) + SignalR/WebSockets
- [ ] Resolución de conflictos
- [ ] PostgreSQL servidor como fuente de verdad

## 7. Seguridad (v1.0 — pendiente)
- [ ] Autenticación (JWT + refresh) y hashing de contraseñas
- [ ] RBAC (admin, jefe_obra, presupuestista, lector) + multi-tenant
- [ ] Cifrado en tránsito/reposo (SQLCipher local) y auditoría
- [ ] Backups y cumplimiento (GDPR)

## 8. Inteligencia Artificial (v2.0 — pendiente)
- [ ] Embeddings + pgvector + búsqueda híbrida
- [ ] Búsqueda en lenguaje natural y sugerencia de partidas
- [ ] Detección de duplicados, estimación de costes, explicación de partidas

## 9. DevOps / despliegue
- [~] CI básico de GitHub Actions (esqueleto creado; pendiente activarlo con código compilable)
- [ ] Pipelines de build/test/release por plataforma
- [ ] Contenedor Docker del backend + PostgreSQL gestionada
- [ ] Observabilidad (Serilog, health checks, métricas)
- [ ] Estrategia de migraciones y backups en producción

---

## Próximo paso recomendado
1. **Validar la compilación en local** (backend `dotnet build`/`dotnet test`/`dotnet run`;
   frontend `flutter pub get`/`flutter run`) y corregir errores.
2. Generar las **migraciones EF Core** y añadir **tests de integración** de la API.
3. Completar el **editor de presupuesto en el frontend** (alta/edición de partidas y
   mediciones; gestión de precios; duplicar/versionar/comparar) e **importar DCF desde la UI**.
4. Sincronización cloud + seguridad/RBAC (v1.0).
