using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Enums;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Presupuesto: árbol de capítulos/partidas con sus mediciones. Versionable.
/// El cierre económico aplica baja de adjudicación, descuentos e IVA sobre el PEM
/// (docs/12-motor-presupuestos-mediciones.md §4).
/// </summary>
public sealed class Presupuesto : AuditableEntity
{
    private readonly List<CapituloPresupuesto> _capitulosRaiz = [];

    public Presupuesto(
        Guid id,
        Guid proyectoId,
        string nombre,
        Iva iva,
        int version = 1) : base(id)
    {
        ProyectoId = proyectoId;
        Nombre = nombre;
        Iva = iva;
        NumeroVersion = version;
        Estado = EstadoPresupuesto.Borrador;
    }

    public Guid ProyectoId { get; private set; }
    public string Nombre { get; set; }
    public int NumeroVersion { get; private set; }
    public EstadoPresupuesto Estado { get; set; }

    /// <summary>Porcentaje de costes indirectos aplicado al coste directo de las partidas.</summary>
    public decimal CostesIndirectosPct { get; set; }

    /// <summary>Baja de adjudicación en % (descuento ofertado en licitación).</summary>
    public decimal BajaPct { get; set; }

    /// <summary>Descuento comercial en importe absoluto, posterior a la baja.</summary>
    public decimal DescuentosImporte { get; set; }

    public Iva Iva { get; set; }

    public IReadOnlyList<CapituloPresupuesto> CapitulosRaiz => _capitulosRaiz;

    public void AnadirCapituloRaiz(CapituloPresupuesto capitulo) => _capitulosRaiz.Add(capitulo);
}
