import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

/// Widget reutilizable para renderizar un [AsyncValue] gestionando de forma
/// homogénea los estados de carga, error y datos en toda la app.
///
/// Uso típico:
/// ```dart
/// final proyectos = ref.watch(proyectosProvider);
/// return AsyncValueWidget(
///   value: proyectos,
///   onRetry: () => ref.invalidate(proyectosProvider),
///   data: (lista) => ListView(...),
/// );
/// ```
class AsyncValueWidget<T> extends StatelessWidget {
  const AsyncValueWidget({
    super.key,
    required this.value,
    required this.data,
    this.onRetry,
    this.loadingMessage,
  });

  /// Estado asíncrono a renderizar.
  final AsyncValue<T> value;

  /// Constructor del contenido cuando hay datos.
  final Widget Function(T data) data;

  /// Acción opcional para reintentar (normalmente `ref.invalidate(...)`).
  final VoidCallback? onRetry;

  /// Mensaje opcional mostrado bajo el indicador de carga.
  final String? loadingMessage;

  @override
  Widget build(BuildContext context) {
    return value.when(
      data: data,
      loading: () => _LoadingView(message: loadingMessage),
      error: (error, _) => ErrorView(
        message: _mensajeDeError(error),
        onRetry: onRetry,
      ),
    );
  }

  String _mensajeDeError(Object error) {
    final texto = error.toString();
    // Mensaje algo más amigable para errores de red habituales.
    if (texto.contains('SocketException') ||
        texto.contains('Connection') ||
        texto.contains('connectTimeout')) {
      return 'No se pudo conectar con el servidor. Comprueba que la API esté '
          'en marcha y la URL configurada.';
    }
    return 'Se produjo un error al cargar los datos.\n$texto';
  }
}

class _LoadingView extends StatelessWidget {
  const _LoadingView({this.message});

  final String? message;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const CircularProgressIndicator(),
          if (message != null) ...[
            const SizedBox(height: 12),
            Text(message!, style: Theme.of(context).textTheme.bodySmall),
          ],
        ],
      ),
    );
  }
}

/// Vista de error reutilizable con botón de reintento opcional.
class ErrorView extends StatelessWidget {
  const ErrorView({super.key, required this.message, this.onRetry});

  final String message;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.error_outline,
                size: 40, color: theme.colorScheme.error),
            const SizedBox(height: 12),
            Text(
              message,
              textAlign: TextAlign.center,
              style: theme.textTheme.bodyMedium,
            ),
            if (onRetry != null) ...[
              const SizedBox(height: 16),
              FilledButton.tonalIcon(
                onPressed: onRetry,
                icon: const Icon(Icons.refresh),
                label: const Text('Reintentar'),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
