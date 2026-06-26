# 0004 - SQLite local + PostgreSQL servidor, offline-first

## Estado

Aceptado

## Contexto

Preventivi App debe funcionar **a pie de obra sin conexión** (mediciones, fotos, consulta de preciosarios) y sincronizarse con la nube cuando haya red. Además:

- El presupuestista trabaja con preciosarios de **500.000+ partidas** que deben consultarse y buscarse de forma instantánea en local.
- Se requiere **búsqueda de texto** rápida sobre resúmenes y textos largos de partidas.
- El servidor debe soportar concurrencia multiusuario, colaboración, auditoría y funciones de **IA** (búsqueda semántica).
- El modelo de datos es el mismo en ambos extremos, salvo capacidades específicas del motor.

## Decisión

Adoptamos una estrategia **offline-first** con **dos motores de base de datos** accedidos mediante **EF Core**:

- **Base de datos local: SQLite** embebida en el cliente, con **FTS5** para búsqueda de texto completo sobre partidas. Es el origen de verdad mientras el dispositivo está offline.
- **Base de datos servidor: PostgreSQL**, con:
  - **pg_trgm** para búsqueda difusa/aproximada.
  - **tsvector** para búsqueda de texto completo.
  - **pgvector** para embeddings y búsqueda semántica de la IA (ver ADR 0007).

El cliente lee y escribe **siempre primero en SQLite**; los cambios se registran en una cola y se propagan a PostgreSQL mediante el mecanismo de sincronización (ver ADR 0005). Las claves primarias son **UUID v7** (ver ADR 0006) para evitar colisiones entre offline y servidor.

```mermaid
graph LR
    UI[Flutter UI] --> L[(SQLite local + FTS5)]
    L -- cola de cambios --> S[(PostgreSQL servidor)]
    S -- pull --> L
    S --> AI[pgvector / búsqueda semántica]
```

## Consecuencias

**Positivas**

- La app es plenamente funcional sin conexión; la latencia de lectura/escritura local es mínima.
- SQLite + FTS5 da búsqueda instantánea sobre grandes preciosarios en el dispositivo.
- PostgreSQL aporta concurrencia robusta, extensiones avanzadas (pg_trgm, tsvector, pgvector) y escalabilidad en servidor.
- EF Core como capa común reduce la duplicación del modelo de datos.

**Negativas / costes**

- Mantener **dos dialectos SQL** implica cuidar diferencias (tipos, FTS5 vs. tsvector, funciones) y, en ocasiones, código/migraciones específicos por proveedor.
- La sincronización bidireccional introduce **complejidad y posibles conflictos** (abordados en ADR 0005).
- SQLite tiene límites de concurrencia de escritura; aceptable al ser monousuario en el cliente.
- Doble juego de migraciones EF Core a mantener.

## Alternativas consideradas

- **Solo PostgreSQL (siempre online)**: elimina la complejidad de sync, pero **rompe el requisito offline-first** imprescindible a pie de obra. Descartada.
- **SQLite en ambos extremos (cliente y servidor)**: simplificaría el dialecto, pero SQLite no cubre la concurrencia multiusuario ni ofrece pgvector/pg_trgm/tsvector para la IA y la búsqueda avanzada en servidor. Descartada.
- **Realm / ObjectBox / Couchbase Lite** (bases embebidas con sync integrado): ofrecen sincronización "lista para usar", pero atan a un proveedor/servicio, no encajan con EF Core ni con PostgreSQL+pgvector y reducen el control sobre la resolución de conflictos. Descartada a favor de un mecanismo de sync propio (ADR 0005).
- **PostgreSQL embebido / PGlite en cliente**: homogeneizaría el dialecto, pero su madurez en escritorio/móvil y su huella son inferiores a SQLite para el caso offline. Descartada.
