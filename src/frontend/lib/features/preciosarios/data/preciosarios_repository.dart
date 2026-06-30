import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_provider.dart';
import '../domain/preciosario.dart';

/// Repositorio del catálogo de precios contra la API REST .NET.
///
/// Endpoints:
///  - `GET /api/preciosarios`
///  - `GET /api/preciosarios/{preciosarioId}/capitulos?padreId=`
///  - `GET /api/preciosarios/capitulos/{capituloId}/partidas`
///  - `GET /api/preciosarios/partidas/{partidaId}/analisis`
class PreciosariosRepository {
  PreciosariosRepository(this._dio);

  final Dio _dio;

  Future<List<Preciosario>> listar() async {
    final res = await _dio.get<List<dynamic>>('/api/preciosarios');
    return (res.data ?? const [])
        .map((e) => Preciosario.fromJson(e as Map<String, dynamic>))
        .toList(growable: false);
  }

  /// Capítulos del preciosario. Sin [padreId] devuelve las raíces.
  Future<List<CapituloCatalogo>> capitulos(
    String preciosarioId, {
    String? padreId,
  }) async {
    final res = await _dio.get<List<dynamic>>(
      '/api/preciosarios/$preciosarioId/capitulos',
      queryParameters: {if (padreId != null) 'padreId': padreId},
    );
    return (res.data ?? const [])
        .map((e) => CapituloCatalogo.fromJson(e as Map<String, dynamic>))
        .toList(growable: false);
  }

  Future<List<PartidaCatalogo>> partidas(String capituloId) async {
    final res = await _dio.get<List<dynamic>>(
      '/api/preciosarios/capitulos/$capituloId/partidas',
    );
    return (res.data ?? const [])
        .map((e) => PartidaCatalogo.fromJson(e as Map<String, dynamic>))
        .toList(growable: false);
  }

  Future<AnalisisPrecios> analisis(String partidaId) async {
    final res = await _dio.get<Map<String, dynamic>>(
      '/api/preciosarios/partidas/$partidaId/analisis',
    );
    return AnalisisPrecios.fromJson(res.data!);
  }

  /// Importa un fichero DCF (FIEBDC) mediante multipart/form-data.
  ///
  /// El campo del formulario es `archivo`. Devuelve el resumen de importación.
  Future<ImportacionResultado> importar({
    required List<int> bytes,
    required String nombreArchivo,
  }) async {
    final form = FormData.fromMap({
      'archivo': MultipartFile.fromBytes(bytes, filename: nombreArchivo),
    });
    final res = await _dio.post<Map<String, dynamic>>(
      '/api/preciosarios/importar',
      data: form,
    );
    return ImportacionResultado.fromJson(res.data!);
  }
}

/// Provider del repositorio de preciosarios.
final preciosariosRepositoryProvider =
    Provider<PreciosariosRepository>((ref) {
  return PreciosariosRepository(ref.watch(dioProvider));
});

/// Lista de preciosarios.
final preciosariosProvider = FutureProvider<List<Preciosario>>((ref) {
  return ref.watch(preciosariosRepositoryProvider).listar();
});

/// Argumentos para consultar capítulos (preciosario + padre opcional).
class CapitulosArgs {
  const CapitulosArgs({required this.preciosarioId, this.padreId});

  final String preciosarioId;
  final String? padreId;

  @override
  bool operator ==(Object other) =>
      other is CapitulosArgs &&
      other.preciosarioId == preciosarioId &&
      other.padreId == padreId;

  @override
  int get hashCode => Object.hash(preciosarioId, padreId);
}

/// Capítulos de un preciosario (raíces o hijos de [padreId]).
final capitulosProvider =
    FutureProvider.family<List<CapituloCatalogo>, CapitulosArgs>((ref, args) {
  return ref
      .watch(preciosariosRepositoryProvider)
      .capitulos(args.preciosarioId, padreId: args.padreId);
});

/// Partidas de un capítulo.
final partidasProvider =
    FutureProvider.family<List<PartidaCatalogo>, String>((ref, capituloId) {
  return ref.watch(preciosariosRepositoryProvider).partidas(capituloId);
});

/// Análisis de precios de una partida.
final analisisProvider =
    FutureProvider.family<AnalisisPrecios, String>((ref, partidaId) {
  return ref.watch(preciosariosRepositoryProvider).analisis(partidaId);
});
