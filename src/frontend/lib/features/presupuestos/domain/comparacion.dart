/// Modelos de dominio para la comparación de presupuestos/versiones.
///
/// Mapean el DTO del backend .NET (camelCase) del endpoint
/// `GET /api/presupuestos/comparar?a={guid}&b={guid}`:
///  - `ComparacionDto { deltaTotal, lineas[] }`
///  - `LineaComparacionDto { codigo, estado, precioA, precioB, medicionA,
///      medicionB, importeA, importeB, deltaImporte }`

/// Resultado de comparar dos presupuestos (A vs B).
class ComparacionResultado {
  const ComparacionResultado({
    required this.deltaTotal,
    required this.lineas,
  });

  /// Diferencia de total entre B y A (positivo = B es más caro).
  final double deltaTotal;
  final List<LineaComparacion> lineas;

  factory ComparacionResultado.fromJson(Map<String, dynamic> json) {
    return ComparacionResultado(
      deltaTotal: (json['deltaTotal'] as num?)?.toDouble() ?? 0,
      lineas: ((json['lineas'] as List<dynamic>?) ?? const [])
          .map((e) => LineaComparacion.fromJson(e as Map<String, dynamic>))
          .toList(growable: false),
    );
  }
}

/// Línea de la tabla de diferencias entre dos presupuestos.
class LineaComparacion {
  const LineaComparacion({
    required this.codigo,
    required this.estado,
    required this.precioA,
    required this.precioB,
    required this.medicionA,
    required this.medicionB,
    required this.importeA,
    required this.importeB,
    required this.deltaImporte,
  });

  final String codigo;

  /// Estado de la línea: p. ej. `Igual`, `Modificada`, `Nueva`, `Eliminada`.
  final String estado;

  final double precioA;
  final double precioB;
  final double medicionA;
  final double medicionB;
  final double importeA;
  final double importeB;
  final double deltaImporte;

  factory LineaComparacion.fromJson(Map<String, dynamic> json) {
    return LineaComparacion(
      codigo: (json['codigo'] as String?) ?? '',
      estado: (json['estado'] as String?) ?? '',
      precioA: (json['precioA'] as num?)?.toDouble() ?? 0,
      precioB: (json['precioB'] as num?)?.toDouble() ?? 0,
      medicionA: (json['medicionA'] as num?)?.toDouble() ?? 0,
      medicionB: (json['medicionB'] as num?)?.toDouble() ?? 0,
      importeA: (json['importeA'] as num?)?.toDouble() ?? 0,
      importeB: (json['importeB'] as num?)?.toDouble() ?? 0,
      deltaImporte: (json['deltaImporte'] as num?)?.toDouble() ?? 0,
    );
  }
}
