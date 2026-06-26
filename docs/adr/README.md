# Architecture Decision Records (ADR)

Este directorio contiene los **Architecture Decision Records** (Registros de Decisiones de Arquitectura) de **Preventivi App**, la aplicación de presupuestos y mediciones de obra estilo Primus (ACCA).

## ¿Qué es un ADR?

Un ADR documenta una **decisión arquitectónica significativa** junto con su contexto y consecuencias. El objetivo es dejar constancia de **por qué** se tomó una decisión, no solo de **qué** se decidió, para que el equipo (presente y futuro) entienda los compromisos asumidos sin tener que reconstruir el razonamiento desde cero.

Las decisiones triviales o fácilmente reversibles no necesitan ADR. Reservamos los ADR para decisiones que:

- Afectan a la estructura del sistema o sus límites.
- Son costosas de revertir.
- Implican un compromiso (trade-off) relevante entre alternativas.

## Formato

Cada ADR sigue una estructura estándar:

| Sección | Descripción |
|---|---|
| **Título** | Frase corta que resume la decisión, numerada (`NNNN-titulo`). |
| **Estado** | `Propuesto`, `Aceptado`, `Rechazado`, `Obsoleto` o `Sustituido por NNNN`. |
| **Contexto** | Fuerzas en juego: requisitos, restricciones, problema a resolver. |
| **Decisión** | Lo que se decide hacer, en voz activa. |
| **Consecuencias** | Resultados positivos, negativos y neutros derivados de la decisión. |
| **Alternativas consideradas** | Otras opciones evaluadas y por qué se descartaron. |

## Convenciones

- Numeración secuencial de cuatro dígitos: `0001`, `0002`, ...
- Un fichero por decisión. Los ADR son **inmutables**: una decisión que cambia no se edita, se crea un ADR nuevo que sustituye al anterior y se marca el antiguo como `Sustituido por NNNN`.
- Toda la documentación se redacta en **español**, con tono técnico y conciso.

## Índice

| # | Título | Estado |
|---|---|---|
| [0001](0001-clean-architecture-ddd.md) | Adoptar Clean Architecture + DDD | Aceptado |
| [0002](0002-flutter-frontend.md) | Flutter + Riverpod para Desktop/Mobile/Web | Aceptado |
| [0003](0003-dotnet-backend-cqrs.md) | Backend en .NET 9 con MediatR/CQRS | Aceptado |
| [0004](0004-sqlite-postgresql-offline-first.md) | SQLite local + PostgreSQL servidor, offline-first | Aceptado |
| [0005](0005-sincronizacion-cola-cambios.md) | Sincronización por cola de cambios + SignalR | Aceptado |
| [0006](0006-uuid-v7-claves.md) | UUID v7 como claves primarias | Aceptado |
| [0007](0007-ia-rag-claude-pgvector.md) | IA con RAG, embeddings en pgvector y modelos Claude | Aceptado |
