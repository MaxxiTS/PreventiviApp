import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_provider.dart';
import '../domain/proyecto.dart';

/// Repositorio de acceso a datos de proyectos contra la API REST .NET.
///
/// Endpoints:
///  - `GET  /api/proyectos`        -> lista de proyectos
///  - `GET  /api/proyectos/{id}`   -> detalle
///  - `POST /api/proyectos`        -> crear
class ProyectosRepository {
  ProyectosRepository(this._dio);

  final Dio _dio;

  /// Lista todos los proyectos.
  Future<List<Proyecto>> listar() async {
    final res = await _dio.get<List<dynamic>>('/api/proyectos');
    final datos = res.data ?? const [];
    return datos
        .map((e) => Proyecto.fromJson(e as Map<String, dynamic>))
        .toList(growable: false);
  }

  /// Obtiene un proyecto por su id.
  Future<Proyecto> obtener(String id) async {
    final res = await _dio.get<Map<String, dynamic>>('/api/proyectos/$id');
    return Proyecto.fromJson(res.data!);
  }

  /// Crea un proyecto. Solo `nombre` es obligatorio.
  Future<Proyecto> crear({
    required String nombre,
    String? clienteId,
    String? direccion,
    String? fecha,
  }) async {
    final res = await _dio.post<Map<String, dynamic>>(
      '/api/proyectos',
      data: {
        'nombre': nombre,
        if (clienteId != null) 'clienteId': clienteId,
        if (direccion != null && direccion.isNotEmpty) 'direccion': direccion,
        if (fecha != null && fecha.isNotEmpty) 'fecha': fecha,
      },
    );
    return Proyecto.fromJson(res.data!);
  }
}

/// Provider del repositorio de proyectos.
final proyectosRepositoryProvider = Provider<ProyectosRepository>((ref) {
  return ProyectosRepository(ref.watch(dioProvider));
});
