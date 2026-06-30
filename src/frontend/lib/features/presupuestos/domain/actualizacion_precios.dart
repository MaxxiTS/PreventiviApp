/// Modelo de dominio para el resultado de actualizar precios desde un
/// preciosario.
///
/// Mapea el DTO del backend .NET (camelCase) del endpoint
/// `POST /api/presupuestos/{id}/actualizar-precios`:
///  - `ActualizacionPreciosDto { actualizadas, respetadas,
///      sinCorrespondencia, sinCambio }`
class ActualizacionPreciosResultado {
  const ActualizacionPreciosResultado({
    required this.actualizadas,
    required this.respetadas,
    required this.sinCorrespondencia,
    required this.sinCambio,
  });

  /// Partidas cuyo precio se actualizó.
  final int actualizadas;

  /// Partidas bloqueadas que se respetaron (no se tocaron).
  final int respetadas;

  /// Partidas sin correspondencia en el preciosario.
  final int sinCorrespondencia;

  /// Partidas cuyo precio ya coincidía (sin cambios).
  final int sinCambio;

  factory ActualizacionPreciosResultado.fromJson(Map<String, dynamic> json) {
    return ActualizacionPreciosResultado(
      actualizadas: (json['actualizadas'] as num?)?.toInt() ?? 0,
      respetadas: (json['respetadas'] as num?)?.toInt() ?? 0,
      sinCorrespondencia: (json['sinCorrespondencia'] as num?)?.toInt() ?? 0,
      sinCambio: (json['sinCambio'] as num?)?.toInt() ?? 0,
    );
  }
}
