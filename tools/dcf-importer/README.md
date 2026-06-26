# Importador DCF

Herramienta/módulo para importar preciosarios en formato **DCF** (Primus/ACCA).

La especificación funcional y técnica completa (pipeline, parser en streaming,
validación, detección de errores, progreso/cancelación, mapeo a entidades del
dominio e idempotencia con precios bloqueados) está en
[`../../docs/11-importador-dcf.md`](../../docs/11-importador-dcf.md).

El importador se implementa como `IPriceBookImporter` en la capa Infrastructure,
con una estrategia por formato (`DcfImporter`, y en el futuro `Bc3Importer`,
`IfcImporter`) para soportar nuevos formatos sin rehacer la arquitectura.
