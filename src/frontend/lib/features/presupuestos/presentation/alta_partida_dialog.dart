import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/errores.dart';
import '../data/presupuestos_repository.dart';

/// Diálogo de alta de una partida con sus líneas de medición.
///
/// Permite definir código, resumen, precio y varias líneas de medición
/// (uds, largo, ancho, alto, coeficiente, fórmula y comentario opcionales).
Future<bool?> mostrarDialogoAltaPartida(
  BuildContext context,
  WidgetRef ref, {
  required String presupuestoId,
  required String capituloId,
}) {
  return showDialog<bool>(
    context: context,
    builder: (_) => _AltaPartidaDialog(
      presupuestoId: presupuestoId,
      capituloId: capituloId,
    ),
  );
}

/// Estado mutable de una línea de medición en el formulario.
class _LineaCtrl {
  final comentario = TextEditingController();
  final uds = TextEditingController();
  final largo = TextEditingController();
  final ancho = TextEditingController();
  final alto = TextEditingController();
  final coeficiente = TextEditingController();
  final formula = TextEditingController();
  bool esComentario = false;

  void dispose() {
    comentario.dispose();
    uds.dispose();
    largo.dispose();
    ancho.dispose();
    alto.dispose();
    coeficiente.dispose();
    formula.dispose();
  }

  static double? _num(TextEditingController c) {
    final t = c.text.trim().replaceAll(',', '.');
    if (t.isEmpty) return null;
    return double.tryParse(t);
  }

  LineaMedicionInput toInput() => LineaMedicionInput(
        comentario: comentario.text.trim().isEmpty
            ? null
            : comentario.text.trim(),
        uds: esComentario ? null : _num(uds),
        largo: esComentario ? null : _num(largo),
        ancho: esComentario ? null : _num(ancho),
        alto: esComentario ? null : _num(alto),
        coeficiente: esComentario ? null : _num(coeficiente),
        formula: esComentario || formula.text.trim().isEmpty
            ? null
            : formula.text.trim(),
        esComentario: esComentario,
      );
}

class _AltaPartidaDialog extends ConsumerStatefulWidget {
  const _AltaPartidaDialog({
    required this.presupuestoId,
    required this.capituloId,
  });

  final String presupuestoId;
  final String capituloId;

  @override
  ConsumerState<_AltaPartidaDialog> createState() => _AltaPartidaDialogState();
}

class _AltaPartidaDialogState extends ConsumerState<_AltaPartidaDialog> {
  final _formKey = GlobalKey<FormState>();
  final _codigo = TextEditingController();
  final _resumen = TextEditingController();
  final _precio = TextEditingController();
  final List<_LineaCtrl> _lineas = [_LineaCtrl()];
  bool _guardando = false;

  @override
  void dispose() {
    _codigo.dispose();
    _resumen.dispose();
    _precio.dispose();
    for (final l in _lineas) {
      l.dispose();
    }
    super.dispose();
  }

  void _anadirLinea() => setState(() => _lineas.add(_LineaCtrl()));

  void _quitarLinea(int i) {
    setState(() {
      _lineas.removeAt(i).dispose();
      if (_lineas.isEmpty) _lineas.add(_LineaCtrl());
    });
  }

