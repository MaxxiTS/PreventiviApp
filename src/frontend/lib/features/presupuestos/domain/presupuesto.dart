/// Modelos de dominio de presupuestos.
///
/// Mapean los DTOs del backend .NET (camelCase):
///  - `PresupuestoResumenDto { id, nombre, version, estado, total }`
///  - `PresupuestoDetalleDto { id, nombre, version, pem, pemAjustado,
///      baseImponible, cuotaIva, total, capitulos[] }`
///  - `CapituloDetalleDto   { id, codigo, titulo, subtotal, partidas[],
///      subcapitulos[] }`
///  - `PartidaDetalleDto    { id, codigo, resumen, precio, medicionTotal,
///      importe }`

/// Resumen de presupuesto para listados.
class PresupuestoResumen {
  const PresupuestoResumen({
    required this.id,
    required this.nombre,
    required this.version,
    required this.estado,
    required this.total,
  });

  final String id;
  final String nombre;
  final int version;
  final String estado;
  final double total;

  factory PresupuestoResumen.fromJson(Map<String, dynamic> json) {
    return PresupuestoResumen(
      id: json['id'] as String,
      nombre: json['nombre'] as String,
      version: (json['version'] as num).toInt(),
      estado: (json['estado'] as String?) ?? 'Borrador',
      total: (json['total'] as num?)?.toDouble() ?? 0,
    );
  }
}

/// Detalle de presupuesto con árbol de capítulos y cierre económico.
class PresupuestoDetalle {
  const PresupuestoDetalle({
    required this.id,
    required this.nombre,
    required this.version,
    required this.pem,
    required this.pemAjustado,
    required this.baseImponible,
    required this.cuotaIva,
    required this.total,
    required this.capitulos,
  });

  final String id;
  final String nombre;
  final int version;

  /// Presupuesto de Ejecución Material.
  final double pem;

  /// PEM tras baja/descuentos.
  final double pemAjustado;

  final double baseImponible;
  final double cuotaIva;
  final double total;
  final List<CapituloDetalle> capitulos;

  factory PresupuestoDetalle.fromJson(Map<String, dynamic> json) {
    return PresupuestoDetalle(
      id: json['id'] as String,
      nombre: json['nombre'] as String,
      version: (json['version'] as num).toInt(),
      pem: (json['pem'] as num?)?.toDouble() ?? 0,
      pemAjustado: (json['pemAjustado'] as num?)?.toDouble() ?? 0,
      baseImponible: (json['baseImponible'] as num?)?.toDouble() ?? 0,
      cuotaIva: (json['cuotaIva'] as num?)?.toDouble() ?? 0,
      total: (json['total'] as num?)?.toDouble() ?? 0,
      capitulos: ((json['capitulos'] as List<dynamic>?) ?? const [])
          .map((e) => CapituloDetalle.fromJson(e as Map<String, dynamic>))
          .toList(growable: false),
    );
  }
}

/// Capítulo del árbol de presupuesto (recursivo en `subcapitulos`).
class CapituloDetalle {
  const CapituloDetalle({
    required this.id,
    required this.codigo,
    required this.titulo,
    required this.subtotal,
    required this.partidas,
    required this.subcapitulos,
  });

  final String id;
  final String codigo;
  final String titulo;
  final double subtotal;
  final List<PartidaDetalle> partidas;
  final List<CapituloDetalle> subcapitulos;

  factory CapituloDetalle.fromJson(Map<String, dynamic> json) {
    return CapituloDetalle(
      id: json['id'] as String,
      codigo: (json['codigo'] as String?) ?? '',
      titulo: (json['titulo'] as String?) ?? '',
      subtotal: (json['subtotal'] as num?)?.toDouble() ?? 0,
      partidas: ((json['partidas'] as List<dynamic>?) ?? const [])
          .map((e) => PartidaDetalle.fromJson(e as Map<String, dynamic>))
          .toList(growable: false),
      subcapitulos: ((json['subcapitulos'] as List<dynamic>?) ?? const [])
          .map((e) => CapituloDetalle.fromJson(e as Map<String, dynamic>))
          .toList(growable: false),
    );
  }
}

/// Partida dentro de un capítulo de presupuesto.
class PartidaDetalle {
  const PartidaDetalle({
    required this.id,
    required this.codigo,
    required this.resumen,
    required this.precio,
    required this.medicionTotal,
    required this.importe,
  });

  final String id;
  final String codigo;
  final String resumen;
  final double precio;
  final double medicionTotal;
  final double importe;

  factory PartidaDetalle.fromJson(Map<String, dynamic> json) {
    return PartidaDetalle(
      id: json['id'] as String,
      codigo: (json['codigo'] as String?) ?? '',
      resumen: (json['resumen'] as String?) ?? '',
      precio: (json['precio'] as num?)?.toDouble() ?? 0,
      medicionTotal: (json['medicionTotal'] as num?)?.toDouble() ?? 0,
      importe: (json['importe'] as num?)?.toDouble() ?? 0,
    );
  }
}
