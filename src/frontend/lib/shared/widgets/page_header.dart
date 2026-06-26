import 'package:flutter/material.dart';

/// Cabecera de página con breadcrumb simple y acciones opcionales.
///
/// Mantiene una jerarquía visual coherente entre pantallas (estilo Linear):
/// migas de pan a la izquierda, acciones a la derecha.
class PageHeader extends StatelessWidget {
  const PageHeader({
    super.key,
    required this.breadcrumbs,
    this.subtitle,
    this.actions = const [],
  });

  /// Segmentos de la ruta, p. ej. `['Proyectos', 'Reforma Edificio Sol']`.
  final List<String> breadcrumbs;

  /// Texto secundario opcional bajo el breadcrumb.
  final String? subtitle;

  /// Acciones alineadas a la derecha (botones, etc.).
  final List<Widget> actions;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final muted = theme.colorScheme.onSurface.withValues(alpha: 0.5);

    return Padding(
      padding: const EdgeInsets.fromLTRB(24, 20, 24, 12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _Breadcrumb(segments: breadcrumbs, mutedColor: muted),
                if (subtitle != null) ...[
                  const SizedBox(height: 4),
                  Text(
                    subtitle!,
                    style: theme.textTheme.bodySmall?.copyWith(color: muted),
                  ),
                ],
              ],
            ),
          ),
          ...actions,
        ],
      ),
    );
  }
}

class _Breadcrumb extends StatelessWidget {
  const _Breadcrumb({required this.segments, required this.mutedColor});

  final List<String> segments;
  final Color mutedColor;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final children = <Widget>[];

    for (var i = 0; i < segments.length; i++) {
      final esUltimo = i == segments.length - 1;
      children.add(
        Text(
          segments[i],
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: esUltimo ? FontWeight.w700 : FontWeight.w500,
            color: esUltimo ? null : mutedColor,
          ),
        ),
      );
      if (!esUltimo) {
        children.add(Padding(
          padding: const EdgeInsets.symmetric(horizontal: 8),
          child: Icon(Icons.chevron_right, size: 18, color: mutedColor),
        ));
      }
    }

    return Wrap(crossAxisAlignment: WrapCrossAlignment.center, children: children);
  }
}
