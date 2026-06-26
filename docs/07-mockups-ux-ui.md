# Mockups y Diseño UX/UI — Preventivi App

Documento de diseño de experiencia e interfaz para la aplicación de **presupuestos y mediciones de obra** (estilo Primus/ACCA, inspiración Notion, Linear, Figma y ClickUp). Define principios, design system, layout, wireframes de las pantallas principales, dark/light mode, responsive, accesibilidad y atajos de teclado.

El objetivo es una herramienta **densa en datos pero respirable**, capaz de manejar preciosarios de **500.000+ partidas** sin perder fluidez, con la sensación de rapidez de Linear, la flexibilidad estructural de Notion y la precisión de una hoja de cálculo profesional.

---

## 1. Principios de diseño y referencias

### 1.1 Principios rectores

| Principio | Descripción | Cómo se aplica |
|-----------|-------------|----------------|
| **Velocidad percibida** | La UI nunca bloquea. Lazy loading, virtual scrolling, optimistic UI. | Linear: navegación instantánea, skeletons en lugar de spinners. |
| **Densidad con jerarquía** | Mucha información por pantalla, pero ordenada por contraste y espaciado, no por ruido. | Figma/ClickUp: tablas compactas, jerarquía tipográfica clara. |
| **Estructura como contenido** | El árbol de capítulos es navegación y dato a la vez. Arrastrar, anidar, plegar. | Notion: bloques anidables, drag & drop. |
| **Teclado primero** | Todo lo frecuente tiene atajo. El ratón es opcional para el usuario experto. | Linear: command palette (Cmd/Ctrl+K), navegación con flechas. |
| **Offline-first sin fricción** | El estado de sincronización es visible pero no intrusivo. | Indicador discreto de sync, edición siempre disponible. |
| **Errores como guía** | Validación inline, mensajes claros, sin callejones sin salida. | FluentValidation reflejado en UI con mensajes en español. |
| **Confianza en los números** | Cálculos (parciales, totales, IVA) siempre visibles y trazables. | Coherencia visual entre medición → importe → total. |

### 1.2 Referencias adaptadas a construcción

- **Notion** → árbol de capítulos/subcapítulos anidable, drag & drop, propiedades editables inline.
- **Linear** → command palette, atajos, transiciones rápidas, estados de proyecto (borrador/activo/cerrado/archivado) como pills.
- **Figma** → panel inspector lateral con propiedades contextuales (aquí: análisis de precios de la partida seleccionada).
- **ClickUp** → tablas densas con columnas configurables, agrupación por capítulo, totales por grupo.

---

## 2. Design System

### 2.1 Paleta de colores

Color de marca **azul técnico** (confianza, ingeniería), acentos de color reservados para estado y categorías de recursos (mano de obra, material, maquinaria).

#### Light mode

| Token | Hex | Uso |
|-------|-----|-----|
| `--bg-base` | `#FFFFFF` | Fondo de paneles principales |
| `--bg-subtle` | `#F7F8FA` | Fondo de app / sidebar |
| `--bg-muted` | `#EEF0F4` | Filas alternas, hovers suaves |
| `--border` | `#E2E5EA` | Bordes de tablas, separadores |
| `--border-strong` | `#CBD0D9` | Bordes de inputs activos |
| `--text-primary` | `#1A1D23` | Texto principal |
| `--text-secondary` | `#5A6472` | Texto secundario, labels |
| `--text-muted` | `#8B93A1` | Placeholders, metadatos |
| `--primary` | `#2563EB` | Botones primarios, enlaces, foco |
| `--primary-hover` | `#1D4ED8` | Hover primario |
| `--primary-subtle` | `#EAF1FE` | Fondo de selección, chips activos |
| `--success` | `#16A34A` | Sincronizado, validado |
| `--warning` | `#D97706` | Pendiente de sync, precio no vigente |
| `--danger` | `#DC2626` | Errores, eliminar, conflicto |
| `--info` | `#0891B2` | Avisos informativos |

#### Dark mode

