import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../shared/utils/formato.dart';
import '../../../shared/widgets/async_value_widget.dart';
import '../../../shared/widgets/page_header.dart';
import '../data/proyectos_repository.dart';
import '../domain/proyecto.dart';
import 'proyectos_providers.dart';

/// Pantalla de listado de proyectos con creación rápida y navegación a detalle.
class ProyectosScreen extends ConsumerWidget {
  const ProyectosScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final proyectos = ref.watch(proyectosProvider);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(
          breadcrumbs: const ['Proyectos'],
          subtitle: 'Gestiona los proyectos de obra y sus presupuestos.',
          actions: [
            FilledButton.icon(
              onPressed: () => _nuevoProyecto(context, ref),
              icon: const Icon(Icons.add, size: 18),
              label: const Text('Nuevo proyecto'),
            ),
          ],
        ),
        const Divider(),
        Expanded(
          child: AsyncValueWidget<List<Proyecto>>(
            value: proyectos,
            onRetry: () => ref.invalidate(proyectosProvider),
            data: (lista) {
              if (lista.isEmpty) {
                return const _EmptyState();
              }
              return RefreshIndicator(
                onRefresh: () async => ref.invalidate(proyectosProvider),
                child: ListView.separated(
                  padding: const EdgeInsets.all(16),
                  itemCount: lista.length,
                  separatorBuilder: (_, __) => const SizedBox(height: 8),
                  itemBuilder: (context, i) =>
                      _ProyectoCard(proyecto: lista[i]),
                ),
              );
            },
          ),
        ),
      ],
    );
  }

  Future<void> _nuevoProyecto(BuildContext context, WidgetRef ref) async {
    final creado = await showDialog<bool>(
      context: context,
      builder: (_) => const _NuevoProyectoDialog(),
    );
    if (creado == true) {
      ref.invalidate(proyectosProvider);
    }
  }
}

class _ProyectoCard extends StatelessWidget {
  const _ProyectoCard({required this.proyecto});

  final Proyecto proyecto;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      child: ListTile(
        onTap: () => context.go('/proyectos/${proyecto.id}'),
        title: Text(
          proyecto.nombre,
          style: theme.textTheme.titleSmall
              ?.copyWith(fontWeight: FontWeight.w600),
        ),
        subtitle: Text(
          [
            if (proyecto.direccion != null && proyecto.direccion!.isNotEmpty)
              proyecto.direccion!,
            if (proyecto.fecha != null && proyecto.fecha!.isNotEmpty)
              Formato.fecha(proyecto.fecha),
          ].join(' · '),
        ),
        trailing: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            _EstadoPill(estado: proyecto.estado),
            const SizedBox(width: 8),
            const Icon(Icons.chevron_right),
          ],
        ),
      ),
    );
  }
}

/// "Pill" de estado del proyecto (estilo Linear).
class _EstadoPill extends StatelessWidget {
  const _EstadoPill({required this.estado});

  final String estado;

  @override
  Widget build(BuildContext context) {
    final scheme = Theme.of(context).colorScheme;
    final color = switch (estado.toLowerCase()) {
      'activo' => Colors.green,
      'cerrado' => scheme.primary,
      'archivado' => Colors.grey,
      _ => Colors.orange, // borrador / desconocido
    };
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(
        estado,
        style: TextStyle(
            color: color, fontSize: 12, fontWeight: FontWeight.w600),
      ),
    );
  }
}

class _EmptyState extends StatelessWidget {
  const _EmptyState();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.folder_open,
              size: 48,
              color: theme.colorScheme.onSurface.withValues(alpha: 0.3)),
          const SizedBox(height: 12),
          Text('Aún no hay proyectos', style: theme.textTheme.titleMedium),
          const SizedBox(height: 4),
          Text(
            'Crea tu primer proyecto con el botón “Nuevo proyecto”.',
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }
}

/// Diálogo simple de creación de proyecto (POST /api/proyectos).
class _NuevoProyectoDialog extends ConsumerStatefulWidget {
  const _NuevoProyectoDialog();

  @override
  ConsumerState<_NuevoProyectoDialog> createState() =>
      _NuevoProyectoDialogState();
}

class _NuevoProyectoDialogState extends ConsumerState<_NuevoProyectoDialog> {
  final _formKey = GlobalKey<FormState>();
  final _nombreCtrl = TextEditingController();
  final _direccionCtrl = TextEditingController();
  bool _guardando = false;
  String? _error;

  @override
  void dispose() {
    _nombreCtrl.dispose();
    _direccionCtrl.dispose();
    super.dispose();
  }

  Future<void> _guardar() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _guardando = true;
      _error = null;
    });
    try {
      await ref.read(proyectosRepositoryProvider).crear(
            nombre: _nombreCtrl.text.trim(),
            direccion: _direccionCtrl.text.trim(),
          );
      if (mounted) Navigator.of(context).pop(true);
    } catch (e) {
      setState(() {
        _guardando = false;
        _error = 'No se pudo crear el proyecto. $e';
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Nuevo proyecto'),
      content: Form(
        key: _formKey,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextFormField(
              controller: _nombreCtrl,
              autofocus: true,
              decoration: const InputDecoration(
                labelText: 'Nombre *',
                hintText: 'Reforma Edificio Sol',
              ),
              validator: (v) => (v == null || v.trim().isEmpty)
                  ? 'El nombre es obligatorio'
                  : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _direccionCtrl,
              decoration: const InputDecoration(
                labelText: 'Dirección',
                hintText: 'C/ Mayor 12, Madrid',
              ),
            ),
            if (_error != null) ...[
              const SizedBox(height: 12),
              Text(
                _error!,
                style: TextStyle(color: Theme.of(context).colorScheme.error),
              ),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed:
              _guardando ? null : () => Navigator.of(context).pop(false),
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
}
