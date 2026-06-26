# 0002 - Flutter + Riverpod para Desktop, Mobile y Web

## Estado

Aceptado

## Contexto

Preventivi App debe ejecutarse en **escritorio** (donde trabaja el presupuestista de forma intensiva, estilo Primus), en **móvil/tablet** (a pie de obra, para mediciones y adjuntar fotos/planos) y en **web** (acceso ligero y colaboración). Requisitos clave:

- Una sola base de código que cubra los tres targets para minimizar coste y divergencia de funcionalidad.
- UI fluida y de alta densidad de información, inspirada en Notion, Linear, Figma y ClickUp.
- **Rendimiento exigente**: preciosarios de 500.000+ partidas con lazy loading, virtual scrolling y caché.
- Capacidad **offline-first** con base de datos local embebida.
- Buen soporte para escritorio nativo (ventanas, ficheros, impresión/PDF).

## Decisión

Adoptamos **Flutter** como framework de UI multiplataforma (Desktop + Mobile + Web) con **Riverpod** para gestión de estado e inyección de dependencias. Estructuramos el código con patrón **feature-first** y **MVVM** (vistas declarativas + ViewModels/Notifiers de Riverpod + repositorios).

Razones:

- Un único runtime de renderizado (Skia/Impeller) garantiza UI **consistente y pixel-perfect** en todos los targets, idóneo para una interfaz de alta densidad.
- Riverpod ofrece estado compilado y seguro, providers componibles y testeo sencillo, encajando con MVVM.
- El widget tree con `ListView.builder`/`Sliver` virtualizado cubre el virtual scrolling necesario para grandes preciosarios.
- Acceso a SQLite local (offline-first) mediante paquetes maduros del ecosistema.

## Consecuencias

**Positivas**

- Una sola base de código y un solo equipo de frontend para tres plataformas.
- Rendimiento de scroll y render nativo, adecuado para tablas grandes.
- Hot reload acelera la iteración de la UI.
- Riverpod facilita estado predecible, caché y testabilidad.

**Negativas / costes**

- **Flutter Web** tiene mayor peso inicial y limitaciones de SEO/accesibilidad frente a una SPA HTML; aceptable porque la web es una herramienta de productividad autenticada, no un sitio público.
- El ecosistema de widgets de escritorio es más joven que el de móvil; algunos comportamientos nativos requieren plugins o código específico por plataforma.
- Dart es menos extendido que TypeScript en el mercado, lo que puede afectar a la contratación.

## Alternativas consideradas

- **React + Electron (desktop) + React Native/PWA (móvil)**: stack muy popular y con gran ecosistema, pero implica **dos bases de código** (web/desktop con DOM vs. móvil) o asumir el alto consumo de memoria de Electron en escritorio. La consistencia visual y el rendimiento de tablas masivas son más difíciles de garantizar de forma uniforme. Descartada por coste de mantenimiento y rendimiento en desktop.
- **.NET MAUI / Avalonia**: alinearía frontend y backend en .NET, pero su soporte web y la madurez/fluidez de UI son inferiores a Flutter para el nivel de pulido buscado. Descartada.
- **Tauri + framework web**: escritorio ligero, pero seguiría requiriendo solución móvil aparte y la web como base introduce las mismas limitaciones de rendimiento para tablas enormes. Descartada.
