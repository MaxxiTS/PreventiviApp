import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../shared/utils/formato.dart';
import '../../../shared/widgets/async_value_widget.dart';
import '../../../shared/widgets/page_header.dart';
import '../data/presupuestos_repository.dart';
import '../domain/comparacion.dart';

/// Argumentos (par A/B) para la comparación de presupuestos.
class ComparacionArgs {
  const ComparacionArgs({required this.aId, required this.bId});

  final String aId;
  final String bId;

  @override
  bool operator ==(Object other) =>
      other is ComparacionArgs && other.aId == aId && other.bId == bId;

  @override
  int get hashCode => Object.hash(aId, bId);
}

/// Comparación entre dos presupuestos A y B (family por par de ids).
final comparacionProvider =
    FutureProvider.family<ComparacionResultado, ComparacionArgs>((ref, args) {
  return ref.read(presupuestosRepositoryProvider).comparar(args.aId, args.bId);
});

/// Pantalla de comparación de dos presupuestos: tabla de diferencias por
/// línea (código, estado, importes A/B y delta) y delta total.
class ComparacionScreen extends ConsumerWidget {
  const ComparacionScreen({
    super.key,
    required this.aId,
    required this.bId,
  });

  final String aId;
  final String bId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final args = ComparacionArgs(aId: aId, bId: bId);
    final comparacion = ref.watch(comparacionProvider(args));

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(
          breadcrumbs: const ['Presupuestos', 'Comparación'],
          subtitle: 'Diferencias entre el presupuesto A y el B.',
          actions: [
            TextButton.icon(
              onPressed: () => context.go('/presupuestos/$aId'),
              icon: const Icon(Icons.arrow_back, size: 18),
              label: const Text('Volver'),
            ),
          ],
        ),
        const Divider(),
        Expanded(
          child: AsyncValueWidget<ComparacionResultado>(
            value: comparacion,
            onRetry: () => ref.invalidate(comparacionProvider(args)),
            data: (c) => Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                _DeltaTotal(deltaTotal: c.deltaTotal),
                const Divider(height: 1),
                Expanded(child: _TablaComparacion(lineas: c.lineas)),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

class _DeltaTotal extends StatelessWidget {
  const _DeltaTotal({required this.deltaTotal});

  final double deltaTotal;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final color = _colorDelta(theme, deltaTotal);
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Card(
        color: color.withValues(alpha: 0.08),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text('Delta total (B − A)',
                  style: theme.textTheme.titleMedium),
              Text(
                _conSigno(deltaTotal),
                style: theme.textTheme.titleLarge
                    ?.copyWith(fontWeight: FontWeight.w700, color: color),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _TablaComparacion extends StatelessWidget {
  const _TablaComparacion({required this.lineas});

  final List<LineaComparacion> lineas;

  @override
  Widget build(BuildContext context) {
    if (lineas.isEmpty) {
      return const Center(child: Text('No hay diferencias que mostrar.'));
    }
    return SingleChildScrollView(
      scrollDirection: Axis.vertical,
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: DataTable(
          columns: const [
            DataColumn(label: Text('Código')),
            DataColumn(label: Text('Estado')),
            DataColumn(label: Text('Importe A'), numeric: true),
            DataColumn(label: Text('Importe B'), numeric: true),
            DataColumn(label: Text('Delta'), numeric: true),
          ],
          rows: [
            for (final l in lineas)
              DataRow(cells: [
                DataCell(Text(l.codigo)),
                DataCell(_EstadoChip(estado: l.estado)),
                DataCell(Text(Formato.euros(l.importeA))),
                DataCell(Text(Formato.euros(l.importeB))),
                DataCell(Builder(
                  builder: (context) => Text(
                    _conSigno(l.deltaImporte),
                    style: TextStyle(
                      color: _colorDelta(Theme.of(context), l.deltaImporte),
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                )),
              ]),
          ],
        ),
      ),
    );
  }
}

class _EstadoChip extends StatelessWidget {
  const _EstadoChip({required this.estado});

  final String estado;

  @override
  Widget build(BuildContext context) {
    final color = _colorEstado(context, estado);
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        estado.isEmpty ? '—' : estado,
        style: TextStyle(color: color, fontWeight: FontWeight.w600),
      ),
    );
  }
}

/// Color según el estado de la línea de comparación.
Color _colorEstado(BuildContext context, String estado) {
  final theme = Theme.of(context);
  switch (estado.toLowerCase()) {
    case 'nueva':
    case 'añadida':
    case 'anadida':
      return Colors.green.shade700;
    case 'eliminada':
    case 'borrada':
      return theme.colorScheme.error;
    case 'modificada':
    case 'cambiada':
      return Colors.orange.shade800;
    case 'igual':
    case 'sincambio':
    case 'sin cambio':
      return theme.colorScheme.onSurface.withValues(alpha: 0.5);
    default:
      return theme.colorScheme.onSurface.withValues(alpha: 0.7);
  }
}

/// Color del delta: verde si baja, rojo si sube, neutro si cero.
Color _colorDelta(ThemeData theme, double delta) {
  if (delta > 0) return theme.colorScheme.error;
  if (delta < 0) return Colors.green.shade700;
  return theme.colorScheme.onSurface.withValues(alpha: 0.6);
}

String _conSigno(double valor) {
  final s = Formato.euros(valor.abs());
  if (valor > 0) return '+ $s';
  if (valor < 0) return '- $s';
  return s;
}