| Token | Hex | Uso |
|-------|-----|-----|
| `--bg-base` | `#16181D` | Fondo de paneles principales |
| `--bg-subtle` | `#0F1115` | Fondo de app / sidebar |
| `--bg-muted` | `#1E2128` | Filas alternas, hovers |
| `--border` | `#2A2E37` | Bordes de tablas, separadores |
| `--border-strong` | `#3A404B` | Bordes de inputs activos |
| `--text-primary` | `#E8EAED` | Texto principal |
| `--text-secondary` | `#A4ABB8` | Texto secundario, labels |
| `--text-muted` | `#6B7280` | Placeholders, metadatos |
| `--primary` | `#3B82F6` | Botones primarios, enlaces, foco |
| `--primary-hover` | `#60A5FA` | Hover primario |
| `--primary-subtle` | `#1A2436` | Fondo de selección, chips activos |
| `--success` | `#22C55E` | Sincronizado, validado |
| `--warning` | `#F59E0B` | Pendiente de sync, precio no vigente |
| `--danger` | `#EF4444` | Errores, eliminar, conflicto |
| `--info` | `#22D3EE` | Avisos informativos |

#### Colores semánticos de recursos (ambos modos)

| Recurso | Light | Dark | Icono |
|---------|-------|------|-------|
| Mano de obra (`mano_obra`) | `#7C3AED` | `#A78BFA` | 👷 / icono casco |
| Material (`material`) | `#0D9488` | `#2DD4BF` | 🧱 / icono ladrillo |
| Maquinaria (`maquinaria`) | `#EA580C` | `#FB923C` | 🚜 / icono máquina |
| Otros (`otros`) | `#64748B` | `#94A3B8` | ◇ / icono diamante |

### 2.2 Tipografía

- **Familia UI**: `Inter` (fallback: system-ui, -apple-system, Segoe UI, Roboto).
- **Familia mono** (códigos, fórmulas, importes alineados): `JetBrains Mono` (fallback: `SF Mono`, `Consolas`).
- Los importes y cantidades numéricas usan **tabular figures** para alineación en columnas.

| Estilo | Tamaño / Línea | Peso | Uso |
|--------|----------------|------|-----|
| Display | 28 / 36 | 700 | Títulos de pantalla grandes |
| H1 | 22 / 30 | 600 | Título de proyecto / presupuesto |
| H2 | 18 / 26 | 600 | Capítulos, secciones de inspector |
| H3 | 15 / 22 | 600 | Subcapítulos, agrupadores |
| Body | 14 / 20 | 400 | Texto general, celdas de tabla |
| Body-strong | 14 / 20 | 600 | Resúmenes de partida, totales |
| Small | 12 / 16 | 400 | Metadatos, ayuda, breadcrumb |
| Mono | 13 / 18 | 400/500 | Códigos, importes, fórmulas |

### 2.3 Escala de espaciado

Base de **4 px**. Tokens: `space-1`=4, `space-2`=8, `space-3`=12, `space-4`=16, `space-5`=20, `space-6`=24, `space-8`=32, `space-10`=40, `space-12`=48.

- **Densidad de tabla**: altura de fila 32 px (compacta) / 40 px (cómoda), conmutable.
- **Padding de celda**: 8 px vertical × 12 px horizontal.
- **Radio de borde**: `radius-sm`=4, `radius-md`=8, `radius-lg`=12, `radius-full`=9999 (pills/chips).
- **Sombras**: `shadow-sm` (cards), `shadow-md` (popovers/dropdowns), `shadow-lg` (modales).

### 2.4 Iconografía

- Set base: **Lucide / Phosphor** (línea 1.5 px, esquinas redondeadas, 16/20/24 px).
- Iconos clave del dominio: carpeta (capítulo), documento-líneas (partida), calculadora (medición), pirámide/desglose (análisis de precios), casco (mano de obra), ladrillo (material), máquina (maquinaria), nube/flecha (sync), versión/rama (versionado), PDF, Excel.
- Estados de sync con punto de color: verde (confirmado), ámbar (pendiente/enviado), rojo (conflicto).

