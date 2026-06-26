# 0006 - UUID v7 como claves primarias

## Estado

Aceptado

## Contexto

En un sistema **offline-first** con sincronización bidireccional (ADR 0004 y 0005), los clientes crean entidades **sin conexión con el servidor**. Necesitamos una estrategia de identificadores que:

- Permita **generar IDs en el cliente** sin coordinación con el servidor, evitando colisiones al sincronizar.
- Sea **única globalmente** entre múltiples dispositivos y la nube.
- No degrade el rendimiento de inserción ni la localidad de índices en SQLite y PostgreSQL, crítico con preciosarios de 500.000+ partidas.
- No exponga conteos ni secuencias adivinables (a diferencia de los enteros autoincrementales).

Los **enteros autoincrementales** (IDENTITY/SERIAL) quedan descartados de raíz: no se pueden generar offline sin riesgo de colisión. Los **UUID v4** (aleatorios) resuelven la unicidad pero, al no ser ordenables, **fragmentan los índices B-tree** y empeoran inserciones y localidad de caché.

## Decisión

Adoptamos **UUID versión 7 (UUIDv7)** como tipo de **clave primaria** para todas las entidades del dominio.

UUIDv7 es un UUID de 128 bits cuyo prefijo es una **marca temporal Unix en milisegundos**, seguido de bits aleatorios. Esto lo hace:

- **Único globalmente** y generable en cualquier cliente offline.
- **Ordenable cronológicamente** (k-sortable): los nuevos IDs se insertan al final del índice, preservando la localidad y minimizando la fragmentación de páginas en B-tree.

En PostgreSQL se almacenan en columnas `uuid`; en SQLite, como `BLOB` de 16 bytes o `TEXT` canónico según convenga al rendimiento. La generación se realiza en la capa de Domain/Infrastructure mediante una librería de UUIDv7.

## Consecuencias

**Positivas**

- Generación de claves **100% offline** sin coordinación ni colisiones, habilitando la sincronización del ADR 0005.
- Inserciones y rangos por tiempo más eficientes que con UUID v4 gracias al orden temporal del prefijo.
- No revela cardinalidad ni permite enumeración secuencial (mejor que enteros autoincrementales en seguridad).
- El propio ID aporta una marca temporal aproximada de creación, útil para depuración y orden por defecto.

**Negativas / costes**

- **16 bytes por clave** frente a 4-8 de un entero: índices y claves foráneas más grandes; impacto acotado y aceptable para el volumen previsto.
- Menos legibles/depurables que enteros pequeños en URLs y logs.
- UUIDv7 codifica el **instante de creación**, lo que filtra información temporal; aceptable para este dominio.
- Requiere una librería de generación compatible y consistente en cliente (Dart) y servidor (.NET).

## Alternativas consideradas

- **Enteros autoincrementales (IDENTITY/SERIAL)**: óptimos en tamaño y localidad, pero **imposibles de generar offline sin colisión**. Descartados por incompatibilidad con offline-first.
- **UUID v4 (aleatorio)**: únicos y generables offline, pero su aleatoriedad **fragmenta los índices** y penaliza inserciones masivas (preciosarios grandes). Descartado a favor de v7.
- **ULID**: equivalente funcional a UUIDv7 (ordenable por tiempo), pero usa representación Base32 propia y no es un UUID estándar, con peor soporte nativo en PostgreSQL/EF Core. Descartado a favor del estándar UUIDv7.
- **Snowflake IDs**: ordenables y compactos (64 bits), pero requieren coordinación de IDs de nodo/worker, poco práctica para clientes offline heterogéneos. Descartado.
