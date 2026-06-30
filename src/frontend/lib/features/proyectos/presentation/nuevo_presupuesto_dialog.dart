import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../shared/utils/errores.dart';
import '../../presupuestos/data/presupuestos_repository.dart';

/// Muestra el diálogo de alta de un presupuesto para el proyecto indicado.
///
/// Tras crear el presupuesto, refresca la lista del proyecto y navega al
/// presupuesto recién creado.
Future<void> mostrarDialogoNuevoPresupuesto(
  BuildContext context,
  WidgetRef ref, {
  required String proyectoId,
}) {
  return showDialog<void>(
    context: context,
    builder: (_) => _NuevoPresupuestoDialog(proyectoId: proyectoId),
  );
}

class _NuevoPresupuestoDialog extends ConsumerStatefulWidget {
  const _NuevoPresupuestoDialog({required this.proyectoId});

  final String proyectoId;

  @override
  ConsumerState<_NuevoPresupuestoDialog> createState() =>
      _NuevoPresupuestoDialogState();
}

class _NuevoPresupuestoDialogState
    extends ConsumerState<_NuevoPresupuestoDialog> {
  final _formKey = GlobalKey<FormState>();
  final _nombre = TextEditingController();
  final _iva = TextEditingController(text: '21');
  final _costesIndirectos = TextEditingController();
  final _baja = TextEditingController();
  final _descuentos = TextEditingController();
  bool _guardando = false;

  @override
  void dispose() {
    _nombre.dispose();
    _iva.dispose();
    _costesIndirectos.dispose();
    _baja.dispose();
    _descuentos.dispose();
    super.dispose();
  }

  double? _num(TextEditingController c) {
    final t = c.text.trim().replaceAll(',', '.');
    if (t.isEmpty) return null;
    return double.tryParse(t);
  }

  Future<void> _guardar() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _guardando = true);
    try {
      final id = await ref.read(presupuestosRepositoryProvider).crear(
            proyectoId: widget.proyectoId,
            nombre: _nombre.text.trim(),
            ivaPct: _num(_iva),
            costesIndirectosPct: _num(_costesIndirectos),
            bajaPct: _num(_baja),
            descuentosImporte: _num(_descuentos),
          );
      ref.invalidate(presupuestosPorProyectoProvider(widget.proyectoId));
      if (!mounted) return;
      Navigator.of(context).pop();
      mostrarSnack(context, 'Presupuesto creado.');
      context.go('/presupuestos/$id?proyectoId=${widget.proyectoId}');
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
      title: const Text('Nuevo presupuesto'),
      content: SizedBox(
        width: 420,
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                controller: _nombre,
                autofocus: true,
                decoration: const InputDecoration(labelText: 'Nombre'),
                validator: (v) => (v == null || v.trim().isEmpty)
                    ? 'Indica un nombre'
                    : null,
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  Expanded(child: _campoPorcentaje(_iva, 'IVA %')),
                  const SizedBox(width: 12),
                  Expanded(
                      child: _campoPorcentaje(
                          _costesIndirectos, 'Costes indirectos %')),
                ],
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  Expanded(child: _campoPorcentaje(_baja, 'Baja %')),
                  const SizedBox(width: 12),
                  Expanded(
                    child: TextFormField(
                      controller: _descuentos,
                      decoration: const InputDecoration(
                        labelText: 'Descuentos',
                        suffixText: '€',
                      ),
                      keyboardType: const TextInputType.numberWithOptions(
                          decimal: true),
                    ),
                  ),
                ],
              ),
            ],
          ),
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
              : const Text('Crear'),
        ),
      ],
    );
  }

  Widget _campoPorcentaje(TextEditingController c, String label) {
    return TextFormField(
      controller: c,
      decoration: InputDecoration(labelText: label, suffixText: '%'),
      keyboardType: const TextInputType.numberWithOptions(decimal: true),
    );
  }
}
