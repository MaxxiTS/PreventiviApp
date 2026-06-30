import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/errores.dart';
import '../../preciosarios/data/preciosarios_repository.dart';
import '../../preciosarios/domain/preciosario.dart';
import '../data/presupuestos_repository.dart';
import '../domain/actualizacion_precios.dart';
import '../domain/presupuesto.dart';

/// Conjunto de diálogos de mutación del flujo de presupuestos.
///
/// Cada diálogo realiza la llamada al repositorio, invalida el detalle del
/// presupuesto afectado y devuelve `true` si la operación tuvo éxito. El
/// feedback (SnackBar) se gestiona en el llamador.

/// Diálogo para añadir un capítulo (o subcapítulo si [padreId] no es nulo).
Future<bool?> mostrarDialogoCapitulo(
  BuildContext context,
  WidgetRef ref, {
  required String presupuestoId,
  String? padreId,
}) {
  return showDialog<bool>(
    context: context,
    builder: (_) => _CapituloDialog(
      presupuestoId: presupuestoId,
      padreId: padreId,
    ),
  );
}

class _CapituloDialog extends ConsumerStatefulWidget {
  const _CapituloDialog({required this.presupuestoId, this.padreId});

  final String presupuestoId;
  final String? padreId;

  @override
  ConsumerState<_CapituloDialog> createState() => _CapituloDialogState();
}

class _CapituloDialogState extends ConsumerState<_CapituloDialog> {
  final _formKey = GlobalKey<FormState>();
  final _codigo = TextEditingController();
  final _titulo = TextEditingController();
  final _orden = TextEditingController();
  bool _guardando = false;

  @override
  void dispose() {
    _codigo.dispose();
    _titulo.dispose();
    _orden.dispose();
    super.dispose();
  }

  Future<void> _guardar() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _guardando = true);
    try {
      await ref.read(presupuestosRepositoryProvider).anadirCapitulo(
            presupuestoId: widget.presupuestoId,
            codigo: _codigo.text.trim(),
            titulo: _titulo.text.trim(),
            padreId: widget.padreId,
            orden: _orden.text.trim().isEmpty
                ? null
                : int.tryParse(_orden.text.trim()),
          );
      ref.invalidate(presupuestoDetalleProvider(widget.presupuestoId));
      if (mounted) Navigator.of(context).pop(true);
    } catch (e) {
      if (mounted) {
        setState(() => _guardando = false);
        mostrarErrorSnack(context, e);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(
          widget.padreId == null ? 'Añadir capítulo' : 'Añadir subcapítulo'),
      content: Form(
        key: _formKey,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextFormField(
              controller: _codigo,
              autofocus: true,
              decoration: const InputDecoration(labelText: 'Código'),
              validator: (v) =>
                  (v == null || v.trim().isEmpty) ? 'Indica un código' : null,
            ),
            const SizedBox(height: 8),
            TextFormField(
              controller: _titulo,
              decoration: const InputDecoration(labelText: 'Título'),
              validator: (v) =>
                  (v == null || v.trim().isEmpty) ? 'Indica un título' : null,
            ),
            const SizedBox(height: 8),
            TextFormField(
              controller: _orden,
              decoration:
                  const InputDecoration(labelText: 'Orden (opcional)'),
              keyboardType: TextInputType.number,
              inputFormatters: [FilteringTextInputFormatter.digitsOnly],
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: _guardando ? null : () => Navigator.of(context).pop(),
          child: const Text('Cancelar'),
        ),
        FilledButton(
          onPressed: _guardando ? null : _guardar,
          child: _guardando
              ? const SizedBox(
                  width: 18,
                  height: 18,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Text('Guardar'),
        ),
      ],
    );
  }
}

/// Diálogo para editar el precio de una partida.
Future<bool?> mostrarDialogoEditarPrecio(
  BuildContext context,
  WidgetRef ref, {
  required String presupuestoId,
  required PartidaDetalle partida,
}) {
  return showDialog<bool>(
    context: context,
    builder: (_) => _EditarPrecioDialog(
      presupuestoId: presupuestoId,
      partida: partida,
    ),
  );
}

