# Visión y Alcance — Preventivi App

> Documento canónico de visión y alcance. Define el "qué" y el "porqué" del producto.
> Todo el contenido está en español. Para detalles de arquitectura, modelo de datos y
> stack técnico, consultar los documentos específicos del repositorio.

## 1. Resumen ejecutivo

**Preventivi App** es una aplicación moderna de **presupuestos y mediciones de obra**,
concebida como una alternativa más intuitiva, rápida y abierta a **Primus (ACCA)**.
Permite importar preciosarios en formato **DCF**, navegar grandes bases de precios
(500.000+ partidas), construir presupuestos jerárquicos de capítulos y partidas, realizar
**mediciones** detalladas, gestionar el **análisis de precios (descompuesto)** de cada
partida y generar documentación profesional (PDF, Excel, CSV/JSON/XML).

La aplicación es **offline-first** con sincronización a la nube: el profesional puede
trabajar a pie de obra sin conexión y, al recuperar red, sus cambios se sincronizan de
forma automática y sin conflictos. La experiencia de usuario se inspira en herramientas
modernas de productividad (Notion, Linear, Figma, ClickUp).

| Atributo | Valor |
|---|---|
| Categoría | Software de presupuestos y mediciones de construcción |
| Referente del mercado | Primus (ACCA Software) |
| Plataformas | Desktop, Mobile y Web (Flutter) |
| Modelo de trabajo | Offline-first + sync a la nube |
| Idioma del producto y docs | Español |
| Formato de importación inicial | DCF (futuro: BC3 / FIEBDC-3, IFC, BIM) |
| Escala objetivo | Preciosarios de 500.000+ partidas |

## 2. Problema que resuelve

La elaboración de presupuestos y mediciones de obra es una tarea crítica en construcción:
de su precisión dependen los márgenes, la viabilidad de las ofertas y el control económico
de la obra. Las herramientas actuales líderes (Primus y similares) son potentes pero
presentan limitaciones recurrentes para el profesional:

| Dolor del usuario | Situación actual (Primus y similares) | Lo que ofrece Preventivi App |
|---|---|---|
| Curva de aprendizaje | Interfaces densas, heredadas, poco intuitivas | UX moderna inspirada en Notion/Linear/Figma |
| Trabajo en obra | Pensado para escritorio; débil en movilidad | Offline-first real en móvil, tablet y desktop |
| Rendimiento con grandes preciosarios | Lentitud al manejar cientos de miles de partidas | Lazy loading, virtual scrolling, caché e indexación |
| Colaboración y sincronización | Ficheros que se comparten manualmente | Sync automática a la nube + change log |
| Búsqueda | Búsquedas limitadas y poco flexibles | Búsqueda full-text (FTS5/tsvector) + semántica con IA |
| Apertura del dato | Formatos cerrados, dependencia del proveedor | Exportación a CSV/JSON/XML/Excel y PDF |
| Asistencia inteligente | Inexistente o muy básica | IA para sugerencias, búsqueda semántica y redacción |

En resumen, el problema es: **los presupuestistas necesitan una herramienta moderna,
rápida, móvil y abierta que mantenga la potencia del modelo de datos de Primus (capítulos,
partidas, descompuestos, mediciones, preciosarios DCF) sin su fricción ni sus
limitaciones.**

## 3. Objetivos de negocio

| ID | Objetivo de negocio | Indicador asociado |
|---|---|---|
| N1 | Posicionar Preventivi App como alternativa moderna y creíble a Primus | Adopción / cuota en estudios y constructoras |
| N2 | Reducir la barrera de entrada frente a soluciones heredadas | Tiempo hasta el primer presupuesto completado |
| N3 | Captar profesionales que ya usan preciosarios DCF | Nº de preciosarios DCF importados con éxito |
| N4 | Generar ingresos recurrentes mediante planes por organización | MRR / conversión free→pago |
| N5 | Construir una base de datos abierta e interoperable de precios | Volumen de partidas gestionadas y exportadas |
| N6 | Diferenciarse por movilidad e IA | Uso de funciones móviles e IA por usuario activo |

## 4. Objetivos de producto

| ID | Objetivo de producto | Descripción |
|---|---|---|
| P1 | Importación fiable de DCF | Importar preciosarios DCF completos preservando jerarquía y descompuestos |
| P2 | Navegación fluida a gran escala | Navegar 500.000+ partidas sin degradación perceptible de rendimiento |
| P3 | Presupuestos jerárquicos versionables | Crear, editar y versionar presupuestos como árboles de capítulos/partidas |
| P4 | Mediciones precisas | Calcular cantidades por líneas (uds × largo × ancho × alto o fórmula) |
| P5 | Análisis de precios completo | Editar descompuestos (mano de obra, material, maquinaria, costes indirectos) |
| P6 | Documentación profesional | Generar PDF, Excel y exportaciones estructuradas listas para cliente |
| P7 | Offline-first con sync sin conflictos | Trabajar sin conexión y sincronizar de forma automática y trazable |
| P8 | Búsqueda potente | Encontrar partidas por texto completo y por semántica (IA) |
| P9 | Multi-proyecto y multi-organización | Gestionar varios proyectos y equipos con control de acceso por roles |

