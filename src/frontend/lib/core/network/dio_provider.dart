import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../config/app_config.dart';

/// Provider del cliente HTTP [Dio] configurado contra la API REST .NET.
///
/// Centraliza `baseUrl`, cabeceras JSON y timeouts. Los repositorios de cada
/// feature dependen de este provider para realizar las peticiones.
final dioProvider = Provider<Dio>((ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.apiBaseUrl,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 30),
      sendTimeout: const Duration(seconds: 30),
      headers: const {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
      },
      // No lanzar excepción salvo en errores reales de red/servidor;
      // el manejo fino de códigos se hace en los repositorios.
      responseType: ResponseType.json,
    ),
  );

  // Log ligero en modo debug (sin dependencias externas).
  assert(() {
    dio.interceptors.add(
      LogInterceptor(
        request: false,
        requestHeader: false,
        responseHeader: false,
        requestBody: true,
        responseBody: false,
      ),
    );
    return true;
  }());

  return dio;
});
