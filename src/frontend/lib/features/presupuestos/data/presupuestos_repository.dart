import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_provider.dart';
import '../domain/presupuesto.dart';

/// Repositorio de presupuestos contra la API REST .NET.
///
/// Endpoints:
///  - `GET /api/presupuestos?proyectoId={id}` -> lista (resumen)
///  - `GET /api/presupuestos/{id}`            -> detalle (árbol + totales)
class PresupuestosRepository {
  PresupuestosRepository(this._dio);

  final Dio _dio;

  /// Lista los presupuestos de un proyecto.
  Future<List<PresupuestoResumen>> listarPorProyecto(String proyectoId) async {
    final res = await _dio.get<List<dynamic>>(
      '/api/presupuestos',
      queryParameters: {'proyectoId': proyectoId},
    );
    final datos = res.data ?? const [];
    return datos
        .map((e) => PresupuestoResumen.fromJson(e as Map<String, dynamic>))
        .toList(growable: false);
  }

  /// Obtiene el detalle completo de un presupuesto (árbol y totales).
  Future<PresupuestoDetalle> obtener(String id) async {
    final res = await _dio.get<Map<String, dynamic>>('/api/presupuestos/$id');
    return PresupuestoDetalle.fromJson(res.data!);
  }
}

/// Provider del repositorio de presupuestos.
final presupuestosRepositoryProvider = Provider<PresupuestosRepository>((ref) {
  return PresupuestosRepository(ref.watch(dioProvider));
});

/// Presupuestos de un proyecto (family por proyectoId).
final presupuestosPorProyectoProvider =
    FutureProvider.family<List<PresupuestoResumen>, String>((ref, proyectoId) {
  return ref
      .watch(presupuestosRepositoryProvider)
      .listarPorProyecto(proyectoId);
});

/// Detalle de un presupuesto (family por id).
final presupuestoDetalleProvider =
    FutureProvider.family<PresupuestoDetalle, String>((ref, id) {
  return ref.watch(presupuestosRepositoryProvider).obtener(id);
});