## 5. Alcance del MVP

### 5.1 Dentro de alcance (in-scope)

| Área | Funcionalidad incluida en el MVP |
|---|---|
| Importación | Importar preciosarios **DCF** (capítulos, partidas, descompuestos, recursos, unidades, precios) |
| Navegación | Árbol de **capítulos/subcapítulos** y **partidas** con lazy loading y virtual scrolling |
| Análisis de precios | Visualizar y editar el **descompuesto** de cada partida (mano de obra, material, maquinaria, % costes indirectos) |
| Presupuestos | Crear presupuestos por **proyecto**, como árbol de capítulos/partidas, **versionables** |
| Mediciones | Capturar **líneas de medición** (uds, largo, ancho, alto, fórmula, parcial) y totalizar |
| Edición | Editar partidas, precios y descompuestos; **bloquear precios** para que no se actualicen al reimportar DCF |
| Cálculo económico | Aplicar **costes indirectos** e **IVA**; totalizar capítulos, partidas y presupuesto |
| Documentos | Generar **PDF** (QuestPDF) y **Excel** (ClosedXML) de presupuestos y mediciones |
| Exportación | Exportar a **CSV / JSON / XML** |
| Multi-proyecto | Gestión de varios **proyectos** y **clientes** por organización |
| Offline | Funcionamiento **offline-first** con BD local **SQLite (FTS5)** |
| Sincronización | **Change log** (cola de sincronización) + sync en background a la nube |
| Buscador | Búsqueda **full-text** sobre partidas y preciosarios (FTS5 local / tsvector servidor) |
| IA (básica) | **Búsqueda semántica** (pgvector) y asistencia de redacción/sugerencias |
| Usuarios y roles | Autenticación y **RBAC** con roles: admin, jefe_obra, presupuestista, lector |

### 5.2 Fuera de alcance (out-of-scope) del MVP

| Área | Excluido del MVP | Comentario |
|---|---|---|
| Formatos adicionales | Importación/exportación **BC3 (FIEBDC-3)**, **IFC**, **BIM** | Planificado post-MVP |
| Certificaciones de obra | Certificaciones y seguimiento económico de ejecución | Fase posterior |
| Planificación temporal | Diagramas de Gantt / cronogramas de obra | Fuera del núcleo de presupuestación |
| Contabilidad / facturación | Facturación, contabilidad y gestión financiera completa | Integración futura |
| Marketplace de preciosarios | Tienda/intercambio de preciosarios entre usuarios | Visión a largo plazo |
| Colaboración en tiempo real | Edición simultánea multiusuario tipo Figma sobre el mismo presupuesto | Sync sí; co-edición en vivo, no |
| IA avanzada | Generación automática de presupuestos completos por IA | Iteración posterior |
| Integraciones externas | ERP, CRM, plataformas de licitación pública | Bajo demanda |

## 6. Personas / usuarios objetivo

| Persona | Rol (RBAC) | Contexto | Necesidades principales |
|---|---|---|---|
| **Presupuestista** | `presupuestista` | Elabora ofertas y presupuestos en estudio | Importar DCF, montar presupuestos rápido, editar precios y descompuestos, generar PDF/Excel, búsqueda potente |
| **Jefe de obra** | `jefe_obra` | Trabaja a pie de obra, a menudo sin conexión | Consultar y ajustar mediciones en móvil/tablet, offline-first, adjuntar fotos/planos, sync fiable |
| **Arquitecto / Aparejador** | `presupuestista` o `lector` | Redacta proyecto y supervisa mediciones | Precisión en mediciones y unidades, control de capítulos/partidas, exportar documentación técnica |
| **Administrador** | `admin` | Gestiona la organización, equipos y datos | Gestión de usuarios y roles (RBAC), control de proyectos/organización, auditoría e historial, control de versiones |

### 6.1 Detalle de necesidades por persona

- **Presupuestista**: velocidad y precisión. Necesita pasar del preciosario al presupuesto
  con el mínimo de clics, reutilizar partidas, ver el análisis de precios y entregar
  documentos profesionales al cliente.
