import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../shared/widgets/page_header.dart';

/// Pantalla de inicio / dashboard ligero con accesos rápidos.
class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const PageHeader(
          breadcrumbs: ['Inicio'],
          subtitle: 'Presupuestos y mediciones de obra — offline-first.',
        ),
        const Divider(),
        Expanded(
          child: Center(
            child: Wrap(
              spacing: 16,
              runSpacing: 16,
              children: [
                _AccesoRapido(
                  icono: Icons.folder_outlined,
                  titulo: 'Proyectos',
                  descripcion: 'Gestiona proyectos y presupuestos',
                  onTap: () => context.go('/proyectos'),
                ),
                _AccesoRapido(
                  icono: Icons.menu_book_outlined,
                  titulo: 'Preciosarios',
                  descripcion: 'Explora el catálogo de precios',
                  onTap: () => context.go('/preciosarios'),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

class _AccesoRapido extends StatelessWidget {
  const _AccesoRapido({
    required this.icono,
    required this.titulo,
    required this.descripcion,
    required this.onTap,
  });

  final IconData icono;
  final String titulo;
  final String descripcion;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return SizedBox(
      width: 240,
      child: Card(
        child: InkWell(
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Icon(icono, size: 28, color: theme.colorScheme.primary),
                const SizedBox(height: 12),
                Text(titulo, style: theme.textTheme.titleMedium),
                const SizedBox(height: 4),
                Text(descripcion, style: theme.textTheme.bodySmall),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