### 2.5 Componentes base

```
BOTONES
┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌────┐
│  Primario    │  │  Secundario  │  │   Texto      │  │ ⌫  │
│ (azul lleno) │  │ (borde)      │  │ (sin fondo)  │  │icon│
└──────────────┘  └──────────────┘  └──────────────┘  └────┘
 Estados: default · hover · active · disabled · loading(⟳)
```

| Componente | Notas de comportamiento |
|-----------|--------------------------|
| **Botón** | Primario / secundario / fantasma / icono / peligro. Loading con spinner inline. |
| **Input** | Borde 1 px, foco con anillo `--primary` 2 px. Variantes: texto, número (con alineación derecha + tabular), búsqueda (icono lupa). Validación inline con mensaje rojo bajo el campo. |
| **Select / Dropdown** | Búsqueda interna si >8 opciones. Para unidades, IVA, tipo de recurso. |
| **Tabla** | Cabecera sticky, columnas redimensionables y reordenables, ordenación por columna, selección de fila/celda, edición inline (doble clic o Enter), filas de subtotal por capítulo, virtual scrolling. |
| **Árbol (tree)** | Nodos plegables (▸/▾), indentación por nivel, drag & drop para reordenar/reanidar, lazy load de hijos, contador de partidas por capítulo. |
| **Tabs** | Subrayado animado en activo. Usadas en el inspector (Resumen/Análisis/Mediciones/Adjuntos/Historial). |
| **Breadcrumb** | Organización › Proyecto › Presupuesto › Capítulo › Subcapítulo › Partida. Cada segmento es navegable; truncado con `…` y menú overflow. |
| **Chips / Pills** | Estado de proyecto/presupuesto, etiquetas (color de `Etiqueta`), tipo de recurso. Cerrables cuando son filtros. |
| **Modal / Dialog** | Overlay con `shadow-lg`, foco atrapado, cierre con Esc. Para crear proyecto, importar DCF, confirmar borrado, exportar. |
| **Toast** | Esquina inferior derecha, auto-cierre 4 s. Éxito/aviso/error. |
| **Command palette** | Overlay central (Cmd/Ctrl+K) para navegación y acciones globales. |
| **Skeleton** | Placeholders animados durante carga de árboles/tablas grandes. |

---

## 3. Layout principal

```
┌──────┬───────────────────────────────────────────────────────────────────────┐
│      │  TOPBAR: ⌂ Org › Proyecto › Presupuesto      🔍 Buscar (Ctrl+K)   ◑ ☁● │
│ SIDE ├───────────────┬───────────────────────────────────┬───────────────────┤
│ BAR  │               │                                   │                   │
│      │   EXPLORER    │        PANEL CENTRAL              │     INSPECTOR     │
│ ⌂    │   (árbol de   │   (tabla de partidas /            │   (detalle /     │
│ 📁   │   capítulos)  │    presupuesto)                   │    análisis de   │
│ 💰   │               │                                   │    precios)      │
│ 📚   │               │                                   │                   │
│ 🔍   │               │                                   │                   │
│ ⚙    │               │                                   │                   │
│      │               │                                   │                   │
│      ├───────────────┴───────────────────────────────────┴───────────────────┤
│      │  STATUSBAR: 1.248 partidas · Total 84.320,50 € · ☁ Sincronizado 12:04 │
└──────┴───────────────────────────────────────────────────────────────────────┘
```

