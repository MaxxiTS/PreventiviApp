using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Domain.Services;

/// <summary>
/// Motor de cálculo ascendente (bottom-up) con redondeo por nivel y recálculo
/// incremental por dirty tracking (docs/12-motor-presupuestos-mediciones.md §9.2).
/// </summary>
public sealed class CalculadoraPresupuesto(EvaluadorMedicion mediciones)
{
    private readonly EvaluadorMedicion _mediciones = mediciones;

    /// <summary>
    /// Precio de una partida a partir de su descompuesto + costes indirectos:
    /// <c>precio = redondear(coste_directo × (1 + indirectos%/100))</c>.
    /// </summary>
    public decimal CalcularPrecioPartida(IReadOnlyList<Descompuesto> descompuesto, decimal indirectosPct)
    {
        var costeDirecto = descompuesto.Sum(d => d.Rendimiento * d.PrecioUnitario);
        var precio = costeDirecto * (1m + indirectosPct / 100m);
        return Redondeo.Precio(precio);
    }

    /// <summary>Importe de una partida = total de medición × precio.</summary>
    public decimal CalcularImportePartida(PartidaPresupuesto partida)
    {
        var totalMedicion = _mediciones.CalcularTotal(partida.Medicion);
        return Redondeo.Importe(totalMedicion * partida.Precio);
    }

    /// <summary>Subtotal de un capítulo (recursivo sobre subcapítulos y partidas).</summary>
    public decimal CalcularSubtotalCapitulo(CapituloPresupuesto capitulo)
    {
        if (!capitulo.Dirty && capitulo.SubtotalCache is { } cache)
            return cache;

        var importePartidas = capitulo.Partidas.Sum(CalcularImportePartida);
        var importeSubcapitulos = capitulo.Subcapitulos.Sum(CalcularSubtotalCapitulo);

        var subtotal = Redondeo.Importe(importePartidas + importeSubcapitulos);
        capitulo.MarcarLimpio(subtotal);
        return subtotal;
    }

    /// <summary>Cierre del presupuesto: PEM → baja de adjudicación → descuentos → IVA → total.</summary>
    public ResultadoPresupuesto CalcularPresupuesto(Presupuesto p)
    {
        var pem = Redondeo.Importe(p.CapitulosRaiz.Sum(CalcularSubtotalCapitulo));

        var coefAdjudicacion = 1m - p.BajaPct / 100m;
        var pemAjustado = Redondeo.Importe(pem * coefAdjudicacion);

        var baseImponible = Redondeo.Importe(pemAjustado - p.DescuentosImporte);
        var cuotaIva = Redondeo.Importe(baseImponible * p.Iva.Porcentaje / 100m);
        var total = Redondeo.Importe(baseImponible + cuotaIva);

        return new ResultadoPresupuesto(pem, pemAjustado, baseImponible, cuotaIva, total);
    }
}
