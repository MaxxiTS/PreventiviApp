import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../config/app_config.dart';

/// Shell persistente de la app: sidebar (NavigationRail) a la izquierda y el
/// contenido de la ruta activa a la derecha (estilo Notion/Linear).
class AppShell extends StatelessWidget {
  const AppShell({super.key, required this.child});

  /// Contenido de la ruta hija renderizado por go_router.
  final Widget child;

  static const _destinos = [
    _Destino(ruta: '/proyectos', icono: Icons.folder_outlined,
        iconoSel: Icons.folder, etiqueta: 'Proyectos'),
    _Destino(ruta: '/preciosarios', icono: Icons.menu_book_outlined,
        iconoSel: Icons.menu_book, etiqueta: 'Preciosarios'),
  ];

  int _indiceActual(BuildContext context) {
    final loc = GoRouterState.of(context).uri.path;
    for (var i = 0; i < _destinos.length; i++) {
      if (loc.startsWith(_destinos[i].ruta)) return i;
    }
    return -1; // p. ej. ruta '/' (inicio): ningún destino seleccionado.
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final indice = _indiceActual(context);

    return Scaffold(
      body: Row(
        children: [
          NavigationRail(
            selectedIndex: indice >= 0 ? indice : null,
            onDestinationSelected: (i) => context.go(_destinos[i].ruta),
            labelType: NavigationRailLabelType.all,
            leading: Padding(
              padding: const EdgeInsets.symmetric(vertical: 16),
              child: Column(
                children: [
                  Icon(Icons.architecture,
                      color: theme.colorScheme.primary, size: 28),
                  const SizedBox(height: 4),
                  GestureDetector(
                    onTap: () => context.go('/'),
                    child: Text(
                      AppConfig.appName.split(' ').first,
                      style: theme.textTheme.labelSmall,
                    ),
                  ),
                ],
              ),
            ),
            destinations: [
              for (final d in _destinos)
                NavigationRailDestination(
                  icon: Icon(d.icono),
                  selectedIcon: Icon(d.iconoSel),
                  label: Text(d.etiqueta),
                ),
            ],
          ),
          const VerticalDivider(width: 1),
          Expanded(
            child: Container(
              color: theme.colorScheme.surface,
              child: child,
            ),
          ),
        ],
      ),
    );
  }
}

class _Destino {
  const _Destino({
    required this.ruta,
    required this.icono,
    required this.iconoSel,
    required this.etiqueta,
  });

  final String ruta;
  final IconData icono;
  final IconData iconoSel;
  final String etiqueta;
}
