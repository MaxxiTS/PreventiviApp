using FluentAssertions;
using PreventiviApp.Application.Presupuestos;
using PreventiviApp.Domain.Services;
using Xunit;

namespace PreventiviApp.Tests.Unit;

public class CalcularPresupuestoHandlerTests
{
    private readonly CalcularPresupuestoHandler _sut =
        new(new CalculadoraPresupuesto(new EvaluadorMedicion(new EvaluadorFormula())));

    [Fact]
    public async Task Calcula_cierre_economico_desde_dtos_doc_4_5()
    {
        // Un capítulo con una partida de precio 100.000 y medición = 1 ⇒ PEM = 100.000,00.
        var partida = new PartidaCalculoDto(
            "01.001", "Partida", 100000m,
            Lineas: [new LineaMedicionDto(Uds: 1m)]);
        var capitulo = new CapituloCalculoDto("01", "Capítulo", [partida], Subcapitulos: []);

        var comando = new CalcularPresupuestoCommand(
            IvaPct: 21m, BajaPct: 12m, DescuentosImporte: 1760m, Capitulos: [capitulo]);

        var resultado = await _sut.Handle(comando, CancellationToken.None);

        resultado.Exito.Should().BeTrue();
        var dto = resultado.Valor;
        dto.Pem.Should().Be(100000.00m);
        dto.PemAjustado.Should().Be(88000.00m);
        dto.BaseImponible.Should().Be(86240.00m);
        dto.CuotaIva.Should().Be(18110.40m);
        dto.Total.Should().Be(104350.40m);

        dto.SubtotalesCapitulos.Should().ContainSingle()
            .Which.Subtotal.Should().Be(100000.00m);
    }
}
