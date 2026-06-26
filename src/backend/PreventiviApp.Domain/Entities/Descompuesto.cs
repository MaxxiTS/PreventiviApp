using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Enums;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Línea del análisis de precios (descompuesto) de una <see cref="Partida"/>:
/// referencia un <see cref="Recurso"/> con un rendimiento y un precio unitario.
/// <c>importe_línea = rendimiento × precio_unitario</c>.
/// </summary>
public sealed class Descompuesto : AuditableEntity
{
    public Descompuesto(
        Guid id,
        Guid partidaId,
        Guid recursoId,
        TipoRecurso tipo,
        decimal rendimiento,
        decimal precioUnitario) : base(id)
    {
        PartidaId = partidaId;
        RecursoId = recursoId;
        Tipo = tipo;
        Rendimiento = rendimiento;
        PrecioUnitario = precioUnitario;
    }

    public Guid PartidaId { get; private set; }
    public Guid RecursoId { get; private set; }

    /// <summary>Naturaleza del recurso (para agrupar mano de obra/material/maquinaria).</summary>
    public TipoRecurso Tipo { get; set; }

    /// <summary>Cantidad de recurso por unidad de partida.</summary>
    public decimal Rendimiento { get; set; }

    /// <summary>Precio unitario del recurso.</summary>
    public decimal PrecioUnitario { get; set; }

    public decimal Importe => Rendimiento * PrecioUnitario;
}
