/// Modelos de dominio del catálogo de precios (preciosarios).
///
/// Mapean los DTOs del backend .NET (camelCase):
///  - `PreciosarioDto { id, nombre, version, fuente }`
///  - `CapituloDto    { id, codigo, titulo, orden }`
///  - `PartidaDto     { id, codigo, resumen, precio }`
///  - `AnalisisPreciosDto { partidaId, codigo, resumen, precio,
///      costeDirecto, lineas[] }`
///  - `DescompuestoDto { id, tipo, rendimiento, precioUnitario, importe }`

class Preciosario {
  const Preciosario({
    required this.id,
    required this.nombre,
    required this.version,
    this.fuente,
  });

  final String id;
  final String nombre;
  final String version;
  final String? fuente;

  factory Preciosario.fromJson(Map<String, dynamic> json) => Preciosario(
        id: json['id'] as String,
        nombre: json['nombre'] as String,
        version: (json['version'] as String?) ?? '',
        fuente: json['fuente'] as String?,
      );
}

class CapituloCatalogo {
  const CapituloCatalogo({
    required this.id,
    required this.codigo,
    required this.titulo,
    required this.orden,
  });

  final String id;
  final String codigo;
  final String titulo;
  final int orden;

  factory CapituloCatalogo.fromJson(Map<String, dynamic> json) =>
      CapituloCatalogo(
        id: json['id'] as String,
        codigo: (json['codigo'] as String?) ?? '',
        titulo: (json['titulo'] as String?) ?? '',
        orden: (json['orden'] as num?)?.toInt() ?? 0,
      );
}

class PartidaCatalogo {
  const PartidaCatalogo({
    required this.id,
    required this.codigo,
    required this.resumen,
    required this.precio,
  });

  final String id;
  final String codigo;
  final String resumen;
  final double precio;

  factory PartidaCatalogo.fromJson(Map<String, dynamic> json) =>
      PartidaCatalogo(
        id: json['id'] as String,
        codigo: (json['codigo'] as String?) ?? '',
        resumen: (json['resumen'] as String?) ?? '',
        precio: (json['precio'] as num?)?.toDouble() ?? 0,
      );
}

class LineaDescompuesto {
  const LineaDescompuesto({
    required this.id,
    required this.tipo,
    required this.rendimiento,
    required this.precioUnitario,
    required this.importe,
  });

  final String id;
  final String tipo;
  final double rendimiento;
  final double precioUnitario;
  final double importe;

  factory LineaDescompuesto.fromJson(Map<String, dynamic> json) =>
      LineaDescompuesto(
        id: json['id'] as String,
        tipo: (json['tipo'] as String?) ?? '',
        rendimiento: (json['rendimiento'] as num?)?.toDouble() ?? 0,
        precioUnitario: (json['precioUnitario'] as num?)?.toDouble() ?? 0,
        importe: (json['importe'] as num?)?.toDouble() ?? 0,
      );
}

/// Resultado de importar un fichero DCF (intercambio FIEBDC) en un preciosario.
///
/// Mapea el DTO del backend .NET (camelCase) del endpoint
/// `POST /api/preciosarios/importar`:
///  - `ImportacionDto { capitulos, partidas, recursos, descompuestos,
///      precios, errores[], advertencias[] }`
class ImportacionResultado {
  const ImportacionResultado({
    required this.capitulos,
    required this.partidas,
    required this.recursos,
    required this.descompuestos,
    required this.precios,
    required this.errores,
    required this.advertencias,
  });

  final int capitulos;
  final int partidas;
  final int recursos;
  final int descompuestos;
  final int precios;
  final List<String> errores;
  final List<String> advertencias;

  factory ImportacionResultado.fromJson(Map<String, dynamic> json) =>
      ImportacionResultado(
        capitulos: (json['capitulos'] as num?)?.toInt() ?? 0,
        partidas: (json['partidas'] as num?)?.toInt() ?? 0,
        recursos: (json['recursos'] as num?)?.toInt() ?? 0,
        descompuestos: (json['descompuestos'] as num?)?.toInt() ?? 0,
        precios: (json['precios'] as num?)?.toInt() ?? 0,
        errores: _textos(json['errores']),
        advertencias: _textos(json['advertencias']),
      );

  static List<String> _textos(dynamic v) =>
      ((v as List<dynamic>?) ?? const [])
          .map((e) => e.toString())
          .toList(growable: false);
}

class AnalisisPrecios {
  const AnalisisPrecios({
    required this.partidaId,
    required this.codigo,
    required this.resumen,
    required this.precio,
    required this.costeDirecto,
    required this.lineas,
  });

  final String partidaId;
  final String codigo;
  final String resumen;
  final double precio;
  final double costeDirecto;
  final List<LineaDescompuesto> lineas;

  factory AnalisisPrecios.fromJson(Map<String, dynamic> json) =>
      AnalisisPrecios(
        partidaId: json['partidaId'] as String,
        codigo: (json['codigo'] as String?) ?? '',
        resumen: (json['resumen'] as String?) ?? '',
        precio: (json['precio'] as num?)?.toDouble() ?? 0,
        costeDirecto: (json['costeDirecto'] as num?)?.toDouble() ?? 0,
        lineas: ((json['lineas'] as List<dynamic>?) ?? const [])
            .map((e) => LineaDescompuesto.fromJson(e as Map<String, dynamic>))
            .toList(growable: false),
      );
}