class _EditarPrecioDialog extends ConsumerStatefulWidget {
  const _EditarPrecioDialog({
    required this.presupuestoId,
    required this.partida,
  });

  final String presupuestoId;
  final PartidaDetalle partida;

  @override
  ConsumerState<_EditarPrecioDialog> createState() =>
      _EditarPrecioDialogState();
}

class _EditarPrecioDialogState extends ConsumerState<_EditarPrecioDialog> {
  late final TextEditingController _precio =
      TextEditingController(text: widget.partida.precio.toString());
  bool _guardando = false;

  @override
  void dispose() {
    _precio.dispose();
    super.dispose();
  }

  Future<void> _guardar() async {
    final valor = double.tryParse(_precio.text.trim().replaceAll(',', '.'));
    if (valor == null) {
      mostrarErrorSnack(context, 'Introduce un precio válido.');
      return;
    }
    setState(() => _guardando = true);
    try {
      await ref
          .read(presupuestosRepositoryProvider)
          .editarPrecio(widget.partida.id, valor);
      ref.invalidate(presupuestoDetalleProvider(widget.presupuestoId));
      if (mounted) Navigator.of(context).pop(true);
    } catch (e) {
      if (mounted) {
        setState(() => _guardando = false);
        mostrarErrorSnack(context, e);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Editar precio'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text('${widget.partida.codigo}  ${widget.partida.resumen}'),
          const SizedBox(height: 12),
          TextField(
            controller: _precio,
            autofocus: true,
            keyboardType:
                const TextInputType.numberWithOptions(decimal: true),
            decoration: const InputDecoration(
              labelText: 'Nuevo precio (€)',
              suffixText: '€',
            ),
          ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: _guardando ? null : () => Navigator.of(context).pop(),
          child: const Text('Cancelar'),
        ),
        FilledButton(
          onPressed: _guardando ? null : _guardar,
          child: _guardando
              ? const SizedBox(
                  width: 18,
                  height: 18,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Text('Guardar'),
        ),
      ],
    );
  }
}

/// Diálogo para actualizar los precios del presupuesto desde un preciosario.
Future<bool?> mostrarDialogoActualizarPrecios(
  BuildContext context,
  WidgetRef ref, {
  required String presupuestoId,
}) {
  return showDialog<bool>(
    context: context,
    builder: (_) => _ActualizarPreciosDialog(presupuestoId: presupuestoId),
  );
}

class _ActualizarPreciosDialog extends ConsumerStatefulWidget {
  const _ActualizarPreciosDialog({required this.presupuestoId});

  final String presupuestoId;

