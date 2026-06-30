import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../shared/utils/errores.dart';
import '../../../shared/utils/formato.dart';
import '../../../shared/widgets/async_value_widget.dart';
import '../../../shared/widgets/page_header.dart';
import '../data/presupuestos_repository.dart';
import '../domain/presupuesto.dart';
import 'alta_partida_dialog.dart';
import 'pdf_descarga.dart';
import 'presupuesto_dialogs.dart';

/// Pantalla de detalle de un presupuesto: resumen económico (PEM, baja, base,
/// IVA, TOTAL) y árbol de capítulos/partidas, con acciones interactivas.
class PresupuestoScreen extends ConsumerWidget {
  const PresupuestoScreen({
    super.key,
    required this.presupuestoId,
    this.proyectoId,
  });

  final String presupuestoId;

  /// Proyecto al que pertenece (necesario para comparar con sus hermanos).
  final String? proyectoId;

  Future<void> _nuevaVersion(BuildContext context, WidgetRef ref) async {
    try {
      final nuevoId = await ref
          .read(presupuestosRepositoryProvider)
          .crearVersion(presupuestoId);
      if (proyectoId != null) {
        ref.invalidate(presupuestosPorProyectoProvider(proyectoId!));
      }
      if (!context.mounted) return;
      mostrarSnack(context, 'Nueva versión creada.');
      context.go('/presupuestos/$nuevoId'
          '${proyectoId != null ? '?proyectoId=$proyectoId' : ''}');
    } catch (e) {
      if (context.mounted) mostrarErrorSnack(context, e);
    }
  }

  Future<void> _comparar(BuildContext context, WidgetRef ref) async {
    if (proyectoId == null) {
      mostrarErrorSnack(
          context, 'No se conoce el proyecto para comparar presupuestos.');
      return;
    }
    final bId = await mostrarDialogoElegirComparacion(
      context,
      ref,
      proyectoId: proyectoId!,
      presupuestoActualId: presupuestoId,
    );
    if (bId == null || !context.mounted) return;
    context.go('/presupuestos/comparar?a=$presupuestoId&b=$bId');
  }

  Future<void> _descargarPdf(BuildContext context, WidgetRef ref) async {
    try {
      final bytes = await ref
          .read(presupuestosRepositoryProvider)
          .descargarPdf(presupuestoId);
      if (!context.mounted) return;
      await guardarPdf(context, bytes, 'presupuesto_$presupuestoId.pdf');
    } catch (e) {
      if (context.mounted) mostrarErrorSnack(context, e);
    }
  }

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
              tooltip: 'Añadir capítulo',
              onPressed: () => mostrarDialogoCapitulo(context, ref,
                  presupuestoId: presupuestoId),
              icon: const Icon(Icons.create_new_folder_outlined),
            ),
            IconButton(
              tooltip: 'Nueva versión',
              onPressed: () => _nuevaVersion(context, ref),
              icon: const Icon(Icons.history),
            ),
            IconButton(
              tooltip: 'Actualizar precios',
              onPressed: () => mostrarDialogoActualizarPrecios(context, ref,
                  presupuestoId: presupuestoId),
              icon: const Icon(Icons.sync),
            ),
            IconButton(
              tooltip: 'Comparar',
              onPressed: () => _comparar(context, ref),
              icon: const Icon(Icons.compare_arrows),
            ),
            IconButton(
              tooltip: 'Descargar PDF',
              onPressed: () => _descargarPdf(context, ref),
              icon: const Icon(Icons.picture_as_pdf_outlined),
            ),
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
                Row(
                  children: [
                    Text('Capítulos',
                        style: Theme.of(context).textTheme.titleMedium),
                    const Spacer(),
                    TextButton.icon(
                      onPressed: () => mostrarDialogoCapitulo(context, ref,
                          presupuestoId: presupuestoId),
                      icon: const Icon(Icons.add, size: 18),
                      label: const Text('Añadir capítulo'),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                if (p.capitulos.isEmpty)
                  const Padding(
                    padding: EdgeInsets.all(24),
                    child: Center(
                        child: Text('El presupuesto no tiene capítulos.')),
                  )
                else
                  for (final cap in p.capitulos)
                    _CapituloTile(
                      capitulo: cap,
                      nivel: 0,
                      presupuestoId: presupuestoId,
                    ),
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
        ? theme.textTheme.titleLarge?.copyWith(
            fontWeight: FontWeight.w700, color: theme.colorScheme.primary)
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
class _CapituloTile extends ConsumerWidget {
  const _CapituloTile({
    required this.capitulo,
    required this.nivel,
    required this.presupuestoId,
  });

  final CapituloDetalle capitulo;
  final int nivel;
  final String presupuestoId;

  Future<void> _duplicar(BuildContext context, WidgetRef ref) async {
    try {
      await ref
          .read(presupuestosRepositoryProvider)
          .duplicarCapitulo(capitulo.id);
      ref.invalidate(presupuestoDetalleProvider(presupuestoId));
      if (context.mounted) mostrarSnack(context, 'Capítulo duplicado.');
    } catch (e) {
      if (context.mounted) mostrarErrorSnack(context, e);
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
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
            PopupMenuButton<String>(
              tooltip: 'Acciones del capítulo',
              onSelected: (v) {
                switch (v) {
                  case 'partida':
                    mostrarDialogoAltaPartida(context, ref,
                        presupuestoId: presupuestoId,
                        capituloId: capitulo.id);
                  case 'subcapitulo':
                    mostrarDialogoCapitulo(context, ref,
                        presupuestoId: presupuestoId, padreId: capitulo.id);
                  case 'duplicar':
                    _duplicar(context, ref);
                }
              },
              itemBuilder: (_) => const [
                PopupMenuItem(
                  value: 'partida',
                  child: ListTile(
                    leading: Icon(Icons.playlist_add),
                    title: Text('Añadir partida'),
                  ),
                ),
                PopupMenuItem(
                  value: 'subcapitulo',
                  child: ListTile(
                    leading: Icon(Icons.create_new_folder_outlined),
                    title: Text('Añadir subcapítulo'),
                  ),
                ),
                PopupMenuItem(
                  value: 'duplicar',
                  child: ListTile(
                    leading: Icon(Icons.copy_all_outlined),
                    title: Text('Duplicar capítulo'),
                  ),
                ),
              ],
            ),
          ],
        ),
        children: [
          for (final partida in capitulo.partidas)
            _PartidaRow(partida: partida, presupuestoId: presupuestoId),
          for (final sub in capitulo.subcapitulos)
            _CapituloTile(
              capitulo: sub,
              nivel: nivel + 1,
              presupuestoId: presupuestoId,
            ),
        ],
      ),
    );
  }
}