| Zona | Función |
|------|---------|
| **Sidebar de navegación** (izquierda, ~56 px colapsada / 220 px expandida) | Iconos de las áreas principales: Inicio, Proyectos, Presupuestos, Preciosarios (biblioteca), Búsqueda IA, Ajustes. Avatar de usuario y organización abajo. Colapsable con `[` . |
| **Topbar** | Breadcrumb contextual a la izquierda; búsqueda global (Ctrl+K) al centro; a la derecha: conmutador light/dark (◑), estado de sync (☁ con punto de color), notificaciones, avatar. |
| **Explorer / Árbol de capítulos** (~280 px) | Árbol jerárquico de `Capitulo`/subcapítulos del preciosario o presupuesto activo. Plegable, drag & drop, búsqueda interna, contador de partidas. Botón "+" para nuevo capítulo/partida. |
| **Panel central** | Vista principal: tabla de partidas del capítulo seleccionado (modo preciosario) o tabla de presupuesto con mediciones e importes (modo presupuesto). Virtual scrolling, edición inline, subtotales por capítulo. |
| **Inspector lateral** (~360 px, colapsable) | Detalle de la entidad seleccionada. Tabs: Resumen, Análisis de precios (descompuesto), Mediciones, Adjuntos, Comentarios, Historial/Versiones. |
| **Breadcrumb** | Ruta navegable completa Org → Proyecto → Presupuesto → Capítulo → Partida. |
| **Búsqueda global** | Acceso por Ctrl+K. Busca proyectos, capítulos, partidas, recursos. Combina FTS5 (local) / tsvector (servidor) + semántica (pgvector) en la pestaña IA. |
| **Tabs** | Dentro del inspector y para cambiar entre presupuestos/versiones de un proyecto. |
| **Statusbar** | Recuento de partidas, total acumulado con IVA, estado y hora de última sincronización. |

---

## 4. Mockups de pantallas principales (wireframes)

### 4.1 Lista de proyectos

```
┌──────┬─────────────────────────────────────────────────────────────────────────┐
│ ⌂    │  Proyectos                                  🔍 Ctrl+K     ◑   ☁●   (MB)  │
│ 📁 ▸ ├─────────────────────────────────────────────────────────────────────────┤
│ 💰   │  [ + Nuevo proyecto ]   [ ⤓ Importar DCF ]      Filtro: [Todos ▾] [⊞|≣]  │
│ 📚   │ ───────────────────────────────────────────────────────────────────────  │
│ 🔍   │  NOMBRE                 CLIENTE        ESTADO      TOTAL        ACTUALIZ. │
│ ⚙    │ ───────────────────────────────────────────────────────────────────────  │
│      │  ▸ Reforma Calle Mayor  Const. Ríos   ● Activo    142.300,00€  hace 2h   │
│      │  ▸ Nave Industrial L4   Logís. SA     ● Activo     89.150,40€  ayer      │
│      │  ▸ Vivienda Unifam. 7   J. Pérez       ◌ Borrador   12.040,00€  3 días    │
│      │  ▸ Rehab. Edif. Centro  Ayto. X        ◍ Cerrado   310.880,90€  10 ene    │
│      │  ▸ Urbaniz. Las Lomas   Promo Sur      ⊘ Archivado  —           2025      │
│      │                                                                           │
│      │  Mostrando 5 de 5 · vista tarjeta/lista conmutable [⊞|≣]                  │
└──────┴─────────────────────────────────────────────────────────────────────────┘
 Estados: ◌ borrador · ● activo · ◍ cerrado · ⊘ archivado  (pills con color)
```

Cada fila abre el proyecto. Acciones por fila (al pasar el ratón): abrir, duplicar, versionar, archivar, exportar. Vista alternativa en tarjetas (kanban por estado al estilo ClickUp).

### 4.2 Vista de preciosario (árbol de capítulos + partidas)

