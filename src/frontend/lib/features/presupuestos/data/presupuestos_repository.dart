import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_provider.dart';
import '../domain/actualizacion_precios.dart';
import '../domain/comparacion.dart';
import '../domain/presupuesto.dart';

/// Línea de medición para el alta de una partida.
///
/// Se serializa a JSON camelCase para el endpoint de alta de partidas.
class LineaMedicionInput {
  const LineaMedicionInput({
    this.comentario,
    this.uds,
    this.largo,
    this.ancho,
    this.alto,
    this.coeficiente,
    this.formula,
    this.esComentario = false,
  });

  final String? comentario;
  final double? uds;
  final double? largo;
  final double? ancho;
  final double? alto;
  final double? coeficiente;
  final String? formula;
  final bool esComentario;

  Map<String, dynamic> toJson() => {
        if (comentario != null && comentario!.isNotEmpty)
          'comentario': comentario,
        if (uds != null) 'uds': uds,
        if (largo != null) 'largo': largo,
        if (ancho != null) 'ancho': ancho,
        if (alto != null) 'alto': alto,
        if (coeficiente != null) 'coeficiente': coeficiente,
        if (formula != null && formula!.isNotEmpty) 'formula': formula,
        'esComentario': esComentario,
      };
}

/// Repositorio de presupuestos contra la API REST .NET.
///
/// Endpoints:
///  - `GET   /api/presupuestos?proyectoId={id}`                  -> lista (resumen)
///  - `GET   /api/presupuestos/{id}`                             -> detalle (árbol + totales)
///  - `POST  /api/presupuestos`                                  -> crear (guid)
///  - `POST  /api/presupuestos/{id}/capitulos`                   -> añadir capítulo (guid)
///  - `POST  /api/presupuestos/capitulos/{capituloId}/partidas`  -> añadir partida (guid)
///  - `PATCH /api/presupuestos/partidas/{partidaId}/precio`      -> editar precio (guid)
///  - `PATCH /api/presupuestos/partidas/{partidaId}/bloqueo`     -> bloquear/desbloquear (guid)
///  - `POST  /api/presupuestos/{id}/actualizar-precios`          -> actualizar precios (resumen)
///  - `POST  /api/presupuestos/partidas/{partidaId}/duplicar`    -> duplicar partida (guid)
///  - `POST  /api/presupuestos/capitulos/{capituloId}/duplicar`  -> duplicar capítulo (guid)
///  - `POST  /api/presupuestos/{id}/versiones`                   -> crear versión (guid)
///  - `GET   /api/presupuestos/comparar?a={guid}&b={guid}`       -> comparación
///  - `GET   /api/presupuestos/{id}/pdf`                         -> PDF (bytes)
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

  /// Crea un presupuesto. Devuelve el id (guid) del presupuesto creado.
  Future<String> crear({
    required String proyectoId,
    required String nombre,
    double? ivaPct,
    double? costesIndirectosPct,
    double? bajaPct,
    double? descuentosImporte,
  }) async {
    final res = await _dio.post<dynamic>(
      '/api/presupuestos',
      data: {
        'proyectoId': proyectoId,
        'nombre': nombre,
        if (ivaPct != null) 'ivaPct': ivaPct,
        if (costesIndirectosPct != null)
          'costesIndirectosPct': costesIndirectosPct,
        if (bajaPct != null) 'bajaPct': bajaPct,
        if (descuentosImporte != null) 'descuentosImporte': descuentosImporte,
      },
    );
    return _guid(res.data);
  }

  /// Añade un capítulo al presupuesto. Devuelve el id (guid) del capítulo.
  Future<String> anadirCapitulo({
    required String presupuestoId,
    required String codigo,
    required String titulo,
    String? padreId,
    int? orden,
  }) async {
    final res = await _dio.post<dynamic>(
      '/api/presupuestos/$presupuestoId/capitulos',
      data: {
        'codigo': codigo,
        'titulo': titulo,
        if (padreId != null) 'padreId': padreId,
        if (orden != null) 'orden': orden,
      },
    );
    return _guid(res.data);
  }

  /// Añade una partida a un capítulo con sus líneas de medición.
  /// Devuelve el id (guid) de la partida.
  Future<String> anadirPartida({
    required String capituloId,
    required String codigo,
    required String resumen,
    required double precio,
    List<LineaMedicionInput> lineas = const [],
    String? partidaOrigenId,
  }) async {
    final res = await _dio.post<dynamic>(
      '/api/presupuestos/capitulos/$capituloId/partidas',
      data: {
        'codigo': codigo,
        'resumen': resumen,
        'precio': precio,
        'lineas': lineas.map((l) => l.toJson()).toList(growable: false),
        if (partidaOrigenId != null) 'partidaOrigenId': partidaOrigenId,
      },
    );
    return _guid(res.data);
  }

  /// Edita el precio de una partida. Devuelve el id (guid) afectado.
  Future<String> editarPrecio(String partidaId, double nuevoPrecio) async {
    final res = await _dio.patch<dynamic>(
      '/api/presupuestos/partidas/$partidaId/precio',
      data: {'nuevoPrecio': nuevoPrecio},
    );
    return _guid(res.data);
  }

  /// Bloquea o desbloquea el precio de una partida. Devuelve el id afectado.
  Future<String> bloquearPrecio(String partidaId, bool bloqueado) async {
    final res = await _dio.patch<dynamic>(
      '/api/presupuestos/partidas/$partidaId/bloqueo',
      data: {'bloqueado': bloqueado},
    );
    return _guid(res.data);
  }

  /// Actualiza los precios del presupuesto desde un preciosario.
  Future<ActualizacionPreciosResultado> actualizarPrecios(
    String presupuestoId,
    String preciosarioId,
  ) async {
    final res = await _dio.post<Map<String, dynamic>>(
      '/api/presupuestos/$presupuestoId/actualizar-precios',
      data: {'preciosarioId': preciosarioId},
    );
    return ActualizacionPreciosResultado.fromJson(res.data!);
  }

  /// Duplica una partida. Devuelve el id (guid) de la nueva partida.
  Future<String> duplicarPartida(String partidaId) async {
    final res = await _dio.post<dynamic>(
      '/api/presupuestos/partidas/$partidaId/duplicar',
    );
    return _guid(res.data);
  }

  /// Duplica un capítulo. Devuelve el id (guid) del nuevo capítulo.
  Future<String> duplicarCapitulo(String capituloId) async {
    final res = await _dio.post<dynamic>(
      '/api/presupuestos/capitulos/$capituloId/duplicar',
    );
    return _guid(res.data);
  }

  /// Crea una nueva versión del presupuesto. Devuelve el id (guid) de la versión.
  Future<String> crearVersion(String presupuestoId) async {
    final res = await _dio.post<dynamic>(
      '/api/presupuestos/$presupuestoId/versiones',
    );
    return _guid(res.data);
  }

  /// Compara dos presupuestos (A vs B).
  Future<ComparacionResultado> comparar(String aId, String bId) async {
    final res = await _dio.get<Map<String, dynamic>>(
      '/api/presupuestos/comparar',
      queryParameters: {'a': aId, 'b': bId},
    );
    return ComparacionResultado.fromJson(res.data!);
  }

  /// Descarga el PDF del presupuesto como bytes.
  Future<List<int>> descargarPdf(String presupuestoId) async {
    final res = await _dio.get<List<int>>(
      '/api/presupuestos/$presupuestoId/pdf',
      options: Options(responseType: ResponseType.bytes),
    );
    return res.data ?? const [];
  }

  /// Normaliza la respuesta de un endpoint `Result<Guid>`: el backend devuelve
  /// el GUID como string JSON (p. ej. body = "3fa85f64-..."), aunque también
  /// se contempla un posible objeto `{ id: ... }`.
  String _guid(dynamic data) {
    if (data is String) return data;
    if (data is Map<String, dynamic>) {
      final id = data['id'] ?? data['value'];
      if (id is String) return id;
    }
    return data?.toString() ?? '';
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
