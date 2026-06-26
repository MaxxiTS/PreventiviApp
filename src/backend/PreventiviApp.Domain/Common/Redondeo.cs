namespace PreventiviApp.Domain.Common;

/// <summary>
/// Reglas de redondeo del motor de cálculo (docs/12-motor-presupuestos-mediciones.md §2.3).
/// Todo el cálculo monetario usa <see cref="decimal"/>; el redondeo solo ocurre al
/// cruzar fronteras de nivel jerárquico o de presentación, con criterio
/// «por mitades al alza» (<see cref="MidpointRounding.AwayFromZero"/>), coherente
/// con la práctica de mediciones española.
/// </summary>
public static class Redondeo
{
    /// <summary>Precio de partida y subtotales/importes: 2 decimales.</summary>
    public static decimal Precio(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    /// <summary>Parcial y total de medición: 3 decimales.</summary>
    public static decimal Medicion(decimal v) => Math.Round(v, 3, MidpointRounding.AwayFromZero);

    /// <summary>Importes y totales económicos: 2 decimales.</summary>
    public static decimal Importe(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
