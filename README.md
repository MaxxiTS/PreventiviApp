# Preventivi App

Aplicación moderna de **presupuestos y mediciones de obra** (estilo Primus), más
intuitiva, rápida y preparada para crecer. Importa preciosarios **DCF**, permite
navegar capítulos y partidas, consultar análisis de precios, crear presupuestos y
mediciones, generar documentos (PDF/Excel/CSV) y funcionar **offline-first** con
sincronización posterior a la nube.

> Estado: **Fase de diseño / arquitectura**. Este repositorio contiene la
> documentación técnica completa y el esqueleto de proyecto sobre el que se
> construirá el MVP.

---

## 🧱 Stack tecnológico (decisión canónica)

| Capa | Tecnología |
|------|-----------|
| Frontend | **Flutter** (Desktop · Mobile · Web) + **Riverpod** (estado) |
| Backend | **.NET 9 Web API** · Clean Architecture · **MediatR** (CQRS) |
| ORM | **Entity Framework Core** |
| BD local | **SQLite** (offline-first) |
| BD servidor | **PostgreSQL** |
| Sincronización | Cola de cambios + **SignalR/WebSockets** + Background Sync |
| Documentos | QuestPDF (PDF), ClosedXML (Excel), exportadores CSV/JSON/XML |
| IA | Búsqueda semántica (embeddings) + LLM para lenguaje natural |

Justificación de cada decisión en [`docs/02-arquitectura.md`](docs/02-arquitectura.md)
y en los [ADR](docs/adr/).

---

## 📚 Documentación

| # | Documento | Contenido |
|---|-----------|-----------|
| 00 | [Visión y alcance](docs/00-vision-y-alcance.md) | Producto, alcance, MVP, métricas |
| 01 | [Análisis del dominio](docs/01-analisis-dominio.md) | Entidades, agregados, glosario DDD |
| 02 | [Arquitectura](docs/02-arquitectura.md) | Clean Architecture, capas, patrones, diagrama de módulos |
| 03 | [Modelo ER y base de datos](docs/03-modelo-er-base-datos.md) | ER, esquema, índices, SQLite/PostgreSQL |
| 04 | [Estructura de carpetas](docs/04-estructura-carpetas.md) | Organización del monorepo |
| 05 | [Casos de uso](docs/05-casos-de-uso.md) | Use cases por módulo |
| 06 | [Historias de usuario](docs/06-historias-usuario.md) | Backlog y criterios de aceptación |
| 07 | [Mockups UX/UI](docs/07-mockups-ux-ui.md) | Pantallas, design system, dark/light |
| 08 | [Flujo de navegación](docs/08-flujo-navegacion.md) | Mapa de navegación end-to-end |
| 09 | [Diseño de APIs REST](docs/09-api-rest.md) | Endpoints, contratos, versionado |
| 10 | [Sincronización offline/online](docs/10-sincronizacion-offline.md) | Estrategia offline-first |
| 11 | [Importador DCF](docs/11-importador-dcf.md) | Parser, validación, progreso, errores |
| 12 | [Motor de presupuestos y mediciones](docs/12-motor-presupuestos-mediciones.md) | Cálculo, fórmulas, versionado |
| 13 | [Generación de documentos](docs/13-generacion-documentos.md) | PDF, Excel, CSV, plantillas |
| 14 | [Buscador inteligente](docs/14-buscador-inteligente.md) | Índices, fuzzy, sinónimos, filtros |
| 15 | [Inteligencia Artificial](docs/15-inteligencia-artificial.md) | Búsqueda NL, sugerencias, estimación |
| 16 | [Plan de pruebas](docs/16-plan-pruebas.md) | Unitarias, integración, aceptación |
| 17 | [Estrategia de despliegue](docs/17-despliegue.md) | Desktop, Web, Mobile, CI/CD |
| 18 | [Roadmap](docs/18-roadmap.md) | MVP → v1.0 → futuro |
| 19 | [Seguridad](docs/19-seguridad.md) | Auth, roles, cifrado, auditoría |
| 20 | [Rendimiento y escalabilidad](docs/20-rendimiento-escalabilidad.md) | 500k+ partidas, lazy/virtual scrolling |

Las decisiones arquitectónicas relevantes se registran como
[ADR (Architecture Decision Records)](docs/adr/).

---

## 🗂️ Estructura del repositorio

```
PreventiviApp/
├── docs/                 # Documentación técnica (índice arriba)
│   ├── adr/              # Architecture Decision Records
│   └── diagramas/        # Diagramas (Mermaid/PlantUML)
├── src/
│   ├── backend/          # .NET 9 — Clean Architecture
│   │   ├── PreventiviApp.Domain/
│   │   ├── PreventiviApp.Application/
│   │   ├── PreventiviApp.Infrastructure/
│   │   ├── PreventiviApp.Api/
│   │   ├── PreventiviApp.Tests.Unit/
│   │   └── PreventiviApp.Tests.Integration/
│   ├── frontend/         # Flutter (feature-first + Riverpod)
│   └── shared/contracts/ # Contratos/DTOs compartidos
├── database/             # Esquema, migraciones, seeds
├── tools/dcf-importer/   # Herramienta/spec del importador DCF
└── scripts/              # Utilidades de desarrollo
```

Detalle completo en [`docs/04-estructura-carpetas.md`](docs/04-estructura-carpetas.md).

---

## 🚀 Cómo empezar (cuando exista código)

```bash
# Backend
cd src/backend && dotnet restore && dotnet run --project PreventiviApp.Api

# Frontend
cd src/frontend && flutter pub get && flutter run
```

## 🤝 Contribución

El desarrollo sigue Clean Architecture, SOLID y DDD. Toda decisión relevante se
documenta como ADR antes de implementarse. Ver
[`docs/16-plan-pruebas.md`](docs/16-plan-pruebas.md) para los estándares de calidad.