  Future<void> _guardar() async {
    if (!_formKey.currentState!.validate()) return;
    final precio =
        double.tryParse(_precio.text.trim().replaceAll(',', '.')) ?? 0;
    final lineas = _lineas
        .map((l) => l.toInput())
        .where((l) =>
            l.esComentario ||
            l.uds != null ||
            l.largo != null ||
            l.ancho != null ||
            l.alto != null ||
            l.coeficiente != null ||
            (l.formula != null && l.formula!.isNotEmpty) ||
            (l.comentario != null && l.comentario!.isNotEmpty))
        .toList(growable: false);

    setState(() => _guardando = true);
    try {
      await ref.read(presupuestosRepositoryProvider).anadirPartida(
            capituloId: widget.capituloId,
            codigo: _codigo.text.trim(),
            resumen: _resumen.text.trim(),
            precio: precio,
            lineas: lineas,
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
    final theme = Theme.of(context);
    return AlertDialog(
      title: const Text('Añadir partida'),
      content: SizedBox(
        width: 560,
        child: Form(
          key: _formKey,
          child: SingleChildScrollView(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Row(
                  children: [
                    Expanded(
                      flex: 2,
                      child: TextFormField(
                        controller: _codigo,
                        decoration:
                            const InputDecoration(labelText: 'Código'),
                        validator: (v) => (v == null || v.trim().isEmpty)
                            ? 'Indica un código'
                            : null,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: TextFormField(
                        controller: _precio,
                        decoration: const InputDecoration(
                          labelText: 'Precio',
                          suffixText: '€',
                        ),
                        keyboardType: const TextInputType.numberWithOptions(
                            decimal: true),
                        validator: (v) {
                          final n = double.tryParse(
                              (v ?? '').trim().replaceAll(',', '.'));
                          if (n == null) return 'Precio no válido';
                          return null;
                        },
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _resumen,
                  decoration: const InputDecoration(labelText: 'Resumen'),
                  validator: (v) => (v == null || v.trim().isEmpty)
                      ? 'Indica un resumen'
                      : null,
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Text('Mediciones', style: theme.textTheme.titleSmall),
                    const Spacer(),
                    TextButton.icon(
                      onPressed: _anadirLinea,
                      icon: const Icon(Icons.add, size: 18),
                      label: const Text('Añadir línea'),
                    ),
                  ],
                ),
                const SizedBox(height: 4),
                for (var i = 0; i < _lineas.length; i++)
                  _LineaMedicionCard(
                    key: ValueKey(_lineas[i]),
                    ctrl: _lineas[i],
                    indice: i + 1,
                    onQuitar: () => _quitarLinea(i),
                    onCambioTipo: () => setState(() {}),
                  ),
              ],
            ),
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
              : const Text('Guardar'),
        ),
      ],
    );
  }
}

class _LineaMedicionCard extends StatelessWidget {
  const _LineaMedicionCard({
    super.key,
    required this.ctrl,
    required this.indice,
    required this.onQuitar,
    required this.onCambioTipo,
  });

  final _LineaCtrl ctrl;
  final int indice;
  final VoidCallback onQuitar;
  final VoidCallback onCambioTipo;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(vertical: 4),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Text('Línea $indice'),
                const Spacer(),
                Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Text('Comentario'),
                    Switch(
                      value: ctrl.esComentario,
                      onChanged: (v) {
                        ctrl.esComentario = v;
                        onCambioTipo();
                      },
                    ),
                  ],
                ),
                IconButton(
                  tooltip: 'Quitar línea',
                  icon: const Icon(Icons.delete_outline),
                  onPressed: onQuitar,
                ),
              ],
            ),
            TextFormField(
              controller: ctrl.comentario,
              decoration:
                  const InputDecoration(labelText: 'Comentario (opcional)'),
            ),
            if (!ctrl.esComentario) ...[
              const SizedBox(height: 8),
              Wrap(
                spacing: 12,
                runSpacing: 8,
                children: [
                  _campoNumero(ctrl.uds, 'Uds'),
                  _campoNumero(ctrl.largo, 'Largo'),
                  _campoNumero(ctrl.ancho, 'Ancho'),
                  _campoNumero(ctrl.alto, 'Alto'),
                  _campoNumero(ctrl.coeficiente, 'Coef.'),
                ],
              ),
              const SizedBox(height: 8),
              TextFormField(
                controller: ctrl.formula,
                decoration: const InputDecoration(
                  labelText: 'Fórmula (opcional)',
                  hintText: 'p. ej. uds*largo*ancho',
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _campoNumero(TextEditingController c, String label) {
    return SizedBox(
      width: 92,
      child: TextFormField(
        controller: c,
        decoration: InputDecoration(labelText: label, isDense: true),
        keyboardType: const TextInputType.numberWithOptions(decimal: true),
      ),
    );
  }
}