```
┌──────┬───────────────────────┬────────────────────────────────────────┬─────────┐
│ ⌂    │ Org › Preciosario DCF 2026 › 03 Cimentaciones        🔍 Ctrl+K  ◑ ☁●     │
│ 📁   ├───────────────────────┼────────────────────────────────────────┼─────────┤
│ 💰   │ ⌕ filtrar capítulos…  │ 03 CIMENTACIONES        [ + Partida ]   │ INSPECTOR│
│ 📚▸  │ ──────────────────    │ ──────────────────────────────────────  │ ───────  │
│ 🔍   │ ▾ 01 Movimiento tier. │ CÓDIGO   RESUMEN              UD  PRECIO │ Partida  │
│ ⚙    │   ▸ 01.01 Desbroce    │ ──────────────────────────────────────  │ E03HAH   │
│      │ ▾ 03 Cimentaciones ◀  │ E03HAH  H. armado zapata     m³ 142,30€ │          │
│      │   ▸ 03.01 Zapatas (24)│ E03HAL  H. limpieza          m³  78,10€ │ [Resumen]│
│      │   ▸ 03.02 Losas   (11)│ E03ALL  Acero B500S          kg   1,42€ │ [Análisis]│
│      │   ▸ 03.03 Muros   (08)│ E03HEM  Encofrado muro       m²  31,90€ │ [Medic.] │
│      │ ▸ 04 Estructura       │ E03ZAP  Zapata aislada       ud 320,00€ │ [Adjunt.]│
│      │ ▸ 05 Albañilería      │ … (virtual scroll, 24 de 512k) …        │ [Histor.]│
│      │                       │                                          │          │
│      │ 1.248 cap · 512.340 p │ Subtotal cap. 03: 18.420,90 €            │          │
└──────┴───────────────────────┴────────────────────────────────────────┴─────────┘
```

El árbol (`Capitulo` con `padre_id`) soporta lazy load: los hijos se cargan al expandir. La tabla central lista `Partida` del capítulo activo con virtual scrolling para soportar 500.000+ registros. El contador `(24)` indica partidas por capítulo.

### 4.3 Análisis de precios / descompuesto de una partida

```
┌──────┬──────────────────────────────────────────────┬──────────────────────────┐
│ 📁   │ … › 03 Cimentaciones › E03HAH  H. armado zapata m³        🔍 Ctrl+K  ◑ ☁●│
│ 💰   ├──────────────────────────────────────────────┼──────────────────────────┤
│ 📚   │  TABLA PARTIDAS (contexto)                    │ INSPECTOR · Análisis      │
│ 🔍   │  …                                            │ ────────────────────────  │
│ ⚙    │  E03HAH  H. armado zapata    m³  142,30€  ◀  │ E03HAH · m³ · 🔓 editable │
│      │                                               │                          │
│      │                                               │ [Resumen][Análisis◀][Med]│
│      │                                               │ ────────────────────────  │
│      │                                               │ CÓD   DESCRIP.  REND CANT IMP│
│      │                                               │ ──────────────────────────  │
│      │                                               │ 👷 MO001 Oficial 1ª        │
│      │                                               │        h  0,40  18,5  7,40€ │
│      │                                               │ 🧱 MT012 Hormigón HA-25    │
│      │                                               │        m³ 1,05  92,0  96,60€│
│      │                                               │ 🚜 MQ004 Bomba hormig.     │
│      │                                               │        h  0,10  45,0  4,50€ │
│      │                                               │ ──────────────────────────  │
│      │                                               │ Mano de obra ......  7,40€ │
│      │                                               │ Materiales ........ 96,60€ │
│      │                                               │ Maquinaria ........  4,50€ │
│      │                                               │ Costes indir. (6%)  6,51€ │
│      │                                               │ ════════════════════════  │
│      │                                               │ PRECIO PARTIDA ... 142,30€ │
│      │                                               │ [ + Recurso ]  [Guardar]   │
└──────┴──────────────────────────────────────────────┴──────────────────────────┘
```

Cada línea es un `Descompuesto` (recurso + rendimiento + cantidad → importe). Colores por tipo de `Recurso`. Pie con el agregado `AnalisisPrecios`: mano de obra + materiales + maquinaria + % `CostesIndirectos` = precio. El candado 🔓/🔒 refleja `Precio.bloqueado` (no se actualiza al reimportar DCF).

### 4.4 Editor de presupuesto con mediciones