- **Jefe de obra**: movilidad y fiabilidad. Necesita capturar y revisar mediciones en obra,
  adjuntar evidencias (fotos, planos), y confiar en que todo se sincroniza al recuperar red.
- **Arquitecto / Aparejador**: rigor técnico. Necesita coherencia de unidades, control de la
  estructura de capítulos y partidas, y trazabilidad de las mediciones del proyecto.
- **Administrador**: control y gobierno. Necesita gestionar accesos, planes de la
  organización, auditoría de cambios (datos antes/después) y el versionado de los presupuestos.

## 7. Propuesta de valor y diferenciadores vs Primus

**Propuesta de valor:** *La potencia del modelo de presupuestos de Primus, con la
experiencia, la movilidad y la inteligencia de una herramienta moderna.*

| Diferenciador | Primus (referente) | Preventivi App |
|---|---|---|
| Experiencia de usuario | Densa, heredada | Moderna (Notion / Linear / Figma / ClickUp) |
| Plataformas | Principalmente desktop | Desktop + Mobile + Web (Flutter) |
| Trabajo sin conexión | Limitado | Offline-first nativo con sync |
| Sincronización | Manual / por fichero | Automática (change log + SignalR/WebSockets) |
| Rendimiento a gran escala | Costoso con grandes bases | 500.000+ partidas con lazy loading e indexación |
| Búsqueda | Básica | Full-text + semántica con IA (pgvector) |
| IA | Sin asistencia | Sugerencias, redacción y búsqueda semántica (Claude) |
| Apertura del dato | Formatos cerrados | Exportación CSV/JSON/XML/Excel/PDF |
| Versionado | Limitado | Presupuestos versionables + auditoría/historial |

## 8. Lista priorizada de funcionalidades

Prioridad: **P0** = imprescindible para el MVP, **P1** = alta (cercano al MVP),
**P2** = media (post-MVP temprano), **P3** = futura.

| Prioridad | Funcionalidad | Descripción |
|---|---|---|
| P0 | **Importar DCF** | Importación de preciosarios DCF con jerarquía, descompuestos, recursos y unidades |
| P0 | **Navegar capítulos / partidas** | Árbol jerárquico con lazy loading y virtual scrolling a gran escala |
| P0 | **Análisis de precios (descompuesto)** | Visualizar/editar mano de obra, material, maquinaria y % costes indirectos |
| P0 | **Presupuestos** | Crear presupuestos por proyecto como árbol de capítulos/partidas |
| P0 | **Mediciones** | Líneas de medición (uds × largo × ancho × alto o fórmula) y totalización |
| P0 | **Edición de partidas / precios** | Editar resúmenes, textos, precios; bloqueo de precios frente a reimportación |
| P0 | **Multi-proyecto** | Gestión de varios proyectos y clientes por organización |
| P0 | **Offline-first** | Trabajo sin conexión con BD local SQLite (FTS5) |
| P0 | **Buscador** | Búsqueda full-text sobre partidas y preciosarios |
| P1 | **Generación de PDF** | Documentos profesionales de presupuesto y medición (QuestPDF) |
| P1 | **Exportación** | CSV / JSON / XML / Excel (ClosedXML) |
| P1 | **Sincronización (sync)** | Change log + SignalR/WebSockets + background sync, sin conflictos |
| P1 | **Versionado de presupuestos** | Versiones, snapshots, historial y auditoría |
| P2 | **IA — búsqueda semántica** | Embeddings + pgvector para encontrar partidas por significado |
| P2 | **IA — asistencia** | Sugerencias y redacción asistida con modelos Claude |
| P2 | **Plantillas** | Plantillas de presupuestos y documentos reutilizables |
| P3 | **Importación BC3 / IFC / BIM** | Formatos adicionales de intercambio |

## 9. Métricas de éxito / KPIs

| Categoría | KPI | Objetivo orientativo |
|---|---|---|
| Activación | Tiempo hasta el primer presupuesto completado | < 30 min desde alta |
| Adopción de importación | Preciosarios DCF importados con éxito por usuario | ≥ 1 en la primera semana |
| Rendimiento | Tiempo de carga/scroll en preciosarios de 500.000+ partidas | < 100 ms percibido (virtual scroll) |
| Fiabilidad de sync | Cambios sincronizados sin conflicto | ≥ 99% |
| Productividad | Tiempo medio para montar un presupuesto vs herramienta previa | -30% |
| Calidad de documentos | PDFs/Excel generados sin reedición manual | ≥ 90% |
| Uso de búsqueda | Búsquedas (full-text + semántica) por sesión | Tendencia creciente |
| Retención | Usuarios activos mensuales que vuelven | ≥ 60% MAU/registro |
| Movilidad | % de mediciones capturadas en móvil/tablet | Tendencia creciente |
| IA | Aceptación de sugerencias de IA | ≥ 40% de sugerencias útiles |

