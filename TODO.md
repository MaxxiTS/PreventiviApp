# ✅ Lista de tareas — Preventivi App

Checklist global del proyecto. Estado a **2026-06-26**.

Leyenda: `[x]` completado · `[~]` parcial / en curso · `[ ]` pendiente

> ⚠️ **Nota de verificación**: todo el backend está escrito para .NET 9 y revisado por
> inspección, pero **no se ha compilado ni ejecutado** en este entorno (el SDK de .NET y su
> descarga están bloqueados por la política de red del entorno remoto). Pendiente validar con
> `dotnet build` / `dotnet test` en local.

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
- [~] Editar / duplicar / copiar partidas (editar **precio** hecho; duplicar/copiar pendiente)
- [x] Bloquear / desbloquear precios (doc 12 §5.3)
- [x] Actualizar precios de un presupuesto desde un preciosario, respetando bloqueados (doc 12 §5.2)
- [~] *Actualizar desde nuevo DCF*: reimportación del catálogo (doc 12 §6) — precios y
  obsolescencia hechos y testeados; alta automática de partidas/capítulos nuevos pendiente
- [ ] Comparar versiones de DCF y de presupuestos (diff)
- [ ] Versionado de presupuestos (snapshots)
- [ ] Undo/redo y autoguardado
- [ ] Plantillas, favoritos, etiquetas, comentarios, adjuntos
- [ ] Buscador inteligente (FTS5 local / tsvector + pg_trgm servidor)

---

## 5. Frontend (Flutter — pendiente)
- [ ] Esqueleto feature-first + Riverpod + go_router + tema dark/light
- [ ] BD local (Drift/SQLite) y capa de datos offline-first
- [ ] Lista de proyectos / abrir proyecto
- [ ] Importar DCF (con progreso/cancelación)
- [ ] Navegación de capítulos/partidas (árbol + virtual scrolling)
- [ ] Análisis de precios (inspector lateral)
- [ ] Editor de presupuesto + mediciones (tabla + fórmulas)
- [ ] Totales y resumen económico
- [ ] Previsualización / descarga de PDF
- [ ] Cliente HTTP de la API + manejo de errores
- [ ] Empaquetado Desktop (Win/macOS/Linux), Web y Mobile

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
1. Validar el backend en local: `dotnet build` / `dotnet test` / `dotnet run` y corregir errores de compilación.
2. Generar las **migraciones EF Core** y añadir **tests de integración** de la API.
3. Arrancar el **frontend Flutter** (sección 5).