```
┌──────┬─────────────────────┬─────────────────────────────────────────┬──────────┐
│ 💰   │ Reforma C/ Mayor › Presupuesto v3 (activo)            🔍 Ctrl+K  ◑ ☁●     │
│ 📚   ├─────────────────────┼─────────────────────────────────────────┼──────────┤
│ 🔍   │ ⌕ capítulos…        │ Pres. v3  [Vista: Medición ▾] [+Partida]│ INSPECTOR│
│ ⚙    │ ─────────────────   │ ─────────────────────────────────────── │ ──────── │
│      │ ▾ 03 Cimentaciones  │ CÓDIGO RESUMEN        UD  MED.  PRECIO IMPORTE│ Medición│
│      │   • Zapatas         │ ─────────────────────────────────────── │ E03HAH  │
│      │   • Losas       ◀   │ E03HAH H.arm. zapata  m³  48,20 142,30 6.858,86€│       │
│      │ ▾ 04 Estructura     │ E03ALL Acero B500S    kg 980,0   1,42 1.391,60€│ Total │
│      │   • Pilares         │ E03HEM Encofrado muro m² 120,5  31,90 3.843,95€│ 48,20m³│
│      │   • Forjados        │ ─────────────────────────────────────── │ ↳ ver   │
│      │ ▸ 05 Albañilería    │ Subtotal cap. 03 ............ 12.094,41€ │ líneas  │
│      │                     │ ─────────────────────────────────────── │ [Editar]│
│      │ Costes indir.: 6%   │ Subtotal cap. 04 ............ 28.500,10€ │          │
│      │ IVA: 21%            │ ═══════════════════════════════════════ │          │
│      │                     │ BASE 40.594,51 + CI 6% + IVA 21%        │          │
│      │ [ Versionar ⎘ ]     │ TOTAL PRESUPUESTO ......... 52.085,90 € │          │
└──────┴─────────────────────┴─────────────────────────────────────────┴──────────┘
```

Modo presupuesto: cada fila es una `PartidaPresupuesto` con su `Medicion.total` × `precio` = importe. El precio puede divergir del preciosario. El pie aplica `CostesIndirectos` + `Iva`. Botón "Versionar" crea nueva `Version` del `Presupuesto`.

### 4.5 Editor de líneas de medición

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│  Medición de partida E03HAH · H. armado zapata · m³                    [ ✕ Esc ]  │
├──────────────────────────────────────────────────────────────────────────────────┤
│  COMENTARIO            UDS  LARGO  ANCHO  ALTO   FÓRMULA          PARCIAL          │
│ ────────────────────────────────────────────────────────────────────────────────  │
│  Zapatas tipo A         12   1,20   1,20   0,60   —                10,368          │
│  Zapatas tipo B          8   1,50   1,50   0,70   —                12,600          │
│  Riostras perimetro      1   —      —      —      18*0,40*0,50      3,600 (fx)     │
│  Refuerzo esquinas       4   0,80   0,80   0,60   —                 1,536          │
│ ────────────────────────────────────────────────────────────────────────────────  │
│  [ + Añadir línea ]                                  TOTAL MEDICIÓN: 28,104 m³     │
│                                                                                    │
│  ℹ parcial = uds × largo × ancho × alto  ·  o resultado de fórmula (fx)           │
│  Tab/Enter para avanzar de celda · ↑↓ entre filas · = inicia fórmula              │
│                                              [ Cancelar ]   [ Guardar medición ]   │
└──────────────────────────────────────────────────────────────────────────────────┘
```

Cada fila es una `LineaMedicion` (`uds`, `largo`, `ancho`, `alto`, `formula`, `parcial`). El parcial se calcula como `uds×largo×ancho×alto` o, si hay `formula`, como su evaluación (marcado `fx`). La suma de parciales es el `Medicion.total`. Edición tipo hoja de cálculo con navegación por teclado.

### 4.6 Buscador inteligente / búsqueda IA

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│  🔍  hormigón armado para zapatas en exterior con bomba                  Ctrl+K   │
├──────────────────────────────────────────────────────────────────────────────────┤
│  [ Todo ]  [ Partidas ]  [ Capítulos ]  [ Recursos ]  [ ✦ IA semántica ◀ ]       │
│ ────────────────────────────────────────────────────────────────────────────────  │
│  ✦ RESULTADOS SEMÁNTICOS (pgvector · relevancia)                                  │
│ ────────────────────────────────────────────────────────────────────────────────  │
│  ●97%  E03HAH  H. armado en zapatas HA-25, vertido con bomba    m³   142,30€   →  │
│  ●91%  E03HAL  H. limpieza bajo zapata, exterior               m³    78,10€   →  │
│  ●86%  E03ZAP  Zapata aislada armada, encofrado exterior        ud   320,00€   →  │
│  ●74%  E04HPL  Pilar de hormigón armado HA-30                   m³   168,40€   →  │
│ ────────────────────────────────────────────────────────────────────────────────  │
│  COINCIDENCIAS EXACTAS (FTS5 / tsvector)                                          │
│  E03HAH · "hormigón armado" en resumen y texto largo                          →  │
│ ────────────────────────────────────────────────────────────────────────────────  │
│  ✦ Sugerencia IA: ¿Añadir las 3 primeras partidas al capítulo 03 actual? [Sí]    │
│  ↑↓ navegar · Enter abrir · ⏎+Shift insertar en presupuesto · Esc cerrar         │
└──────────────────────────────────────────────────────────────────────────────────┘
```