  @override
  ConsumerState<_ActualizarPreciosDialog> createState() =>
      _ActualizarPreciosDialogState();
}

class _ActualizarPreciosDialogState
    extends ConsumerState<_ActualizarPreciosDialog> {
  String? _preciosarioId;
  bool _guardando = false;
  ActualizacionPreciosResultado? _resultado;

  Future<void> _actualizar() async {
    if (_preciosarioId == null) return;
    setState(() => _guardando = true);
    try {
      final res = await ref
          .read(presupuestosRepositoryProvider)
          .actualizarPrecios(widget.presupuestoId, _preciosarioId!);
      ref.invalidate(presupuestoDetalleProvider(widget.presupuestoId));
      if (mounted) {
        setState(() {
          _guardando = false;
          _resultado = res;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _guardando = false);
        mostrarErrorSnack(context, e);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final preciosarios = ref.watch(preciosariosProvider);
    final res = _resultado;

    return AlertDialog(
      title: const Text('Actualizar precios'),
      content: SizedBox(
        width: 380,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const Text('Selecciona el preciosario de referencia:'),
            const SizedBox(height: 12),
            preciosarios.when(
              loading: () => const Center(
                child: Padding(
                  padding: EdgeInsets.all(8),
                  child: CircularProgressIndicator(),
                ),
              ),
              error: (e, _) => Text('Error al cargar preciosarios: $e'),
              data: (lista) {
                if (lista.isEmpty) {
                  return const Text('No hay preciosarios disponibles.');
                }
                return DropdownButtonFormField<String>(
                  value: _preciosarioId,
                  isExpanded: true,
                  decoration:
                      const InputDecoration(labelText: 'Preciosario'),
                  items: [
                    for (final p in lista)
                      DropdownMenuItem(
                        value: p.id,
                        child: Text('${p.nombre} (${p.version})',
                            overflow: TextOverflow.ellipsis),
                      ),
                  ],
                  onChanged: (v) => setState(() => _preciosarioId = v),
                );
              },
            ),
            if (res != null) ...[
              const Divider(height: 24),
              Text('Resultado',
                  style: Theme.of(context).textTheme.titleSmall),
              const SizedBox(height: 8),
              _filaResumen('Actualizadas', res.actualizadas),
              _filaResumen('Respetadas (bloqueadas)', res.respetadas),
              _filaResumen('Sin correspondencia', res.sinCorrespondencia),
              _filaResumen('Sin cambio', res.sinCambio),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: _guardando
              ? null
              : () => Navigator.of(context).pop(_resultado != null),
          child: Text(_resultado == null ? 'Cancelar' : 'Cerrar'),
        ),
        if (_resultado == null)
          FilledButton(
            onPressed:
                (_guardando || _preciosarioId == null) ? null : _actualizar,
            child: _guardando
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Text('Actualizar'),
          ),
      ],
    );
  }

  Widget _filaResumen(String label, int valor) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 2),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [Text(label), Text('$valor')],
      ),
    );
  }
}

/// Diálogo para elegir otro presupuesto del mismo proyecto y compararlo con
/// el actual. Devuelve el id del presupuesto B elegido (o null si se cancela).
Future<String?> mostrarDialogoElegirComparacion(
  BuildContext context,
  WidgetRef ref, {
  required String proyectoId,
  required String presupuestoActualId,
}) {
  return showDialog<String>(
    context: context,
    builder: (_) => _ElegirComparacionDialog(
      proyectoId: proyectoId,
      presupuestoActualId: presupuestoActualId,
    ),
  );
}

class _ElegirComparacionDialog extends ConsumerStatefulWidget {
  const _ElegirComparacionDialog({
    required this.proyectoId,
    required this.presupuestoActualId,
  });

  final String proyectoId;
  final String presupuestoActualId;

  @override
  ConsumerState<_ElegirComparacionDialog> createState() =>
      _ElegirComparacionDialogState();
}

class _ElegirComparacionDialogState
    extends ConsumerState<_ElegirComparacionDialog> {
  String? _seleccionado;

  @override
  Widget build(BuildContext context) {
    final presupuestos =
        ref.watch(presupuestosPorProyectoProvider(widget.proyectoId));

    return AlertDialog(
      title: const Text('Comparar con…'),
      content: SizedBox(
        width: 380,
        child: presupuestos.when(
          loading: () => const Center(
            child: Padding(
              padding: EdgeInsets.all(16),
              child: CircularProgressIndicator(),
            ),
          ),
          error: (e, _) => Text('Error al cargar presupuestos: $e'),
          data: (lista) {
            final otros = lista
                .where((p) => p.id != widget.presupuestoActualId)
                .toList(growable: false);
            if (otros.isEmpty) {
              return const Text(
                  'No hay otros presupuestos en este proyecto para comparar.');
            }
            return DropdownButtonFormField<String>(
              value: _seleccionado,
              isExpanded: true,
              decoration:
                  const InputDecoration(labelText: 'Presupuesto B'),
              items: [
                for (final p in otros)
                  DropdownMenuItem(
                    value: p.id,
                    child: Text('${p.nombre} · v${p.version}',
                        overflow: TextOverflow.ellipsis),
                  ),
              ],
              onChanged: (v) => setState(() => _seleccionado = v),
            );
          },
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancelar'),
        ),
        FilledButton(
          onPressed: _seleccionado == null
              ? null
              : () => Navigator.of(context).pop(_seleccionado),
          child: const Text('Comparar'),
        ),
      ],
    );
  }
}
