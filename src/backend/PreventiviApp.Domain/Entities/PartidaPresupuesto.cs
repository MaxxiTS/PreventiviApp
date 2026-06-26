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
    public decimal Precio { get; private set; }

    /// <summary>Si está bloqueado, no se sobrescribe al actualizar precios desde el preciosario.</summary>
    public bool PrecioBloqueado { get; private set; }

    public Medicion Medicion { get; private set; } = null!;

    /// <summary>Edición manual del precio por el usuario; permitida incluso si está bloqueado.</summary>
    public void EditarPrecio(decimal nuevoPrecio)
    {
        Precio = nuevoPrecio;
        Tocar();
    }

    /// <summary>Bloquea el precio para protegerlo de actualizaciones automáticas (doc 12 §5.3).</summary>
    public void BloquearPrecio()
    {
        if (PrecioBloqueado) return;
        PrecioBloqueado = true;
        Tocar();
    }

    public void DesbloquearPrecio()
    {
        if (!PrecioBloqueado) return;
        PrecioBloqueado = false;
        Tocar();
    }

    /// <summary>
    /// Aplica un precio procedente del preciosario respetando el bloqueo. Devuelve
    /// <c>true</c> solo si el precio cambia (no bloqueado y distinto del actual).
    /// </summary>
    public bool ActualizarPrecioDesdePreciosario(decimal precioPreciosario)
    {
        if (PrecioBloqueado || Precio == precioPreciosario)
            return false;

        Precio = precioPreciosario;
        Tocar();
        return true;
    }

    private void Tocar() => ActualizadoEn = DateTimeOffset.UtcNow;
}