Combina búsqueda léxica (FTS5 local / tsvector servidor) y **semántica por embeddings** (pgvector + LLM Claude). Muestra porcentaje de relevancia, agrupa por tipo y ofrece acciones de inserción directa en el presupuesto activo.

### 4.7 Generación / preview de documento PDF

```
┌──────┬──────────────────────────────────┬───────────────────────────────────────┐
│ 💰   │ Exportar · Reforma C/Mayor v3                              🔍 Ctrl+K  ◑ ☁●│
├──────┼──────────────────────────────────┼───────────────────────────────────────┤
│ CONFIG.                                  │   PREVIEW (QuestPDF)                   │
│ ───────────────────────────────         │ ─────────────────────────────────────  │
│ Formato:  (●)PDF ( )Excel ( )CSV ( )BC3  │ ┌───────────────────────────────────┐ │
│ Plantilla:[ Presupuesto detallado ▾ ]    │ │  PRESUPUESTO                      │ │
│                                          │ │  Reforma Calle Mayor             │ │
│ Incluir:                                 │ │  Cliente: Const. Ríos · 26/06/26 │ │
│  [✓] Mediciones detalladas               │ │ ───────────────────────────────  │ │
│  [✓] Análisis de precios (descompuesto)  │ │ 03 CIMENTACIONES                 │ │
│  [✓] Resumen por capítulos               │ │  E03HAH H.arm. zapata            │ │
│  [ ] Imágenes/adjuntos                    │ │   48,20 m³ × 142,30 = 6.858,86€  │ │
│  [✓] Costes indirectos e IVA             │ │   …                              │ │
│                                          │ │ ───────────────────────────────  │ │
│ Logo:    [ ⤓ subir ]                      │ │ BASE  40.594,51 €                │ │
│ Idioma:  [ Español ▾ ]                    │ │ CI 6% · IVA 21%                  │ │
│                                          │ │ TOTAL 52.085,90 €     pág. 1/14  │ │
│ [ Generar y descargar ⤓ ]                │ └───────────────────────────────────┘ │
└──────┴──────────────────────────────────┴───────────────────────────────────────┘
```

Panel izquierdo de configuración (formato, `Plantilla`, secciones a incluir) y preview en vivo a la derecha. Exportadores: QuestPDF (PDF), ClosedXML (Excel), CSV/JSON/XML y futuro BC3 (FIEBDC-3).

---

## 5. Dark mode vs Light mode

- Conmutador en la topbar (◑) y atajo. Persistencia por usuario; opción "seguir sistema".
- Mismas estructuras y jerarquías; cambian tokens de color (sección 2.1). El dark mode usa fondos `#0F1115`/`#16181D` y reduce el brillo de los acentos para evitar fatiga.
- **Reglas**: contraste mínimo AA en ambos modos; los colores semánticos de recurso (mano de obra/material/maquinaria) se desaturan ligeramente en dark; las sombras se sustituyen por bordes más marcados (`--border`) ya que las sombras pierden eficacia sobre fondos oscuros.
- Importes negativos o de alerta nunca dependen solo del color: añaden icono/signo.

