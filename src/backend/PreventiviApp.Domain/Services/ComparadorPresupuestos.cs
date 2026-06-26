namespace PreventiviApp.Domain.Services;

/// <summary>Resultado de comparar una partida entre dos versiones de presupuesto.</summary>
public enum EstadoComparacion
{
    Igual,
    PrecioModificado,
    MedicionModificada,
    Anadida,
    Eliminada,
}

/// <summary>Foto de una partida para comparar versiones (código, precio, medición e importe).</summary>
public readonly record struct PartidaSnapshot(string Codigo, decimal Precio, decimal Medicion, decimal Importe);

/// <summary>Diferencia de una partida entre la versión A y la B.</summary>
public sealed record LineaComparacion(
    string Codigo,
    EstadoComparacion Estado,
    decimal? PrecioA, decimal? PrecioB,
    decimal? MedicionA, decimal? MedicionB,
    decimal? ImporteA, decimal? ImporteB,
    decimal DeltaImporte);

/// <summary>Comparación completa entre dos versiones, con el delta total de importe.</summary>
public sealed record ResultadoComparacion(IReadOnlyList<LineaComparacion> Lineas, decimal DeltaTotal);

/// <summary>
/// Compara dos versiones de presupuesto por código de partida y clasifica cada una
/// como añadida, eliminada, con precio o medición modificados, o igual (doc 12 §7.2).
/// </summary>
public sealed class ComparadorPresupuestos
{
    public ResultadoComparacion Comparar(IEnumerable<PartidaSnapshot> versionA, IEnumerable<PartidaSnapshot> versionB)
    {
        var porCodigoA = new Dictionary<string, PartidaSnapshot>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in versionA)
            porCodigoA[p.Codigo] = p;

        var porCodigoB = new Dictionary<string, PartidaSnapshot>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in versionB)
            porCodigoB[p.Codigo] = p;

        var codigos = porCodigoA.Keys.Union(porCodigoB.Keys, StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase);

        var lineas = new List<LineaComparacion>();
        decimal deltaTotal = 0m;

        foreach (var codigo in codigos)
        {
            var enA = porCodigoA.TryGetValue(codigo, out var a);
            var enB = porCodigoB.TryGetValue(codigo, out var b);

            LineaComparacion linea;
            if (enA && !enB)
            {
                linea = new LineaComparacion(codigo, EstadoComparacion.Eliminada,
                    a.Precio, null, a.Medicion, null, a.Importe, null, -a.Importe);
            }
            else if (!enA && enB)
            {
                linea = new LineaComparacion(codigo, EstadoComparacion.Anadida,
                    null, b.Precio, null, b.Medicion, null, b.Importe, b.Importe);
            }
            else
            {
                var estado = a.Precio != b.Precio
                    ? EstadoComparacion.PrecioModificado
                    : a.Medicion != b.Medicion
                        ? EstadoComparacion.MedicionModificada
                        : EstadoComparacion.Igual;

                linea = new LineaComparacion(codigo, estado,
                    a.Precio, b.Precio, a.Medicion, b.Medicion, a.Importe, b.Importe, b.Importe - a.Importe);
            }

            deltaTotal += linea.DeltaImporte;
            lineas.Add(linea);
        }

        return new ResultadoComparacion(lineas, deltaTotal);
    }
}
