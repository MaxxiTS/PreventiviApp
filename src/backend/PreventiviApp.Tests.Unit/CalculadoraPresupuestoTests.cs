using FluentAssertions;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Enums;
using PreventiviApp.Domain.Services;
using Xunit;

namespace PreventiviApp.Tests.Unit;

public class CalculadoraPresupuestoTests
{
    private readonly CalculadoraPresupuesto _sut = new(new EvaluadorMedicion(new EvaluadorFormula()));

    [Fact]
    public void CalcularPrecioPartida_desde_descompuesto_doc_2_2() // precio esperado 37,64
    {
        var partidaId = Guid.NewGuid();
        var descompuesto = new List<Descompuesto>
        {
            Linea(partidaId, TipoRecurso.ManoObra,   0.650m, 19.80m), // 12,8700
            Linea(partidaId, TipoRecurso.ManoObra,   0.650m, 17.20m), // 11,1800
            Linea(partidaId, TipoRecurso.Material,   56.000m, 0.18m), // 10,0800
            Linea(partidaId, TipoRecurso.Material,   0.030m, 78.50m), //  2,3550
            Linea(partidaId, TipoRecurso.Maquinaria, 0.030m, 1.95m),  //  0,0585
        };

        // coste directo = 36,5435 ; × (1 + 3/100) = 37,63981 ⇒ 37,64
        _sut.CalcularPrecioPartida(descompuesto, indirectosPct: 3m).Should().Be(37.64m);
    }

    [Fact]
    public void CalcularImportePartida_excavacion_doc_1_3() // 33,000 × 18,45 = 608,85
    {
        var partida = new PartidaPresupuesto(Guid.NewGuid(), Guid.NewGuid(), "01.01.001", "Excavación", 18.45m);
        Linea(partida, 1m, 25.00m, 0.60m, 1.20m);
        Linea(partida, 1m, 12.50m, 0.60m, 1.20m);
        Linea(partida, 4m, 1.00m, 1.00m, 1.50m);

        _sut.CalcularImportePartida(partida).Should().Be(608.85m);
    }

    [Fact]
    public void CalcularImportePartida_hormigon_doc_3_5() // 23,886 × 92,30 = 2.204,68
    {
        var partida = new PartidaPresupuesto(Guid.NewGuid(), Guid.NewGuid(), "03.001", "Hormigón zapatas", 92.30m);
        Linea(partida, 12m, 1.50m, 1.50m, 0.60m);
        Linea(partida, 1m, 18.00m, 0.80m, 0.50m);
        partida.Medicion.AnadirLinea().Formula = "16.2 * 0.03";

        _sut.CalcularImportePartida(partida).Should().Be(2204.68m);
    }

    [Fact]
    public void CalcularSubtotalCapitulo_suma_partidas_y_subcapitulos()
    {
        var presupuestoId = Guid.NewGuid();
        var raiz = new CapituloPresupuesto(Guid.NewGuid(), presupuestoId, "01", "Capítulo", 1);
        raiz.AnadirPartida(PartidaConImporte(raiz.Id, precio: 10m, total: 2m)); // 20,00
        var sub = new CapituloPresupuesto(Guid.NewGuid(), presupuestoId, "01.01", "Subcapítulo", 1, raiz.Id);
        sub.AnadirPartida(PartidaConImporte(sub.Id, precio: 5m, total: 3m));     // 15,00
        raiz.AnadirSubcapitulo(sub);

        _sut.CalcularSubtotalCapitulo(raiz).Should().Be(35.00m);
    }

    [Fact]
    public void Subtotal_usa_cache_y_se_recalcula_tras_marcar_sucio()
    {
        var capitulo = new CapituloPresupuesto(Guid.NewGuid(), Guid.NewGuid(), "02", "Capítulo", 1);
        capitulo.AnadirPartida(PartidaConImporte(capitulo.Id, precio: 100m, total: 1m)); // 100,00

        _sut.CalcularSubtotalCapitulo(capitulo).Should().Be(100.00m);
        capitulo.Dirty.Should().BeFalse();
        capitulo.SubtotalCache.Should().Be(100.00m);

        // Al añadir otra partida el capítulo se marca sucio y recalcula.
        capitulo.AnadirPartida(PartidaConImporte(capitulo.Id, precio: 50m, total: 1m)); // +50,00
        capitulo.Dirty.Should().BeTrue();
        _sut.CalcularSubtotalCapitulo(capitulo).Should().Be(150.00m);
    }

    [Fact]
    public void CalcularPresupuesto_cierre_economico_doc_4_5()
    {
        var presupuesto = new Presupuesto(Guid.NewGuid(), Guid.NewGuid(), "Obra", new Iva(Guid.NewGuid(), "General", 21m))
        {
            BajaPct = 12m,
            DescuentosImporte = 1760m,
        };
        var capitulo = new CapituloPresupuesto(Guid.NewGuid(), presupuesto.Id, "01", "Capítulo", 1);
        capitulo.AnadirPartida(PartidaConImporte(capitulo.Id, precio: 100000m, total: 1m)); // PEM = 100.000,00
        presupuesto.AnadirCapituloRaiz(capitulo);

        var r = _sut.CalcularPresupuesto(presupuesto);

        r.Pem.Should().Be(100000.00m);
        r.PemAjustado.Should().Be(88000.00m);    // × 0,88
        r.BaseImponible.Should().Be(86240.00m);  // − 1.760
        r.CuotaIva.Should().Be(18110.40m);       // 21 %
        r.Total.Should().Be(104350.40m);
    }

    // --- helpers ---

    private static Descompuesto Linea(Guid partidaId, TipoRecurso tipo, decimal rendimiento, decimal precioUnitario)
        => new(Guid.NewGuid(), partidaId, Guid.NewGuid(), tipo, rendimiento, precioUnitario);

    private static void Linea(PartidaPresupuesto partida, decimal uds, decimal largo, decimal ancho, decimal alto)
    {
        var l = partida.Medicion.AnadirLinea();
        (l.Uds, l.Largo, l.Ancho, l.Alto) = (uds, largo, ancho, alto);
    }

    private static PartidaPresupuesto PartidaConImporte(Guid capituloId, decimal precio, decimal total)
    {
        var partida = new PartidaPresupuesto(Guid.NewGuid(), capituloId, "P", "Partida", precio);
        partida.Medicion.AnadirLinea().Uds = total; // parcial = total × 1 × 1 × 1
        return partida;
    }
}