---

## 6. Responsive

| Breakpoint | Ancho | Layout |
|-----------|-------|--------|
| **Desktop** | ≥ 1280 px | Las 4 zonas visibles: Sidebar + Explorer + Panel central + Inspector. Experiencia completa. |
| **Laptop** | 1024–1279 px | Inspector colapsable a panel flotante (overlay) al seleccionar partida. Explorer reducido. |
| **Tablet** | 768–1023 px | Dos paneles a la vez: Explorer ⇄ Panel central; el Inspector pasa a hoja inferior (bottom sheet). Sidebar colapsada a iconos. |
| **Móvil** | < 768 px | Navegación por pila (un panel a la vez): Lista → Árbol → Tabla → Detalle. Sidebar como menú hamburguesa. Edición de mediciones en vista de tarjetas apiladas en lugar de tabla ancha. Acciones principales en barra inferior. |

Cambios clave en móvil:
- La tabla de partidas se transforma en lista de tarjetas (código + resumen + importe).
- El editor de líneas de medición muestra una línea por tarjeta con campos apilados.
- Búsqueda y command palette ocupan pantalla completa.
- Totales fijados en una barra inferior persistente (sticky).

---

## 7. Accesibilidad y atajos de teclado

### 7.1 Accesibilidad

- **Contraste**: WCAG 2.1 AA mínimo (texto normal 4.5:1, grande 3:1) en light y dark.
- **Foco visible**: anillo de 2 px `--primary` en todos los elementos interactivos; nunca se elimina el outline.
- **Navegación por teclado completa**: todo accionable sin ratón; orden de tabulación lógico; foco atrapado en modales y devuelto al cerrar.
- **Semántica/ARIA**: roles `tree`/`treeitem` para el árbol de capítulos, `grid` para tablas editables, `tablist`/`tab` para inspector, `dialog` para modales, `aria-live` para toasts y estado de sync.
- **No solo color**: estados (sync, validación, tipo de recurso) acompañados de icono o texto.
- **Tamaño de toque**: objetivos ≥ 44×44 px en táctil.
- **Texto escalable**: respeta el zoom del SO hasta 200% sin pérdida de contenido; soporte a lectores de pantalla en etiquetas e importes.

### 7.2 Atajos de teclado

| Atajo | Acción |
|-------|--------|
| `Ctrl/Cmd + K` | Búsqueda global / command palette |
| `Ctrl/Cmd + P` | Saltar a proyecto/presupuesto |
| `Ctrl/Cmd + F` | Buscar dentro del árbol/tabla actual |
| `Ctrl/Cmd + S` | Guardar (sync inmediato del cambio) |
| `Ctrl/Cmd + N` | Nuevo (proyecto / partida según contexto) |
| `Ctrl/Cmd + Z` / `Ctrl/Cmd + Shift + Z` | Deshacer / rehacer |
| `[` / `]` | Colapsar / expandir sidebar |
| `\` | Mostrar / ocultar inspector |
| `↑ ↓` | Navegar filas (tabla) o nodos (árbol) |
| `→ / ←` | Expandir / plegar nodo del árbol |
| `Enter` | Entrar en celda / abrir partida |
| `Tab / Shift+Tab` | Siguiente / anterior celda en edición |
| `=` | Iniciar fórmula en línea de medición |
| `Esc` | Cerrar modal / overlay / cancelar edición |
| `Ctrl/Cmd + Enter` | Confirmar y crear siguiente (líneas de medición) |
| `Ctrl/Cmd + D` | Duplicar fila / partida seleccionada |
| `Del` | Eliminar fila seleccionada (con confirmación) |
| `Ctrl/Cmd + Shift + V` | Versionar presupuesto |
| `Ctrl/Cmd + E` | Exportar (abrir panel de documento) |
| `?` | Mostrar ayuda de atajos |
```
