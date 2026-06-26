import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/formato.dart';
import '../../../shared/widgets/async_value_widget.dart';
import '../../../shared/widgets/page_header.dart';
import '../data/preciosarios_repository.dart';
import '../domain/preciosario.dart';

/// Pantalla del catálogo de precios.
///
/// Navegación tipo árbol/columnas:
///   Preciosario → Capítulos (drill-down por padreId) → Partidas → Análisis.
class PreciosariosScreen extends ConsumerStatefulWidget {
  const PreciosariosScreen({super.key});

  @override
  ConsumerState<PreciosariosScreen> createState() =>
      _PreciosariosScreenState();
}

class _PreciosariosScreenState extends ConsumerState<PreciosariosScreen> {
  String? _preciosarioId;

  /// Pila de capítulos navegados (breadcrumb de capítulos).
  final List<CapituloCatalogo> _ruta = [];

  String? _capituloSeleccionadoId;
  String? _partidaSeleccionadaId;

  @override
  Widget build(BuildContext context) {
    final preciosarios = ref.watch(preciosariosProvider);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(
          breadcrumbs: ['Preciosarios', ..._ruta.map((c) => c.titulo)],
          subtitle: 'Catálogo de precios: capítulos, partidas y análisis.',
        ),
        const Divider(),
        Expanded(
          child: AsyncValueWidget<List<Preciosario>>(
            value: preciosarios,
            onRetry: () => ref.invalidate(preciosariosProvider),
            data: (lista) {
              if (lista.isEmpty) {
                return const Center(
                  child: Text('No hay preciosarios. Importa un DCF en el backend.'),
                );
              }
              // Selección inicial del primer preciosario.
              _preciosarioId ??= lista.first.id;
              return Row(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  SizedBox(
                    width: 240,
                    child: _ListaPreciosarios(
                      preciosarios: lista,
                      seleccionadoId: _preciosarioId,
                      onSelect: (id) => setState(() {
                        _preciosarioId = id;
                        _ruta.clear();
                        _capituloSeleccionadoId = null;
                        _partidaSeleccionadaId = null;
                      }),
                    ),
                  ),
                  const VerticalDivider(width: 1),
                  Expanded(child: _PanelCapitulos(
                    preciosarioId: _preciosarioId!,
                    padreId:
                        _ruta.isEmpty ? null : _ruta.last.id,
                    ruta: _ruta,
                    onEntrarCapitulo: (cap) => setState(() {
                      _ruta.add(cap);
                      _capituloSeleccionadoId = cap.id;
                      _partidaSeleccionadaId = null;
                    }),
                    onSeleccionarCapitulo: (cap) => setState(() {
                      _capituloSeleccionadoId = cap.id;
                      _partidaSeleccionadaId = null;
                    }),
                    onSubir: () => setState(() {
                      if (_ruta.isNotEmpty) _ruta.removeLast();
                      _capituloSeleccionadoId =
                          _ruta.isEmpty ? null : _ruta.last.id;
                      _partidaSeleccionadaId = null;
                    }),
                  )),
                  const VerticalDivider(width: 1),
                  Expanded(
                    child: _capituloSeleccionadoId == null
                        ? const _PanelVacio(
                            mensaje: 'Selecciona un capítulo para ver sus partidas.')
                        : _PanelPartidas(
                            capituloId: _capituloSeleccionadoId!,
                            seleccionadaId: _partidaSeleccionadaId,
                            onSelect: (p) => setState(
                                () => _partidaSeleccionadaId = p.id),
                          ),
                  ),
                  const VerticalDivider(width: 1),
                  SizedBox(
                    width: 320,
                    child: _partidaSeleccionadaId == null
                        ? const _PanelVacio(
                            mensaje: 'Selecciona una partida para ver su análisis.')
                        : _PanelAnalisis(partidaId: _partidaSeleccionadaId!),
                  ),
                ],
              );
            },
          ),
        ),
      ],
    );
  }
}

class _ListaPreciosarios extends StatelessWidget {
  const _ListaPreciosarios({
    required this.preciosarios,
    required this.seleccionadoId,
    required this.onSelect,
  });

  final List<Preciosario> preciosarios;
  final String? seleccionadoId;
  final ValueChanged<String> onSelect;

  @override
  Widget build(BuildContext context) {
    return ListView(
      children: [
        for (final p in preciosarios)
          ListTile(
            dense: true,
            selected: p.id == seleccionadoId,
            title: Text(p.nombre, maxLines: 1, overflow: TextOverflow.ellipsis),
            subtitle: Text(p.version),
            onTap: () => onSelect(p.id),
          ),
      ],
    );
  }
}

class _PanelCapitulos extends ConsumerWidget {
  const _PanelCapitulos({
    required this.preciosarioId,
    required this.padreId,
    required this.ruta,
    required this.onEntrarCapitulo,
    required this.onSeleccionarCapitulo,
    required this.onSubir,
  });

