import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../shared/utils/formato.dart';
import '../../../shared/widgets/async_value_widget.dart';
import '../../../shared/widgets/page_header.dart';
import '../../presupuestos/data/presupuestos_repository.dart';
import '../../presupuestos/domain/presupuesto.dart';
import '../domain/proyecto.dart';
import 'proyectos_providers.dart';

/// Detalle de un proyecto: datos básicos y lista de sus presupuestos.
class ProyectoDetalleScreen extends ConsumerWidget {
  const ProyectoDetalleScreen({super.key, required this.proyectoId});

  final String proyectoId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final proyecto = ref.watch(proyectoProvider(proyectoId));
    final presupuestos =
        ref.watch(presupuestosPorProyectoProvider(proyectoId));

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PageHeader(
          breadcrumbs: [
            'Proyectos',
            proyecto.maybeWhen(
              data: (p) => p.nombre,
              orElse: () => 'Detalle',
            ),
          ],
          actions: [
            TextButton.icon(
              onPressed: () => context.go('/proyectos'),
              icon: const Icon(Icons.arrow_back, size: 18),
              label: const Text('Volver'),
            ),
          ],
        ),
        const Divider(),
        Expanded(
          child: AsyncValueWidget<Proyecto>(
            value: proyecto,
            onRetry: () => ref.invalidate(proyectoProvider(proyectoId)),
            data: (p) => ListView(
              padding: const EdgeInsets.all(16),
              children: [
                _DatosProyecto(proyecto: p),
                const SizedBox(height: 20),
                Text('Presupuestos',
                    style: Theme.of(context).textTheme.titleMedium),
                const SizedBox(height: 8),
                AsyncValueWidget<List<PresupuestoResumen>>(
                  value: presupuestos,
                  onRetry: () => ref.invalidate(
                      presupuestosPorProyectoProvider(proyectoId)),
                  data: (lista) {
                    if (lista.isEmpty) {
                      return const Padding(
                        padding: EdgeInsets.all(24),
                        child: Center(
                            child: Text('Este proyecto aún no tiene presupuestos.')),
                      );
                    }
                    return Column(
                      children: [
                        for (final pr in lista)
                          Card(
                            child: ListTile(
                              onTap: () =>
                                  context.go('/presupuestos/${pr.id}'),
                              title: Text('${pr.nombre} · v${pr.version}'),
                              subtitle: Text(pr.estado),
                              trailing: Text(
                                Formato.euros(pr.total),
                                style: Theme.of(context)
                                    .textTheme
                                    .titleSmall
                                    ?.copyWith(fontWeight: FontWeight.w700),
                              ),
                            ),
                          ),
                      ],
                    );
                  },
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

class _DatosProyecto extends StatelessWidget {
  const _DatosProyecto({required this.proyecto});

  final Proyecto proyecto;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(proyecto.nombre, style: theme.textTheme.titleLarge),
            const SizedBox(height: 8),
            if (proyecto.direccion != null && proyecto.direccion!.isNotEmpty)
              _linea(context, Icons.place_outlined, proyecto.direccion!),
            if (proyecto.fecha != null && proyecto.fecha!.isNotEmpty)
              _linea(context, Icons.calendar_today_outlined,
                  Formato.fecha(proyecto.fecha)),
            _linea(context, Icons.flag_outlined, 'Estado: ${proyecto.estado}'),
          ],
        ),
      ),
    );
  }

  Widget _linea(BuildContext context, IconData icono, String texto) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3),
      child: Row(
        children: [
          Icon(icono, size: 16),
          const SizedBox(width: 8),
          Text(texto, style: Theme.of(context).textTheme.bodyMedium),
        ],
      ),
    );
  }
}
