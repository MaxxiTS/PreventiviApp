import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

/// Utilidades para presentar errores de red de forma homogénea.
///
/// El backend .NET devuelve ProblemDetails (status 400/404/409) con un campo
/// `detail` (y a veces `title`). Esta función extrae el mensaje más útil.
String mensajeError(Object error) {
  if (error is DioException) {
    final data = error.response?.data;
    if (data is Map) {
      final detail = data['detail'] ?? data['title'] ?? data['message'];
      if (detail is String && detail.isNotEmpty) return detail;
    }
    if (data is String && data.isNotEmpty) return data;
    final code = error.response?.statusCode;
    if (code != null) return 'Error del servidor (HTTP $code).';
    return 'No se pudo conectar con el servidor.';
  }
  return error.toString();
}

/// Muestra un SnackBar de error (rojo) con el mensaje del [error].
void mostrarErrorSnack(BuildContext context, Object error) {
  if (!context.mounted) return;
  final theme = Theme.of(context);
  ScaffoldMessenger.of(context)
    ..hideCurrentSnackBar()
    ..showSnackBar(
      SnackBar(
        content: Text(mensajeError(error)),
        backgroundColor: theme.colorScheme.error,
      ),
    );
}

/// Muestra un SnackBar informativo/de éxito con el [mensaje].
void mostrarSnack(BuildContext context, String mensaje) {
  if (!context.mounted) return;
  ScaffoldMessenger.of(context)
    ..hideCurrentSnackBar()
    ..showSnackBar(SnackBar(content: Text(mensaje)));
}
