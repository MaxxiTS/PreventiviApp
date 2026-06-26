/// Modelo de dominio de un Proyecto de obra.
///
/// Mapea el `ProyectoDto` del backend .NET (serializado en camelCase):
/// `{ id, nombre, clienteId, direccion, fecha, estado }`.
class Proyecto {
  const Proyecto({
    required this.id,
    required this.nombre,
    this.clienteId,
    this.direccion,
    this.fecha,
    required this.estado,
  });

  final String id;
  final String nombre;
  final String? clienteId;
  final String? direccion;

  /// Fecha en formato ISO (`yyyy-MM-dd`) tal cual la devuelve la API.
  final String? fecha;

  /// Estado del proyecto (`Borrador`, `Activo`, `Cerrado`, `Archivado`, ...).
  final String estado;

  factory Proyecto.fromJson(Map<String, dynamic> json) {
    return Proyecto(
      id: json['id'] as String,
      nombre: json['nombre'] as String,
      clienteId: json['clienteId'] as String?,
      direccion: json['direccion'] as String?,
      fecha: json['fecha'] as String?,
      estado: (json['estado'] as String?) ?? 'Borrador',
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'nombre': nombre,
        'clienteId': clienteId,
        'direccion': direccion,
        'fecha': fecha,
        'estado': estado,
      };
}