  final String preciosarioId;
  final String? padreId;
  final List<CapituloCatalogo> ruta;
  final ValueChanged<CapituloCatalogo> onEntrarCapitulo;
  final ValueChanged<CapituloCatalogo> onSeleccionarCapitulo;
  final VoidCallback onSubir;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final args =
        CapitulosArgs(preciosarioId: preciosarioId, padreId: padreId);
    final capitulos = ref.watch(capitulosProvider(args));

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (ruta.isNotEmpty)
          ListTile(
            dense: true,
            leading: const Icon(Icons.arrow_upward, size: 18),
            title: const Text('Subir un nivel'),
            onTap: onSubir,
          ),
        Expanded(
          child: AsyncValueWidget<List<CapituloCatalogo>>(
            value: capitulos,
            onRetry: () => ref.invalidate(capitulosProvider(args)),
            data: (lista) {
              if (lista.isEmpty) {
                return const Center(child: Text('Sin capítulos.'));
              }
              return ListView(
                children: [
                  for (final c in lista)
                    ListTile(
                      dense: true,
                      leading: const Icon(Icons.folder_outlined, size: 18),
                      title: Text('${c.codigo}  ${c.titulo}',
                          maxLines: 2, overflow: TextOverflow.ellipsis),
                      trailing: IconButton(
                        tooltip: 'Entrar en subcapítulos',
                        icon: const Icon(Icons.chevron_right),
                        onPressed: () => onEntrarCapitulo(c),
                      ),
                      onTap: () => onSeleccionarCapitulo(c),
                    ),
                ],
              );
            },
          ),
        ),
      ],
    );
  }
}

class _PanelPartidas extends ConsumerWidget {
  const _PanelPartidas({
    required this.capituloId,
    required this.seleccionadaId,
    required this.onSelect,
  });

  final String capituloId;
  final String? seleccionadaId;
  final ValueChanged<PartidaCatalogo> onSelect;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final partidas = ref.watch(partidasProvider(capituloId));
    return AsyncValueWidget<List<PartidaCatalogo>>(
      value: partidas,
      onRetry: () => ref.invalidate(partidasProvider(capituloId)),
      data: (lista) {
        if (lista.isEmpty) {
          return const Center(child: Text('Este capítulo no tiene partidas.'));
        }
        return ListView(
          children: [
            for (final p in lista)
              ListTile(
                dense: true,
                selected: p.id == seleccionadaId,
                title: Text('${p.codigo}  ${p.resumen}',
                    maxLines: 2, overflow: TextOverflow.ellipsis),
                trailing: Text(Formato.euros(p.precio)),
                onTap: () => onSelect(p),
              ),
          ],
        );
      },
    );
  }
}

class _PanelAnalisis extends ConsumerWidget {
  const _PanelAnalisis({required this.partidaId});

  final String partidaId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final analisis = ref.watch(analisisProvider(partidaId));
    final theme = Theme.of(context);

    return AsyncValueWidget<AnalisisPrecios>(
      value: analisis,
      onRetry: () => ref.invalidate(analisisProvider(partidaId)),
      data: (a) => ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text('Análisis de precios', style: theme.textTheme.titleMedium),
          const SizedBox(height: 8),
          Text('${a.codigo}  ${a.resumen}', style: theme.textTheme.bodyMedium),
          const SizedBox(height: 12),
          for (final l in a.lineas)
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 4),
              child: Row(
                children: [
                  Expanded(child: Text(l.tipo)),
                  Text(
                    '${Formato.numero(l.rendimiento)} × '
                    '${Formato.euros(l.precioUnitario)}',
                    style: theme.textTheme.bodySmall,
                  ),
                  const SizedBox(width: 8),
                  Text(Formato.euros(l.importe),
                      style: theme.textTheme.bodyMedium),
                ],
              ),
            ),
          const Divider(),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text('Coste directo'),
              Text(Formato.euros(a.costeDirecto),
                  style: const TextStyle(fontWeight: FontWeight.w600)),
            ],
          ),
          const SizedBox(height: 4),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text('Precio', style: theme.textTheme.titleSmall),
              Text(Formato.euros(a.precio),
                  style: theme.textTheme.titleSmall
                      ?.copyWith(color: theme.colorScheme.primary)),
            ],
          ),
        ],
      ),
    );
  }
}

class _PanelVacio extends StatelessWidget {
  const _PanelVacio({required this.mensaje});

  final String mensaje;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Text(
          mensaje,
          textAlign: TextAlign.center,
          style: Theme.of(context)
              .textTheme
              .bodySmall
              ?.copyWith(color: Theme.of(context).colorScheme.onSurface.withValues(alpha: 0.5)),
        ),
      ),
    );
  }
}
