import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../shared/utils/formato.dart';
import '../../../shared/widgets/async_value_widget.dart';
import '../../../shared/widgets/page_header.dart';
import '../data/presupuestos_repository.dart';
import '../domain/presupuesto.dart';

/// Pantalla de detalle de un presupuesto: resumen económico (PEM, baja, base,
/// IVA, TOTAL) y árbol de capítulos/partidas.
class PresupuestoScreen extends ConsumerWidget {
  const PresupuestoScreen({super.key, required this.presupuestoId});

  final String presupuestoId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detalle = ref.watch(presupuestoDetalleProvider(presupuestoId));

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(
          breadcrumbs: [
            'Presupuestos',
            detalle.maybeWhen(
              data: (p) => '${p.nombre} (v${p.version})',
              orElse: () => 'Detalle',
            ),
          ],
          actions: [
            IconButton(
              tooltip: 'Recargar',
              onPressed: () =>
                  ref.invalidate(presupuestoDetalleProvider(presupuestoId)),
              icon: const Icon(Icons.refresh),
            ),
          ],
        ),
        const Divider(),
        Expanded(
          child: AsyncValueWidget<PresupuestoDetalle>(
            value: detalle,
            onRetry: () =>
                ref.invalidate(presupuestoDetalleProvider(presupuestoId)),
            data: (p) => ListView(
              padding: const EdgeInsets.all(16),
              children: [
                _ResumenEconomico(presupuesto: p),
                const SizedBox(height: 20),
                Text('Capítulos',
                    style: Theme.of(context).textTheme.titleMedium),
                const SizedBox(height: 8),
                if (p.capitulos.isEmpty)
                  const Padding(
                    padding: EdgeInsets.all(24),
                    child: Center(child: Text('El presupuesto no tiene capítulos.')),
                  )
                else
                  for (final cap in p.capitulos)
                    _CapituloTile(capitulo: cap, nivel: 0),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

/// Tarjeta con el cierre económico del presupuesto.
class _ResumenEconomico extends StatelessWidget {
  const _ResumenEconomico({required this.presupuesto});

  final PresupuestoDetalle presupuesto;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final baja = presupuesto.pem - presupuesto.pemAjustado;

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text('Resumen económico', style: theme.textTheme.titleMedium),
            const SizedBox(height: 12),
            _fila(context, 'PEM (Presupuesto Ejecución Material)',
                Formato.euros(presupuesto.pem)),
            _fila(context, 'Baja / descuentos', '- ${Formato.euros(baja)}'),
            _fila(context, 'PEM ajustado',
                Formato.euros(presupuesto.pemAjustado)),
            const Divider(height: 24),
            _fila(context, 'Base imponible',
                Formato.euros(presupuesto.baseImponible)),
            _fila(context, 'Cuota IVA', Formato.euros(presupuesto.cuotaIva)),
            const Divider(height: 24),
            _fila(
              context,
              'TOTAL',
              Formato.euros(presupuesto.total),
              destacado: true,
            ),
          ],
        ),
      ),
    );
  }

  Widget _fila(BuildContext context, String label, String valor,
      {bool destacado = false}) {
    final theme = Theme.of(context);
    final estilo = destacado
        ? theme.textTheme.titleLarge
            ?.copyWith(fontWeight: FontWeight.w700, color: theme.colorScheme.primary)
        : theme.textTheme.bodyMedium;
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: estilo),
          Text(valor, style: estilo),
        ],
      ),
    );
  }
}

/// Nodo del árbol de capítulos (expandible), con sus partidas y subcapítulos.
class _CapituloTile extends StatelessWidget {
  const _CapituloTile({required this.capitulo, required this.nivel});

  final CapituloDetalle capitulo;
  final int nivel;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: EdgeInsets.only(left: nivel * 12.0),
      child: ExpansionTile(
        initiallyExpanded: nivel == 0,
        tilePadding: const EdgeInsets.symmetric(horizontal: 8),
        childrenPadding: const EdgeInsets.only(left: 8, bottom: 4),
        title: Row(
          children: [
            Expanded(
              child: Text(
                '${capitulo.codigo}  ${capitulo.titulo}',
                style: theme.textTheme.titleSmall
                    ?.copyWith(fontWeight: FontWeight.w600),
              ),
            ),
            Text(
              Formato.euros(capitulo.subtotal),
              style: theme.textTheme.bodyMedium
                  ?.copyWith(fontWeight: FontWeight.w600),
            ),
          ],
        ),
        children: [
          for (final partida in capitulo.partidas)
            _PartidaRow(partida: partida),
          for (final sub in capitulo.subcapitulos)
            _CapituloTile(capitulo: sub, nivel: nivel + 1),
        ],
      ),
    );
  }
}

class _PartidaRow extends StatelessWidget {
  const _PartidaRow({required this.partida});

  final PartidaDetalle partida;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final muted = theme.colorScheme.onSurface.withValues(alpha: 0.6);
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('${partida.codigo}  ${partida.resumen}',
                    style: theme.textTheme.bodyMedium),
                Text(
                  'Medición: ${Formato.numero(partida.medicionTotal)} × '
                  '${Formato.euros(partida.precio)}',
                  style: theme.textTheme.bodySmall?.copyWith(color: muted),
                ),
              ],
            ),
          ),
          const SizedBox(width: 12),
          Text(Formato.euros(partida.importe),
              style: theme.textTheme.bodyMedium),
        ],
      ),
    );
  }
}
