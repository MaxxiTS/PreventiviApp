using FluentAssertions;
using PreventiviApp.Domain.Services;
using Xunit;

namespace PreventiviApp.Tests.Unit;

public class EvaluadorFormulaTests
{
    private readonly EvaluadorFormula _sut = new();

    private static IReadOnlyDictionary<string, decimal> Vars(
        decimal uds = 0, decimal largo = 0, decimal ancho = 0, decimal alto = 0)
        => new Dictionary<string, decimal>
        {
            ["uds"] = uds, ["largo"] = largo, ["ancho"] = ancho, ["alto"] = alto,
        };

    [Theory]
    [InlineData("2 + 3 * 4", 14)]          // precedencia
    [InlineData("(2 + 3) * 4", 20)]        // paréntesis
    [InlineData("10 / 4", 2.5)]            // división decimal
    [InlineData("-3 + 5", 2)]              // unario negativo
    [InlineData("16.2 * 0.03", 0.486)]     // doc §3.5 línea 3
    [InlineData("2 - 3 - 4", -5)]          // asociatividad izquierda
    public void Evalua_aritmetica_basica(string formula, decimal esperado)
        => _sut.Evaluar(formula, Vars()).Should().Be(esperado);

    [Fact]
    public void Multiplica_dimensiones_con_merma() // doc §3.4
        => _sut.Evaluar("uds * largo * ancho * 1.05", Vars(uds: 10, largo: 2.40m, ancho: 1.20m))
               .Should().Be(30.24m);

    [Theory]
    [InlineData("abs(-4.5)", 4.5)]
    [InlineData("min(3, 7)", 3)]
    [InlineData("max(3, 7)", 7)]
    [InlineData("round(2.5)", 3)]          // AwayFromZero
    [InlineData("round(2.345, 2)", 2.35)]
    public void Evalua_funciones(string formula, decimal esperado)
        => _sut.Evaluar(formula, Vars()).Should().Be(esperado);

    [Fact]
    public void Diagonal_con_sqrt_y_pow() // doc §3.4: sqrt(pow(largo,2)+pow(alto,2))
        => _sut.Evaluar("sqrt(pow(largo,2) + pow(alto,2))", Vars(largo: 1.20m, alto: 0.90m))
               .Should().BeApproximately(1.5m, 0.0001m);

    [Fact]
    public void Seccion_circular_con_PI() // doc §3.4: PI*pow(0.30,2)/4*largo, largo=3 ⇒ ~0.212
        => _sut.Evaluar("PI * pow(0.30, 2) / 4 * largo", Vars(largo: 3.00m))
               .Should().BeApproximately(0.212m, 0.001m);

    [Theory]
    [InlineData("2 +")]            // expresión incompleta
    [InlineData("foo(1)")]         // función desconocida
    [InlineData("desconocida")]    // variable desconocida
    [InlineData("1 / 0")]          // división por cero
    [InlineData("(2 + 3")]         // paréntesis sin cerrar
    [InlineData("2 ) 3")]          // texto sobrante
    [InlineData("")]               // vacía
    public void Lanza_en_formula_invalida(string formula)
    {
        var accion = () => _sut.Evaluar(formula, Vars());
        accion.Should().Throw<FormatException>();
    }
}
