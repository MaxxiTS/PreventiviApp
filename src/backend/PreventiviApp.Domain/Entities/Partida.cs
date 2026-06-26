using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Partida (unidad de obra) del preciosario: código, resumen, texto, unidad y
/// precio. Su análisis de precios se modela con <see cref="Descompuesto"/>.
/// </summary>
public sealed class Partida : AuditableEntity
{
    public Partida(
        Guid id,
        Guid capituloId,
        string codigo,
        string resumen,
        decimal precio,
        Guid? unidadId = null) : base(id)
    {
        CapituloId = capituloId;
        Codigo = codigo;
        Resumen = resumen;
        Precio = precio;
        UnidadId = unidadId;
    }

    public Guid CapituloId { get; set; }
    public string Codigo { get; set; }
    public string Resumen { get; set; }
    public string? TextoLargo { get; set; }
    public decimal Precio { get; set; }
    public Guid? UnidadId { get; set; }

    /// <summary>Marca de partida sin correspondencia en la última versión del DCF.</summary>
    public bool Obsoleta { get; set; }
}
