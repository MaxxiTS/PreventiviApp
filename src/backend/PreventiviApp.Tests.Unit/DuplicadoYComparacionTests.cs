using FluentAssertions;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;
using Xunit;

namespace PreventiviApp.Tests.Unit;

public class DuplicadoYComparacionTests
{
    private static PartidaPresupuesto PartidaCon(Guid capituloId, string codigo, decimal precio, decimal uds)
    {
        var p = new PartidaPresupuesto(Guid.NewGuid(), capituloId, codigo, "Partida", precio);
        p.Medicion.AnadirLinea().Uds = uds;
        return p;
    }

    [Fact]
    public void Duplicar_capitulo_es_copia_profunda_con_nuevos_ids()
    {
        var cap = new CapituloPresupuesto(Guid.NewGuid(), Guid.NewGuid(), "01", "Capítulo", 1);
        var partida = PartidaCon(cap.Id, "01.001", 10m, uds: 2m);
        partida.BloquearPrecio();
        cap.AnadirPartida(partida);

        var copia = cap.Duplicar(nuevoPadreId: null);

        copia.Id.Should().NotBe(cap.Id);
        copia.Partidas.Should().ContainSingle();

        var partidaCopia = copia.Partidas[0];
        partidaCopia.Id.Should().NotBe(partida.Id);
        partidaCopia.CapituloPresupuestoId.Should().Be(copia.Id);
        partidaCopia.Codigo.Should().Be("01.001");
        partidaCopia.Precio.Should().Be(10m);
        partidaCopia.PrecioBloqueado.Should().BeTrue(); // se preserva el bloqueo
        partidaCopia.Medicion.Lineas.Should().ContainSingle();
        partidaCopia.Medicion.Lineas[0].Uds.Should().Be(2m);
        partidaCopia.Medicion.Id.Should().NotBe(partida.Medicion.Id);
    }

    [Fact]
    public void CrearNuevaVersion_incrementa_version_y_clona_el_arbol()
    {
        var presupuesto = new Presupuesto(Guid.NewGuid(), Guid.NewGuid(), "Obra", new Iva(Guid.NewGuid(), "IVA", 21m))
        {
            BajaPct = 10m,
        };
        var cap = new CapituloPresupuesto(Guid.NewGuid(), presupuesto.Id, "01", "Capítulo", 1);
        cap.AnadirPartida(PartidaCon(cap.Id, "01.001", 100m, uds: 1m));
        presupuesto.AnadirCapituloRaiz(cap);

        var v2 = presupuesto.CrearNuevaVersion();

        v2.Id.Should().NotBe(presupuesto.Id);
        v2.NumeroVersion.Should().Be(presupuesto.NumeroVersion + 1);
        v2.BajaPct.Should().Be(10m);
        v2.CapitulosRaiz.Should().ContainSingle();
        v2.CapitulosRaiz[0].Id.Should().NotBe(cap.Id);
        v2.CapitulosRaiz[0].Partidas[0].Codigo.Should().Be("01.001");
    }

    [Fact]
    public void Comparar_clasifica_y_calcula_delta_total() // doc 12 §7.3
    {
        var a = new[]
        {
            new PartidaSnapshot("01", 18.45m, 33m, 608.85m),
            new PartidaSnapshot("02", 37.64m, 120m, 4516.80m),
            new PartidaSnapshot("05", 12.10m, 200m, 2420.00m),
        };
        var b = new[]
        {
            new PartidaSnapshot("01", 18.45m, 33m, 608.85m),  // igual
            new PartidaSnapshot("02", 39.10m, 120m, 4692.00m),// precio ↑
            new PartidaSnapshot("04", 54.20m, 80m, 4336.00m), // añadida
        };

        var resultado = new ComparadorPresupuestos().Comparar(a, b);

        Estado(resultado, "01").Should().Be(EstadoComparacion.Igual);
        Estado(resultado, "02").Should().Be(EstadoComparacion.PrecioModificado);
        Estado(resultado, "04").Should().Be(EstadoComparacion.Anadida);
        Estado(resultado, "05").Should().Be(EstadoComparacion.Eliminada);

        // 175,20 (02) + 4.336,00 (04) − 2.420,00 (05) = 2.091,20
        resultado.DeltaTotal.Should().Be(2091.20m);
    }

    private static EstadoComparacion Estado(ResultadoComparacion r, string codigo)
        => r.Lineas.Single(x => x.Codigo == codigo).Estado;
}
