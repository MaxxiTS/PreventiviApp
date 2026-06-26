using FluentAssertions;
using PreventiviApp.Domain.Services;
using Xunit;

namespace PreventiviApp.Tests.Unit;

public class ConciliadorPreciosarioTests
{
    private readonly ConciliadorPreciosario _sut = new();

    [Fact]
    public void Clasifica_alta_actualizar_respetar_sincambio_obsoleta() // doc 12 §6
    {
        var existentes = new[]
        {
            new EstadoPartidaCatalogo("A", 20m, Bloqueado: true),  // bloqueada
            new EstadoPartidaCatalogo("B", 30m, Bloqueado: false), // cambia
            new EstadoPartidaCatalogo("C", 40m, Bloqueado: false), // ausente en DCF
            new EstadoPartidaCatalogo("E", 60m, Bloqueado: false), // igual
        };
        var entrantes = new[]
        {
            new PartidaEntranteDcf("A", 25m), // bloqueada -> respetar
            new PartidaEntranteDcf("B", 35m), // -> actualizar
            new PartidaEntranteDcf("D", 50m), // nueva -> alta
            new PartidaEntranteDcf("E", 60m), // -> sin cambio
        };

        var decisiones = _sut.Conciliar(existentes, entrantes);

        Accion(decisiones, "A").Should().Be(AccionConciliacion.Respetar);
        Accion(decisiones, "B").Should().Be(AccionConciliacion.Actualizar);
        Accion(decisiones, "D").Should().Be(AccionConciliacion.Alta);
        Accion(decisiones, "E").Should().Be(AccionConciliacion.SinCambio);
        Accion(decisiones, "C").Should().Be(AccionConciliacion.Obsoleta);

        // El precio de la decisión de actualizar es el del DCF; el de respetar, el actual.
        Precio(decisiones, "B").Should().Be(35m);
        Precio(decisiones, "A").Should().Be(20m);
    }

    [Fact]
    public void Reimportacion_identica_no_produce_cambios()
    {
        var existentes = new[] { new EstadoPartidaCatalogo("01", 10m, false) };
        var entrantes = new[] { new PartidaEntranteDcf("01", 10m) };

        _sut.Conciliar(existentes, entrantes)
            .Should().ContainSingle().Which.Accion.Should().Be(AccionConciliacion.SinCambio);
    }

    private static AccionConciliacion Accion(IReadOnlyList<DecisionConciliacion> d, string codigo)
        => d.Single(x => x.Codigo == codigo).Accion;

    private static decimal Precio(IReadOnlyList<DecisionConciliacion> d, string codigo)
        => d.Single(x => x.Codigo == codigo).PrecioNuevo;
}
