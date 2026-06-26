# Base de datos

Recursos de base de datos de Preventivi App.

- **`schema/`** — Esquema canónico (DDL). El diseño completo, índices y diferencias
  SQLite/PostgreSQL están en [`../docs/03-modelo-er-base-datos.md`](../docs/03-modelo-er-base-datos.md).
- **`migrations/`** — Migraciones de EF Core (se generan con `dotnet ef migrations add`).
- **`seeds/`** — Datos semilla (unidades, IVA, roles/permisos, preciosario de ejemplo).

## Estrategia

| Entorno | Motor | Uso |
|---------|-------|-----|
| Local (cliente) | **SQLite** (FTS5, SQLCipher) | Offline-first |
| Servidor | **PostgreSQL** (pg_trgm, tsvector, pgvector) | Fuente de verdad + IA |

La sincronización entre ambas se describe en
[`../docs/10-sincronizacion-offline.md`](../docs/10-sincronizacion-offline.md).
