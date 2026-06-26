using FluentAssertions;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;
using Xunit;

namespace PreventiviApp.Tests.Unit;

public class GestionPreciosTests
{
    private static PartidaPresupuesto Partida(string codigo, decimal precio)
        => new(Guid.NewGuid(), Guid.NewGuid(), codigo, "Partida", precio);

    [Fact]
    public void EditarPrecio_cambia_el_precio()
    {
        var p = Partida("01", 10m);
        p.EditarPrecio(15m);
        p.Precio.Should().Be(15m);
    }

    [Fact]
    public void Precio_bloqueado_no_se_actualiza_desde_preciosario_pero_si_a_mano()
    {
        var p = Partida("01", 15m);
        p.BloquearPrecio();
        p.PrecioBloqueado.Should().BeTrue();

        p.ActualizarPrecioDesdePreciosario(20m).Should().BeFalse();
        p.Precio.Should().Be(15m); // respetado

        p.EditarPrecio(25m); // la edición manual siempre se permite
        p.Precio.Should().Be(25m);
    }

    [Fact]
    public void Tras_desbloquear_se_actualiza_desde_preciosario()
    {
        var p = Partida("01", 15m);
        p.BloquearPrecio();
        p.DesbloquearPrecio();

        p.ActualizarPrecioDesdePreciosario(30m).Should().BeTrue();
        p.Precio.Should().Be(30m);
    }

    [Fact]
    public void Actualizar_con_mismo_precio_no_cambia()
    {
        var p = Partida("01", 30m);
        p.ActualizarPrecioDesdePreciosario(30m).Should().BeFalse();
    }

    [Fact]
    public void Actualizacion_masiva_clasifica_correctamente() // doc 12 §5–§6
    {
        var actualizada = Partida("01", 10m);                 // 10 -> 12
        var respetada = Partida("02", 20m);                   // bloqueada, 20 (ignora 25)
        respetada.BloquearPrecio();
        var sinCambio = Partida("03", 30m);                   // 30 -> 30
        var huerfana = Partida("99", 5m);                     // sin código en preciosario

        var precios = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["01"] = 12m,
            ["02"] = 25m,
            ["03"] = 30m,
        };

        var sut = new ActualizadorPreciosPresupuesto();
        var resultado = sut.Actualizar([actualizada, respetada, sinCambio, huerfana], precios);

        resultado.Actualizadas.Should().Be(1);
        resultado.Respetadas.Should().Be(1);
        resultado.SinCambio.Should().Be(1);
        resultado.SinCorrespondencia.Should().Be(1);

        actualizada.Precio.Should().Be(12m);
        respetada.Precio.Should().Be(20m);
    }
}