class _PartidaRow extends ConsumerWidget {
  const _PartidaRow({required this.partida, required this.presupuestoId});

  final PartidaDetalle partida;
  final String presupuestoId;

  Future<void> _duplicar(BuildContext context, WidgetRef ref) async {
    try {
      await ref
          .read(presupuestosRepositoryProvider)
          .duplicarPartida(partida.id);
      ref.invalidate(presupuestoDetalleProvider(presupuestoId));
      if (context.mounted) mostrarSnack(context, 'Partida duplicada.');
    } catch (e) {
      if (context.mounted) mostrarErrorSnack(context, e);
    }
  }

  Future<void> _bloquear(
      BuildContext context, WidgetRef ref, bool bloqueado) async {
    try {
      await ref
          .read(presupuestosRepositoryProvider)
          .bloquearPrecio(partida.id, bloqueado);
      ref.invalidate(presupuestoDetalleProvider(presupuestoId));
      if (context.mounted) {
        mostrarSnack(context,
            bloqueado ? 'Precio bloqueado.' : 'Precio desbloqueado.');
      }
    } catch (e) {
      if (context.mounted) mostrarErrorSnack(context, e);
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
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
          PopupMenuButton<String>(
            tooltip: 'Acciones de la partida',
            onSelected: (v) {
              switch (v) {
                case 'precio':
                  mostrarDialogoEditarPrecio(context, ref,
                      presupuestoId: presupuestoId, partida: partida);
                case 'bloquear':
                  _bloquear(context, ref, true);
                case 'desbloquear':
                  _bloquear(context, ref, false);
                case 'duplicar':
                  _duplicar(context, ref);
              }
            },
            itemBuilder: (_) => const [
              PopupMenuItem(
                value: 'precio',
                child: ListTile(
                  leading: Icon(Icons.edit_outlined),
                  title: Text('Editar precio'),
                ),
              ),
              PopupMenuItem(
                value: 'bloquear',
                child: ListTile(
                  leading: Icon(Icons.lock_outline),
                  title: Text('Bloquear precio'),
                ),
              ),
              PopupMenuItem(
                value: 'desbloquear',
                child: ListTile(
                  leading: Icon(Icons.lock_open_outlined),
                  title: Text('Desbloquear precio'),
                ),
              ),
              PopupMenuItem(
                value: 'duplicar',
                child: ListTile(
                  leading: Icon(Icons.copy_outlined),
                  title: Text('Duplicar'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
