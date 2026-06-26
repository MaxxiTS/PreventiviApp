using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Partida insertada en un presupuesto. Su <see cref="Precio"/> es independiente
/// del preciosario (puede divergir) y tiene una <see cref="Medicion"/> asociada.
/// El importe = total de medición × precio.
/// </summary>
public sealed class PartidaPresupuesto : AuditableEntity
{
    public PartidaPresupuesto(
        Guid id,
        Guid capituloPresupuestoId,
        string codigo,
        string resumen,
        decimal precio,
        Guid? partidaOrigenId = null) : base(id)
    {
        CapituloPresupuestoId = capituloPresupuestoId;
        Codigo = codigo;
        Resumen = resumen;
        Precio = precio;
        PartidaOrigenId = partidaOrigenId;
        Medicion = new Medicion(Guid.NewGuid(), id);
    }

    private PartidaPresupuesto() { } // EF Core

    public Guid CapituloPresupuestoId { get; private set; }

    /// <summary>Referencia opcional a la <see cref="Partida"/> del preciosario de origen.</summary>
    public Guid? PartidaOrigenId { get; private set; }

    public string Codigo { get; set; } = string.Empty;
    public string Resumen { get; set; } = string.Empty;

    /// <summary>Precio unitario del presupuesto (independiente del preciosario).</summary>
    public decimal Precio { get; set; }

    /// <summary>Si está bloqueado, no se sobrescribe al actualizar precios desde DCF.</summary>
    public bool PrecioBloqueado { get; set; }

    public Medicion Medicion { get; private set; } = null!;
}
