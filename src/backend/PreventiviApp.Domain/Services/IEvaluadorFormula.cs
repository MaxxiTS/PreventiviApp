namespace PreventiviApp.Domain.Services;

/// <summary>
/// Evalúa fórmulas aritméticas de líneas de medición. Soporta operadores
/// <c>+ - * /</c>, paréntesis, variables (<c>uds, largo, ancho, alto</c>), la
/// constante <c>PI</c> y funciones (<c>sqrt, pow, abs, round, min, max, sin, cos</c>).
/// Ver docs/12-motor-presupuestos-mediciones.md §3.4.
/// </summary>
public interface IEvaluadorFormula
{
    /// <summary>
    /// Evalúa <paramref name="formula"/> con las variables de <paramref name="variables"/>.
    /// Lanza <see cref="FormatException"/> si la fórmula es sintácticamente inválida.
    /// </summary>
    decimal Evaluar(string formula, IReadOnlyDictionary<string, decimal> variables);
}
