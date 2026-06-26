# Historias de Usuario — Preventivi App

Documento de historias de usuario organizado por **épicas**. Cada historia sigue el formato *Como [rol] quiero [acción] para [beneficio]*, con **criterios de aceptación en Gherkin** (Dado/Cuando/Entonces) y **prioridad MoSCoW** (Must/Should/Could/Won't).

## Roles del producto

| Rol | Descripción |
|-----|-------------|
| **admin** | Administra la organización, usuarios, roles y configuración global. |
| **jefe_obra** | Responsable de obra: gestiona proyectos, mediciones y seguimiento. |
| **presupuestista** | Crea y edita presupuestos, partidas, precios y descompuestos. |
| **lector** | Consulta presupuestos y documentos sin permisos de edición. |

## Convención de prioridades MoSCoW

- **Must**: imprescindible para el MVP.
- **Should**: importante, planificado para v1.0.
- **Could**: deseable, candidato a futuro.
- **Won't**: fuera del alcance actual (registrado para el roadmap).

---

## Épica 1 — Gestión de proyectos

### HU-1.1 — Crear proyecto
**Como** jefe_obra **quiero** crear un proyecto con cliente, dirección y fecha **para** organizar mis presupuestos por obra.

```gherkin
Dado que estoy autenticado con un rol con permiso de creación
Cuando completo nombre, cliente, dirección y fecha y guardo
Entonces el proyecto se crea en estado "borrador"
Y se le asigna un id UUID v7 y se registra creado_en
Y aparece en el listado de proyectos de mi organización
```
**Prioridad:** Must

### HU-1.2 — Cambiar estado del proyecto
**Como** jefe_obra **quiero** cambiar el estado del proyecto (borrador/activo/cerrado/archivado) **para** reflejar el ciclo de vida de la obra.

```gherkin
Dado un proyecto en estado "borrador"
Cuando lo cambio a "activo"
Entonces el estado se actualiza y se registra en el historial
Y los proyectos "archivado" se ocultan del listado principal por defecto
```
**Prioridad:** Must

### HU-1.3 — Listar y filtrar proyectos
**Como** presupuestista **quiero** listar, buscar y filtrar proyectos por estado y cliente **para** localizar rápidamente la obra correcta.

```gherkin
Dado que existen varios proyectos en mi organización
Cuando filtro por estado "activo" y cliente "ACME"
Entonces solo veo los proyectos que cumplen ambos criterios
Y el listado se carga con paginación/lazy loading
```
**Prioridad:** Must

### HU-1.4 — Gestionar clientes
**Como** presupuestista **quiero** dar de alta y editar clientes (nombre, NIF, contacto) **para** reutilizarlos en varios proyectos.

```gherkin
Dado el formulario de cliente
Cuando guardo un cliente con NIF válido
Entonces se almacena y queda disponible al crear proyectos
Y si el NIF está duplicado se muestra una advertencia
```
**Prioridad:** Should

### HU-1.5 — Etiquetar proyectos
**Como** jefe_obra **quiero** asignar etiquetas de color a los proyectos **para** clasificarlos visualmente.

```gherkin
Dado un proyecto existente
Cuando le asigno una o más etiquetas
Entonces las etiquetas se muestran en el listado
Y puedo filtrar proyectos por etiqueta
```
**Prioridad:** Could

---

## Épica 2 — Importación DCF

### HU-2.1 — Importar preciosario DCF
**Como** presupuestista **quiero** importar un archivo de preciosario en formato DCF **para** disponer de una base de precios actualizada.

```gherkin
Dado un archivo DCF válido
Cuando lo selecciono e inicio la importación
Entonces el sistema procesa el archivo en background
Y crea un Preciosario con nombre, version, fuente y origen_dcf
Y se calcula y almacena hash_archivo para control de duplicados
```
**Prioridad:** Must

### HU-2.2 — Importar preciosarios grandes sin bloquear la UI
**Como** presupuestista **quiero** importar preciosarios de más de 500.000 partidas **para** trabajar con catálogos completos sin congelar la aplicación.

```gherkin
Dado un preciosario con más de 500.000 partidas
Cuando ejecuto la importación
Entonces el procesamiento ocurre en background con barra de progreso
Y la interfaz permanece responsiva durante el proceso
Y al finalizar se notifica el resultado (importadas/errores)
```
**Prioridad:** Must

### HU-2.3 — Detectar y resolver reimportación
**Como** presupuestista **quiero** que al reimportar un DCF se detecte la versión existente **para** decidir entre actualizar o crear una nueva versión.

```gherkin
Dado un preciosario ya importado con cierto hash_archivo
Cuando importo un archivo con el mismo origen pero distinta versión
Entonces el sistema me ofrece "actualizar" o "crear nueva versión"
Y los precios marcados como bloqueado=true no se sobrescriben
```
**Prioridad:** Should

### HU-2.4 — Informe de validación de importación
**Como** presupuestista **quiero** ver un informe de validación tras importar **para** conocer filas rechazadas y advertencias.

```gherkin
Dado un archivo DCF con filas inválidas
Cuando termina la importación
Entonces se muestra un informe con líneas correctas, advertencias y errores
Y puedo exportar el informe
```
**Prioridad:** Should

### HU-2.5 — Importar otros formatos (BC3/IFC/BIM)
**Como** presupuestista **quiero** importar formatos BC3 (FIEBDC-3), IFC y BIM **para** integrar fuentes adicionales.

```gherkin
Dado un archivo BC3 válido
Cuando lo importo
Entonces se mapea al modelo de capítulos y partidas
```
**Prioridad:** Won't

---

## Épica 3 — Navegación de preciosarios

### HU-3.1 — Navegar el árbol de capítulos y partidas
**Como** presupuestista **quiero** navegar la jerarquía de capítulos, subcapítulos y partidas **para** explorar el preciosario.

```gherkin
Dado un preciosario importado
Cuando abro su árbol de capítulos
Entonces veo capítulos y subcapítulos según padre_id y orden
Y al expandir un capítulo se cargan sus partidas con lazy loading
```
**Prioridad:** Must

### HU-3.2 — Virtual scrolling en listados masivos
**Como** presupuestista **quiero** desplazarme por listas de cientos de miles de partidas con fluidez **para** trabajar sin esperas.

```gherkin
Dado un capítulo con decenas de miles de partidas
Cuando hago scroll
Entonces solo se renderizan las filas visibles (virtual scrolling)
Y el desplazamiento se mantiene fluido
```
**Prioridad:** Must

### HU-3.3 — Ver detalle de partida y su análisis de precios
**Como** presupuestista **quiero** ver el detalle de una partida con su descompuesto **para** entender la composición del precio.

```gherkin
Dada una partida seleccionada
Cuando abro su detalle
Entonces veo código, resumen, texto_largo, unidad y precio
Y veo el AnalisisPrecios (mano de obra, material, maquinaria, % costes indirectos)
```
**Prioridad:** Must

### HU-3.4 — Comparar versiones de un preciosario
**Como** presupuestista **quiero** comparar dos versiones del mismo preciosario **para** ver qué precios cambiaron.

```gherkin
Dadas dos versiones del mismo preciosario
Cuando ejecuto la comparación
Entonces veo partidas añadidas, eliminadas y con precio modificado
```
**Prioridad:** Could

---

## Épica 4 — Creación de presupuestos

### HU-4.1 — Crear presupuesto en un proyecto
**Como** presupuestista **quiero** crear un presupuesto dentro de un proyecto **para** estructurar el coste de la obra.

```gherkin
Dado un proyecto activo
Cuando creo un presupuesto con nombre
Entonces se crea con version 1, estado "borrador" y total 0
Y queda vinculado al proyecto_id
```
**Prioridad:** Must

### HU-4.2 — Añadir partidas del preciosario al presupuesto
**Como** presupuestista **quiero** arrastrar o seleccionar partidas del preciosario al presupuesto **para** componer el árbol de capítulos.

```gherkin
Dado un presupuesto abierto y un preciosario disponible
Cuando añado una partida desde el preciosario
Entonces se crea una PartidaPresupuesto dentro del CapituloPresupuesto correspondiente
Y su precio se copia desde el preciosario (puede divergir después)
```
**Prioridad:** Must

### HU-4.3 — Organizar capítulos del presupuesto
**Como** presupuestista **quiero** crear, reordenar y anidar capítulos del presupuesto **para** estructurarlo según la obra.

```gherkin
Dado un presupuesto con varios capítulos
Cuando reordeno o anido un capítulo
Entonces se actualiza padre_id y orden
Y los totales se recalculan respetando la jerarquía
```
**Prioridad:** Must

### HU-4.4 — Calcular total con costes indirectos e IVA
**Como** presupuestista **quiero** que el presupuesto sume con % de costes indirectos e IVA **para** obtener el total final correcto.

```gherkin
Dado un presupuesto con partidas y mediciones
Cuando se aplica el % de CostesIndirectos y el tipo de Iva
Entonces el total refleja (suma de importes + costes indirectos) + IVA
Y el total se actualiza al cambiar cualquier importe
```
**Prioridad:** Must

### HU-4.5 — Versionar presupuesto
**Como** jefe_obra **quiero** generar una nueva versión del presupuesto **para** conservar el histórico de ofertas.

```gherkin
Dado un presupuesto en estado "borrador" o "enviado"
Cuando creo una nueva versión
Entonces se incrementa version y se guarda un snapshot de la anterior
Y puedo consultar versiones previas
```
**Prioridad:** Should

---

## Épica 5 — Mediciones

### HU-5.1 — Añadir líneas de medición a una partida
**Como** jefe_obra **quiero** añadir líneas de medición (uds, largo, ancho, alto) a una partida **para** calcular cantidades reales.

```gherkin
Dada una PartidaPresupuesto
Cuando añado una LineaMedicion con uds, largo, ancho y alto
Entonces el parcial se calcula como uds × largo × ancho × alto
Y el total de la Medicion suma todos los parciales
```
**Prioridad:** Must

### HU-5.2 — Medir mediante fórmula
**Como** jefe_obra **quiero** introducir una fórmula en una línea de medición **para** calcular cantidades complejas.

```gherkin
Dada una LineaMedicion con una fórmula
Cuando la fórmula es válida
Entonces el parcial se calcula como resultado de la fórmula
Y si la fórmula es inválida se muestra un error y no se guarda
```
**Prioridad:** Should

### HU-5.3 — Importe de partida a partir de la medición
**Como** presupuestista **quiero** que el importe de la partida sea total_medición × precio **para** mantener coherencia entre cantidades y coste.

```gherkin
Dada una partida con total de medición calculado
Cuando cambia el total de medición o el precio
Entonces el importe de la partida se recalcula como total × precio
Y el total del capítulo y del presupuesto se actualizan
```
**Prioridad:** Must

### HU-5.4 — Comentar líneas de medición
**Como** jefe_obra **quiero** añadir comentarios a las líneas de medición **para** documentar el origen de las cantidades.

```gherkin
Dada una LineaMedicion
Cuando añado un comentario descriptivo
Entonces el comentario se guarda y se muestra junto a la línea
```
**Prioridad:** Could

---

## Épica 6 — Edición de partidas y precios

### HU-6.1 — Editar datos de una partida del presupuesto
**Como** presupuestista **quiero** editar resumen, texto largo y unidad de una partida del presupuesto **para** adaptarla a la obra.

```gherkin
Dada una PartidaPresupuesto
Cuando modifico su resumen, texto_largo o unidad y guardo
Entonces los cambios se persisten sin alterar el preciosario origen
Y la acción queda registrada en el historial
```
**Prioridad:** Must

### HU-6.2 — Modificar el precio de una partida
**Como** presupuestista **quiero** modificar el precio de una partida del presupuesto **para** ajustar la oferta sin cambiar el preciosario.

```gherkin
Dada una PartidaPresupuesto con precio heredado del preciosario
Cuando cambio su precio
Entonces el precio del presupuesto diverge del preciosario
Y el importe y los totales se recalculan
```
**Prioridad:** Must

### HU-6.3 — Bloquear precios frente a reimportación
**Como** presupuestista **quiero** marcar un precio como bloqueado **para** que no se sobrescriba al reimportar un DCF.

```gherkin
Dado un Precio con bloqueado=true
Cuando se reimporta el preciosario de origen
Entonces ese precio conserva su valor
Y se indica visualmente que está bloqueado
```
**Prioridad:** Must

### HU-6.4 — Editar el descompuesto de una partida
**Como** presupuestista **quiero** editar las líneas del descompuesto (recurso, rendimiento, cantidad, precio) **para** ajustar el análisis de precios.

```gherkin
Dado el descompuesto de una partida
Cuando edito el rendimiento o la cantidad de una línea
Entonces el importe de la línea se recalcula
Y el precio resultante de la partida se actualiza según el AnalisisPrecios
```
**Prioridad:** Should

### HU-6.5 — Gestionar recursos y proveedores
**Como** presupuestista **quiero** gestionar recursos (mano de obra, material, maquinaria) y proveedores **para** reutilizarlos en los descompuestos.

```gherkin
Dado el catálogo de recursos
Cuando creo o edito un recurso con su tipo y precio
Entonces queda disponible para usarse en descompuestos
```
**Prioridad:** Could

---

## Épica 7 — Buscador inteligente

### HU-7.1 — Buscar partidas por texto
**Como** presupuestista **quiero** buscar partidas por código o texto **para** encontrarlas rápidamente en catálogos enormes.

```gherkin
Dado un preciosario importado
Cuando busco por un término
Entonces obtengo resultados mediante búsqueda full-text (SQLite FTS5 / Postgres tsvector)
Y los resultados se devuelven con baja latencia pese al tamaño del catálogo
```
**Prioridad:** Must

### HU-7.2 — Búsqueda tolerante a errores tipográficos
**Como** presupuestista **quiero** que la búsqueda tolere errores de escritura **para** encontrar partidas aun con typos.

```gherkin
Dado un término con una errata
Cuando ejecuto la búsqueda
Entonces se aplican coincidencias aproximadas (pg_trgm)
Y se devuelven resultados relevantes ordenados por similitud
```
**Prioridad:** Should

### HU-7.3 — Filtros combinados en la búsqueda
**Como** presupuestista **quiero** combinar la búsqueda de texto con filtros (capítulo, unidad, rango de precio) **para** acotar resultados.

```gherkin
Dada una búsqueda de texto
Cuando añado filtros por capítulo y rango de precio
Entonces los resultados respetan texto y filtros simultáneamente
```
**Prioridad:** Should

---

## Épica 8 — IA

### HU-8.1 — Búsqueda semántica de partidas
**Como** presupuestista **quiero** buscar partidas describiendo lo que necesito en lenguaje natural **para** encontrar la unidad de obra adecuada sin conocer su código.

```gherkin
Dada una descripción en lenguaje natural
Cuando ejecuto la búsqueda semántica
Entonces el sistema usa embeddings y similitud vectorial (pgvector)
Y devuelve las partidas más afines ordenadas por relevancia
```
**Prioridad:** Should

### HU-8.2 — Sugerencia de partidas para un presupuesto
**Como** presupuestista **quiero** que la IA sugiera partidas relacionadas **para** acelerar la composición del presupuesto.

```gherkin
Dado un presupuesto en elaboración
Cuando solicito sugerencias
Entonces la IA propone partidas afines al contexto actual
Y puedo aceptarlas o descartarlas
```
**Prioridad:** Could

### HU-8.3 — Asistente conversacional sobre el presupuesto
**Como** jefe_obra **quiero** preguntar al asistente sobre el presupuesto en lenguaje natural **para** obtener resúmenes y respuestas rápidas.

```gherkin
Dado un presupuesto abierto
Cuando pregunto al asistente (LLM Claude) sobre totales o capítulos
Entonces recibo una respuesta basada en los datos del presupuesto
```
**Prioridad:** Could

### HU-8.4 — Generación automática de textos de partida
**Como** presupuestista **quiero** que la IA redacte el texto largo de una partida **para** ahorrar tiempo de documentación.

```gherkin
Dada una partida con resumen breve
Cuando pido generar el texto_largo
Entonces la IA propone un texto técnico que puedo editar antes de guardar
```
**Prioridad:** Won't

---

## Épica 9 — Generación de documentos

### HU-9.1 — Generar PDF del presupuesto
**Como** presupuestista **quiero** generar un PDF del presupuesto **para** entregarlo al cliente.

```gherkin
Dado un presupuesto completo
Cuando genero el documento PDF
Entonces se produce un PDF con QuestPDF que incluye capítulos, partidas, mediciones y totales
Y el documento refleja costes indirectos e IVA
```
**Prioridad:** Must

### HU-9.2 — Aplicar plantilla de documento
**Como** presupuestista **quiero** aplicar una plantilla al documento generado **para** mantener la imagen corporativa.

```gherkin
Dada una Plantilla de documento
Cuando genero el PDF aplicando esa plantilla
Entonces el documento usa el formato, encabezados y estilo de la plantilla
```
**Prioridad:** Should

### HU-9.3 — Generar documento de mediciones
**Como** jefe_obra **quiero** generar un documento de mediciones detallado **para** justificar las cantidades.

```gherkin
Dado un presupuesto con mediciones
Cuando genero el documento de mediciones
Entonces se incluyen las líneas con uds, largo, ancho, alto, fórmula y parcial
```
**Prioridad:** Should

---

## Épica 10 — Exportaciones

### HU-10.1 — Exportar a Excel
**Como** presupuestista **quiero** exportar el presupuesto a Excel **para** seguir trabajándolo en hojas de cálculo.

```gherkin
Dado un presupuesto
Cuando exporto a Excel
Entonces se genera un archivo .xlsx con ClosedXML con capítulos, partidas y totales
```
**Prioridad:** Must

### HU-10.2 — Exportar a CSV/JSON/XML
**Como** presupuestista **quiero** exportar a CSV, JSON o XML **para** integrar con otras herramientas.

```gherkin
Dado un presupuesto
Cuando elijo el formato CSV, JSON o XML y exporto
Entonces se genera el archivo en el formato seleccionado con la estructura del presupuesto
```
**Prioridad:** Should

### HU-10.3 — Exportar a BC3 (FIEBDC-3)
**Como** presupuestista **quiero** exportar en formato BC3 **para** intercambiar con otros programas de mediciones.

```gherkin
Dado un presupuesto
Cuando exporto a BC3
Entonces se genera un archivo FIEBDC-3 compatible
```
**Prioridad:** Won't

---

## Épica 11 — Offline / Sync

### HU-11.1 — Trabajar sin conexión
**Como** jefe_obra **quiero** seguir trabajando sin conexión a internet **para** no depender de la red en obra.

```gherkin
Dado que pierdo la conexión
Cuando creo o edito presupuestos y mediciones
Entonces los cambios se guardan en la BD local SQLite
Y la aplicación sigue siendo plenamente funcional
```
**Prioridad:** Must

### HU-11.2 — Sincronizar cambios al recuperar conexión
**Como** jefe_obra **quiero** que mis cambios se sincronicen al volver la conexión **para** mantener la nube actualizada.

```gherkin
Dada una ColaSincronizacion con operaciones pendientes
Cuando se recupera la conexión
Entonces el background sync envía los cambios al servidor
Y cada elemento pasa de "pendiente" a "enviado" y luego "confirmado"
```
**Prioridad:** Must

### HU-11.3 — Sincronización en tiempo real
**Como** presupuestista **quiero** ver los cambios de mis compañeros en tiempo real **para** colaborar sobre el mismo presupuesto.

```gherkin
Dado un presupuesto compartido y conexión activa
Cuando otro usuario realiza un cambio
Entonces recibo la actualización vía SignalR/WebSockets sin recargar
```
**Prioridad:** Should

### HU-11.4 — Resolver conflictos de sincronización
**Como** jefe_obra **quiero** resolver conflictos cuando dos usuarios editan lo mismo **para** no perder datos.

```gherkin
Dado un elemento en estado "conflicto" en la ColaSincronizacion
Cuando abro la resolución de conflictos
Entonces veo los datos en conflicto y puedo elegir qué versión conservar
```
**Prioridad:** Should

---

## Épica 12 — Seguridad y roles

### HU-12.1 — Autenticación de usuario
**Como** usuario **quiero** iniciar sesión con email y contraseña **para** acceder de forma segura.

```gherkin
Dado un usuario activo con credenciales válidas
Cuando inicio sesión
Entonces se valida el password_hash y obtengo acceso
Y si las credenciales son inválidas o el usuario está inactivo se deniega el acceso
```
**Prioridad:** Must

### HU-12.2 — Control de acceso por roles (RBAC)
**Como** admin **quiero** que cada rol tenga permisos definidos **para** controlar qué puede hacer cada usuario.

```gherkin
Dado un usuario con rol "lector"
Cuando intenta editar un presupuesto
Entonces la acción se deniega por falta de permiso
Y un usuario "presupuestista" sí puede editar
```
**Prioridad:** Must

### HU-12.3 — Gestionar usuarios y roles
**Como** admin **quiero** crear usuarios y asignarles roles **para** administrar mi organización.

```gherkin
Dado el panel de administración
Cuando creo un usuario y le asigno un rol
Entonces el usuario puede iniciar sesión con los permisos de ese rol
```
**Prioridad:** Should

### HU-12.4 — Aislamiento por organización
**Como** admin **quiero** que los datos estén aislados por organización **para** garantizar la privacidad multi-tenant.

```gherkin
Dado un usuario de la organización A
Cuando consulta proyectos
Entonces solo ve datos de su organizacion_id
Y nunca accede a datos de otra organización
```
**Prioridad:** Must

---

## Épica 13 — Historial / Undo-Redo / Autoguardado

### HU-13.1 — Autoguardado de cambios
**Como** presupuestista **quiero** que mis cambios se guarden automáticamente **para** no perder trabajo.

```gherkin
Dado que estoy editando un presupuesto
Cuando realizo un cambio
Entonces se guarda automáticamente sin acción manual
Y se muestra el estado de guardado
```
**Prioridad:** Must

### HU-13.2 — Deshacer y rehacer
**Como** presupuestista **quiero** deshacer y rehacer acciones **para** corregir errores rápidamente.

```gherkin
Dada una acción de edición realizada
Cuando pulso deshacer
Entonces se revierte el último cambio
Y al pulsar rehacer se vuelve a aplicar
```
**Prioridad:** Should

### HU-13.3 — Historial y auditoría de cambios
**Como** admin **quiero** consultar el historial de auditoría **para** saber quién cambió qué y cuándo.

```gherkin
Dada una entidad modificada
Cuando consulto su historial
Entonces veo usuario_id, accion, datos_antes, datos_despues y creado_en
```
**Prioridad:** Should

---

## Épica 14 — Plantillas / Favoritos / Comparación

### HU-14.1 — Crear presupuesto desde plantilla
**Como** presupuestista **quiero** crear un presupuesto a partir de una plantilla **para** reutilizar estructuras habituales.

```gherkin
Dada una Plantilla de presupuesto
Cuando creo un presupuesto desde ella
Entonces el nuevo presupuesto hereda los capítulos y partidas de la plantilla
```
**Prioridad:** Should

### HU-14.2 — Guardar presupuesto como plantilla
**Como** presupuestista **quiero** guardar un presupuesto como plantilla **para** reutilizarlo en futuras obras.

```gherkin
Dado un presupuesto existente
Cuando lo guardo como plantilla
Entonces se crea una Plantilla con su contenido y tipo
```
**Prioridad:** Should

### HU-14.3 — Marcar partidas como favoritas
**Como** presupuestista **quiero** marcar partidas como favoritas **para** acceder a ellas con rapidez.

```gherkin
Dada una partida del preciosario
Cuando la marco como favorita
Entonces aparece en mi lista de favoritos
Y puedo añadirla al presupuesto desde ahí
```
**Prioridad:** Could

### HU-14.4 — Comparar presupuestos o versiones
**Como** jefe_obra **quiero** comparar dos presupuestos o versiones **para** analizar diferencias de coste.

```gherkin
Dados dos presupuestos o dos versiones
Cuando ejecuto la comparación
Entonces veo diferencias por capítulo y partida con variación de importe
```
**Prioridad:** Could

---

## Resumen del backlog priorizado

Estimación relativa en **story points** (escala Fibonacci: 1, 2, 3, 5, 8, 13). Release: **MVP**, **v1.0** o **futuro**.

| ID | Historia | Épica | MoSCoW | Story Points | Release |
|----|----------|-------|--------|:---:|:---:|
| HU-1.1 | Crear proyecto | Gestión de proyectos | Must | 3 | MVP |
| HU-1.2 | Cambiar estado del proyecto | Gestión de proyectos | Must | 2 | MVP |
| HU-1.3 | Listar y filtrar proyectos | Gestión de proyectos | Must | 3 | MVP |
| HU-1.4 | Gestionar clientes | Gestión de proyectos | Should | 3 | v1.0 |
| HU-1.5 | Etiquetar proyectos | Gestión de proyectos | Could | 2 | futuro |
| HU-2.1 | Importar preciosario DCF | Importación DCF | Must | 8 | MVP |
| HU-2.2 | Importar preciosarios grandes sin bloquear UI | Importación DCF | Must | 8 | MVP |
| HU-2.3 | Detectar y resolver reimportación | Importación DCF | Should | 5 | v1.0 |
| HU-2.4 | Informe de validación de importación | Importación DCF | Should | 3 | v1.0 |
| HU-2.5 | Importar BC3/IFC/BIM | Importación DCF | Won't | 13 | futuro |
| HU-3.1 | Navegar árbol de capítulos y partidas | Navegación de preciosarios | Must | 5 | MVP |
| HU-3.2 | Virtual scrolling en listados masivos | Navegación de preciosarios | Must | 8 | MVP |
| HU-3.3 | Detalle de partida y análisis de precios | Navegación de preciosarios | Must | 3 | MVP |
| HU-3.4 | Comparar versiones de preciosario | Navegación de preciosarios | Could | 5 | futuro |
| HU-4.1 | Crear presupuesto en un proyecto | Creación de presupuestos | Must | 3 | MVP |
| HU-4.2 | Añadir partidas al presupuesto | Creación de presupuestos | Must | 5 | MVP |
| HU-4.3 | Organizar capítulos del presupuesto | Creación de presupuestos | Must | 5 | MVP |
| HU-4.4 | Calcular total con costes indirectos e IVA | Creación de presupuestos | Must | 5 | MVP |
| HU-4.5 | Versionar presupuesto | Creación de presupuestos | Should | 5 | v1.0 |
| HU-5.1 | Líneas de medición en partida | Mediciones | Must | 5 | MVP |
| HU-5.2 | Medir mediante fórmula | Mediciones | Should | 5 | v1.0 |
| HU-5.3 | Importe de partida desde medición | Mediciones | Must | 3 | MVP |
| HU-5.4 | Comentar líneas de medición | Mediciones | Could | 2 | futuro |
| HU-6.1 | Editar datos de partida | Edición de partidas y precios | Must | 3 | MVP |
| HU-6.2 | Modificar precio de partida | Edición de partidas y precios | Must | 3 | MVP |
| HU-6.3 | Bloquear precios frente a reimportación | Edición de partidas y precios | Must | 3 | MVP |
| HU-6.4 | Editar descompuesto de partida | Edición de partidas y precios | Should | 5 | v1.0 |
| HU-6.5 | Gestionar recursos y proveedores | Edición de partidas y precios | Could | 5 | futuro |
| HU-7.1 | Buscar partidas por texto (FTS) | Buscador inteligente | Must | 5 | MVP |
| HU-7.2 | Búsqueda tolerante a typos | Buscador inteligente | Should | 5 | v1.0 |
| HU-7.3 | Filtros combinados en búsqueda | Buscador inteligente | Should | 3 | v1.0 |
| HU-8.1 | Búsqueda semántica de partidas | IA | Should | 8 | v1.0 |
| HU-8.2 | Sugerencia de partidas | IA | Could | 8 | futuro |
| HU-8.3 | Asistente conversacional | IA | Could | 8 | futuro |
| HU-8.4 | Generación de textos de partida | IA | Won't | 5 | futuro |
| HU-9.1 | Generar PDF del presupuesto | Generación de documentos | Must | 5 | MVP |
| HU-9.2 | Aplicar plantilla de documento | Generación de documentos | Should | 5 | v1.0 |
| HU-9.3 | Generar documento de mediciones | Generación de documentos | Should | 3 | v1.0 |
| HU-10.1 | Exportar a Excel | Exportaciones | Must | 5 | MVP |
| HU-10.2 | Exportar a CSV/JSON/XML | Exportaciones | Should | 3 | v1.0 |
| HU-10.3 | Exportar a BC3 | Exportaciones | Won't | 8 | futuro |
| HU-11.1 | Trabajar sin conexión | Offline / Sync | Must | 8 | MVP |
| HU-11.2 | Sincronizar al recuperar conexión | Offline / Sync | Must | 13 | MVP |
| HU-11.3 | Sincronización en tiempo real | Offline / Sync | Should | 8 | v1.0 |
| HU-11.4 | Resolver conflictos de sincronización | Offline / Sync | Should | 8 | v1.0 |
| HU-12.1 | Autenticación de usuario | Seguridad y roles | Must | 3 | MVP |
| HU-12.2 | Control de acceso por roles (RBAC) | Seguridad y roles | Must | 5 | MVP |
| HU-12.3 | Gestionar usuarios y roles | Seguridad y roles | Should | 3 | v1.0 |
| HU-12.4 | Aislamiento por organización | Seguridad y roles | Must | 5 | MVP |
| HU-13.1 | Autoguardado de cambios | Historial / Undo-Redo | Must | 3 | MVP |
| HU-13.2 | Deshacer y rehacer | Historial / Undo-Redo | Should | 5 | v1.0 |
| HU-13.3 | Historial y auditoría | Historial / Undo-Redo | Should | 3 | v1.0 |
| HU-14.1 | Crear presupuesto desde plantilla | Plantillas / Favoritos | Should | 3 | v1.0 |
| HU-14.2 | Guardar presupuesto como plantilla | Plantillas / Favoritos | Should | 3 | v1.0 |
| HU-14.3 | Marcar partidas favoritas | Plantillas / Favoritos | Could | 2 | futuro |
| HU-14.4 | Comparar presupuestos o versiones | Plantillas / Favoritos | Could | 5 | futuro |

### Distribución por release

| Release | Historias | Story Points |
|---------|:---:|:---:|
| **MVP** | 24 | 119 |
| **v1.0** | 18 | 80 |
| **futuro** | 14 | 84 |
| **Total** | 56 | 283 |
