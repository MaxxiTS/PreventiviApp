/// Configuración global de la aplicación.
///
/// La URL base de la API se puede sobrescribir en tiempo de compilación con:
///   flutter run --dart-define=API_BASE_URL=http://192.168.1.50:5000
///
/// Si no se especifica, se usa `http://localhost:5000` (backend .NET en local).
class AppConfig {
  const AppConfig._();

  /// URL base por defecto del backend .NET (sin barra final).
  static const String _defaultApiBaseUrl = 'http://localhost:5000';

  /// URL base efectiva de la API REST.
  ///
  /// Lee `API_BASE_URL` de `--dart-define`; si está vacío, usa el valor por
  /// defecto. Se normaliza eliminando una posible barra final.
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: _defaultApiBaseUrl,
  );

  /// Nombre visible de la aplicación.
  static const String appName = 'Preventivi App';

  /// Locale por defecto para formateo de números/importes (es-ES, €).
  static const String localeEsEs = 'es_ES';
}
