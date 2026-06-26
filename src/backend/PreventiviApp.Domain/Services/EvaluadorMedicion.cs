using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Domain.Services;

/// <summary>
/// Calcula el parcial de una línea (dimensional o por fórmula) y el total de una
/// medición (docs/12-motor-presupuestos-mediciones.md §9.1).
/// </summary>
public sealed class EvaluadorMedicion(IEvaluadorFormula formula)
{
    private readonly IEvaluadorFormula _formula = formula;

    /// <summary>
    /// Parcial de una línea: por fórmula si está informada, si no por dimensiones
    /// (<c>uds × largo × ancho × alto</c>, donde una dimensión vacía actúa como 1),
    /// multiplicado por el coeficiente. Las líneas de comentario valen 0.
    /// </summary>
    public decimal CalcularParcial(LineaMedicion linea)
    {
        if (linea.EsComentario)
            return 0m;

        decimal baseCalculo = !string.IsNullOrWhiteSpace(linea.Formula)
            ? _formula.Evaluar(linea.Formula, linea.ContextoVariables())
            : Neutro(linea.Uds) * Neutro(linea.Largo) * Neutro(linea.Ancho) * Neutro(linea.Alto);

        var coef = linea.Coeficiente ?? 1m;
        return Redondeo.Medicion(baseCalculo * coef);
    }

    /// <summary>Total de la medición = suma de los parciales de sus líneas.</summary>
    public decimal CalcularTotal(Medicion medicion)
        => Redondeo.Medicion(medicion.Lineas.Sum(CalcularParcial));

    /// <summary>Una dimensión vacía/nula actúa como elemento neutro (1).</summary>
    private static decimal Neutro(decimal? dim) => dim is null or 0m ? 1m : dim.Value;
}
