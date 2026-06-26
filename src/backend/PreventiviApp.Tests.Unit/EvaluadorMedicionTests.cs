using FluentAssertions;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;
using Xunit;

namespace PreventiviApp.Tests.Unit;

public class EvaluadorMedicionTests
{
    private readonly EvaluadorMedicion _sut = new(new EvaluadorFormula());

    private static Medicion NuevaMedicion() => new(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Parcial_dimensional() // doc §1.3 línea 1: 1 × 25,00 × 0,60 × 1,20
    {
        var m = NuevaMedicion();
        var l = m.AnadirLinea();
        (l.Uds, l.Largo, l.Ancho, l.Alto) = (1m, 25.00m, 0.60m, 1.20m);

        _sut.CalcularParcial(l).Should().Be(18.000m);
    }

    [Fact]
    public void Dimension_vacia_actua_como_neutro_1()
    {
        var m = NuevaMedicion();
        var soloUds = m.AnadirLinea();
        soloUds.Uds = 4m; // largo/ancho/alto nulos ⇒ 4 × 1 × 1 × 1

        _sut.CalcularParcial(soloUds).Should().Be(4.000m);

        var superficie = m.AnadirLinea();
        (superficie.Largo, superficie.Ancho) = (5.00m, 2.00m); // uds/alto nulos ⇒ 1 × 5 × 2 × 1
        _sut.CalcularParcial(superficie).Should().Be(10.000m);
    }

    [Fact]
    public void Linea_de_comentario_vale_cero()
    {
        var m = NuevaMedicion();
        var l = m.AnadirLinea();
        l.EsComentario = true;
        l.Uds = 99m; // se ignora

        _sut.CalcularParcial(l).Should().Be(0m);
    }

    [Fact]
    public void Coeficiente_multiplica_el_parcial() // 10 × 2,40 × 1,20 × coef 1,05
    {
        var m = NuevaMedicion();
        var l = m.AnadirLinea();
        (l.Uds, l.Largo, l.Ancho, l.Coeficiente) = (10m, 2.40m, 1.20m, 1.05m);

        _sut.CalcularParcial(l).Should().Be(30.240m);
    }

    [Fact]
    public void Parcial_por_formula_ignora_dimensiones() // doc §3.5 línea 3
    {
        var m = NuevaMedicion();
        var l = m.AnadirLinea();
        l.Formula = "16.2 * 0.03";

        _sut.CalcularParcial(l).Should().Be(0.486m);
    }

    [Fact]
    public void Total_medicion_excavacion_doc_1_3() // total esperado 33,000
    {
        var m = NuevaMedicion();
        Linea(m, 1m, 25.00m, 0.60m, 1.20m);
        Linea(m, 1m, 12.50m, 0.60m, 1.20m);
        Linea(m, 4m, 1.00m, 1.00m, 1.50m);

        _sut.CalcularTotal(m).Should().Be(33.000m);
    }

    [Fact]
    public void Total_medicion_hormigon_con_formula_y_coef_doc_3_5() // total esperado 23,886
    {
        var m = NuevaMedicion();
        Linea(m, 12m, 1.50m, 1.50m, 0.60m);
        Linea(m, 1m, 18.00m, 0.80m, 0.50m);
        var formula = m.AnadirLinea();
        formula.Formula = "16.2 * 0.03"; // sobreancho excavación

        _sut.CalcularTotal(m).Should().Be(23.886m);
    }

    private static void Linea(Medicion m, decimal uds, decimal largo, decimal ancho, decimal alto)
    {
        var l = m.AnadirLinea();
        (l.Uds, l.Largo, l.Ancho, l.Alto) = (uds, largo, ancho, alto);
    }
}
