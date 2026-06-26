using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Enums;

namespace PreventiviApp.Domain.Entities;

/// <summary>Proyecto de obra. Agrupa presupuestos y referencia a un cliente.</summary>
public sealed class Proyecto : AuditableEntity
{
    public Proyecto(Guid id, string nombre, Guid? clienteId = null) : base(id)
    {
        Nombre = nombre;
        ClienteId = clienteId;
        Estado = EstadoProyecto.Borrador;
    }

    public string Nombre { get; set; }
    public Guid? ClienteId { get; set; }
    public string? Direccion { get; set; }
    public DateOnly? Fecha { get; set; }
    public EstadoProyecto Estado { get; private set; }

    /// <summary>Transición de estado válida (borrador → activo → cerrado → archivado).</summary>
    public Result CambiarEstado(EstadoProyecto nuevo)
    {
        bool valido = (Estado, nuevo) switch
        {
            (EstadoProyecto.Borrador, EstadoProyecto.Activo) => true,
            (EstadoProyecto.Activo, EstadoProyecto.Cerrado) => true,
            (EstadoProyecto.Cerrado, EstadoProyecto.Archivado) => true,
            (EstadoProyecto.Cerrado, EstadoProyecto.Activo) => true, // reabrir
            (_, EstadoProyecto.Archivado) => true,                   // archivar siempre permitido
            _ => false,
        };

        if (!valido)
            return Result.Fallo(Error.Conflicto($"Transición de estado no permitida: {Estado} → {nuevo}."));

        Estado = nuevo;
        ActualizadoEn = DateTimeOffset.UtcNow;
        return Result.Ok();
    }
}
