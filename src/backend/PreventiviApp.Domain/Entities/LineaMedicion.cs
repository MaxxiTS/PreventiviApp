using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Línea de medición. Calcula un <c>parcial</c> por vía dimensional
/// (<c>uds × largo × ancho × alto × coeficiente</c>) o por fórmula
/// (docs/12-motor-presupuestos-mediciones.md §3). Puede ser una línea de
/// comentario (parcial = 0).
/// </summary>
public sealed class LineaMedicion : AuditableEntity
{
    public LineaMedicion(Guid id, Guid medicionId, int orden) : base(id)
    {
        MedicionId = medicionId;
        Orden = orden;
    }

    public Guid MedicionId { get; private set; }
    public int Orden { get; set; }

    /// <summary>Texto descriptivo de la línea.</summary>
    public string Comentario { get; set; } = string.Empty;

    /// <summary>Si es true, la línea es solo texto y su parcial es 0.</summary>
    public bool EsComentario { get; set; }

    // Dimensiones (nulas = neutro 1 en el cálculo dimensional).
    public decimal? Uds { get; set; }
    public decimal? Largo { get; set; }
    public decimal? Ancho { get; set; }
    public decimal? Alto { get; set; }

    /// <summary>Coeficiente multiplicador (esponjamientos, mermas, solapes...).</summary>
    public decimal? Coeficiente { get; set; }

    /// <summary>Fórmula aritmética opcional. Si está informada, ignora las dimensiones.</summary>
    public string? Formula { get; set; }

    /// <summary>Variables disponibles para la evaluación de la fórmula.</summary>
    public IReadOnlyDictionary<string, decimal> ContextoVariables() => new Dictionary<string, decimal>
    {
        ["uds"] = Uds ?? 0m,
        ["largo"] = Largo ?? 0m,
        ["ancho"] = Ancho ?? 0m,
        ["alto"] = Alto ?? 0m,
    };
}