## 10. Restricciones y supuestos

### 10.1 Restricciones

| ID | Restricción |
|---|---|
| R1 | Stack fijo: Flutter + Riverpod (frontend), .NET 9 Web API con Clean Architecture (backend) |
| R2 | Persistencia: EF Core; SQLite (FTS5) local y PostgreSQL (pg_trgm, tsvector, pgvector) en servidor |
| R3 | Identificadores **UUID v7** en todas las entidades sincronizables |
| R4 | Toda la documentación, interfaz y dominio en **español** |
| R5 | Arquitectura **offline-first**; el cliente debe ser plenamente funcional sin conexión |
| R6 | Rendimiento objetivo: preciosarios de **500.000+ partidas** |
| R7 | Documentos mediante QuestPDF (PDF) y ClosedXML (Excel); export CSV/JSON/XML |
| R8 | IA basada en modelos Claude (Opus 4.8, Sonnet 4.6, Haiku 4.5) y embeddings sobre pgvector |

### 10.2 Supuestos

| ID | Supuesto |
|---|---|
| S1 | Los usuarios disponen de preciosarios en formato DCF para importar |
| S2 | El público objetivo conoce el modelo de Primus (capítulos, partidas, descompuestos, mediciones) |
| S3 | Existe conectividad intermitente; la sync se realiza cuando hay red disponible |
| S4 | Las organizaciones gestionan sus propios usuarios y roles (RBAC) |
| S5 | El formato DCF es el punto de partida; BC3/IFC/BIM se incorporarán después |
| S6 | Los precios pueden divergir entre preciosario y presupuesto (precio de partida instanciado) |

## 11. Glosario inicial del dominio

| Término | Definición |
|---|---|
| **Capítulo** | Agrupación jerárquica de partidas dentro de un preciosario o presupuesto; puede contener subcapítulos (jerarquía padre/hijo). |
| **Partida** | Unidad de obra con código, resumen, texto largo, unidad y precio. Es el elemento básico que se mide y valora. |
| **Descompuesto** | Línea del análisis de precios de una partida: recurso, rendimiento, cantidad, precio unitario e importe. |
| **Análisis de precios** | Composición de una partida en mano de obra, materiales y maquinaria, más un % de costes indirectos. Vista agregada del descompuesto. |
| **Recurso** | Elemento que compone un descompuesto: mano de obra, material, maquinaria u otros (tabla base con discriminador). |
| **Medición** | Cálculo de las cantidades de una partida en un presupuesto, compuesto por líneas de medición. |
| **Línea de medición** | Detalle de medición: comentario, uds (nº de iguales), largo, ancho, alto, fórmula y parcial (uds × largo × ancho × alto o resultado de la fórmula). |
| **Preciosario** | Base de datos de precios (capítulos, partidas, descompuestos) importada desde DCF; tiene versión, fuente y hash de archivo. |
| **DCF** | Formato de intercambio de preciosarios usado para la importación de precios. Punto de partida; futuro: BC3, IFC, BIM. |
| **BC3 (FIEBDC-3)** | Formato estándar de intercambio de bases de datos de construcción. Soporte previsto post-MVP. |
| **Presupuesto** | Árbol de capítulos y partidas con sus mediciones para un proyecto; suma costes indirectos e IVA. Es versionable. |
| **Proyecto** | Trabajo de obra para un cliente, con uno o varios presupuestos; tiene estado (borrador/activo/cerrado/archivado). |
| **Costes indirectos** | Porcentaje aplicado al presupuesto para repercutir costes no imputables directamente a una partida. |
| **IVA** | Impuesto aplicado al presupuesto según tipo configurable. |
| **Precio bloqueado** | Precio marcado para que no se actualice al reimportar el preciosario DCF. |
| **Coeficiente de rendimiento** | Coeficiente aplicado en mediciones/descompuestos para ajustar rendimientos. |
| **Versión** | Snapshot de una entidad (p. ej. un presupuesto) que permite trazar y recuperar estados previos. |
| **Auditoría / Historial** | Registro de acciones con datos antes/después para trazabilidad y control. |
| **Cola de sincronización (change log)** | Registro de operaciones offline (insert/update/delete) pendientes de sincronizar con el servidor. |
| **Offline-first** | Estrategia en la que la app funciona plenamente sin conexión y sincroniza al recuperar red. |
| **RBAC** | Control de acceso basado en roles (admin, jefe_obra, presupuestista, lector) y permisos. |
| **Búsqueda semántica** | Búsqueda por significado mediante embeddings y similitud vectorial (pgvector). |
