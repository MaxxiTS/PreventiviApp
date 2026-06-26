using FluentValidation;
using MediatR;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;

namespace PreventiviApp.Application.Presupuestos;

/// <summary>
/// Calcula los totales de un presupuesto a partir de su árbol de capítulos,
/// partidas y mediciones, sin persistirlo. Reutiliza el motor de dominio
/// <see cref="CalculadoraPresupuesto"/>.
/// </summary>
public sealed record CalcularPresupuestoCommand(
    decimal IvaPct,
    decimal BajaPct,
    decimal DescuentosImporte,
    IReadOnlyList<CapituloCalculoDto> Capitulos) : IRequest<Result<PresupuestoCalculadoDto>>;

public sealed class CalcularPresupuestoValidator : AbstractValidator<CalcularPresupuestoCommand>
{
    public CalcularPresupuestoValidator()
    {
        RuleFor(x => x.IvaPct).InclusiveBetween(0m, 100m);
        RuleFor(x => x.BajaPct).InclusiveBetween(0m, 100m);
        RuleFor(x => x.DescuentosImporte).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.Capitulos).NotEmpty();
    }
}

internal sealed class CalcularPresupuestoHandler(CalculadoraPresupuesto calculadora)
    : IRequestHandler<CalcularPresupuestoCommand, Result<PresupuestoCalculadoDto>>
{
    public Task<Result<PresupuestoCalculadoDto>> Handle(
        CalcularPresupuestoCommand request,
        CancellationToken cancellationToken)
    {
        var iva = new Iva(Guid.NewGuid(), "IVA", request.IvaPct);
        var presupuesto = new Presupuesto(Guid.NewGuid(), Guid.NewGuid(), "Cálculo", iva)
        {
            BajaPct = request.BajaPct,
            DescuentosImporte = request.DescuentosImporte,
        };

        var orden = 1;
        foreach (var capituloDto in request.Capitulos)
            presupuesto.AnadirCapituloRaiz(ConstruirCapitulo(capituloDto, presupuesto.Id, orden++));

        var resultado = calculadora.CalcularPresupuesto(presupuesto);

        var subtotales = presupuesto.CapitulosRaiz
            .Select(c => new CapituloCalculadoDto(c.Codigo, c.Titulo, calculadora.CalcularSubtotalCapitulo(c)))
            .ToList();

        var dto = new PresupuestoCalculadoDto(
            resultado.Pem,
            resultado.PemAjustado,
            resultado.BaseImponible,
            resultado.CuotaIva,
            resultado.Total,
            subtotales);

        return Task.FromResult(Result.Ok(dto));
    }

    private static CapituloPresupuesto ConstruirCapitulo(
        CapituloCalculoDto dto,
        Guid presupuestoId,
        int orden,
        Guid? padreId = null)
    {
        var capitulo = new CapituloPresupuesto(
            Guid.NewGuid(), presupuestoId, dto.Codigo, dto.Titulo, orden, padreId);

        var ordenPartida = 1;
        foreach (var partidaDto in dto.Partidas)
            capitulo.AnadirPartida(ConstruirPartida(partidaDto, capitulo.Id, ordenPartida++));

        var ordenSub = 1;
        foreach (var subDto in dto.Subcapitulos)
            capitulo.AnadirSubcapitulo(ConstruirCapitulo(subDto, presupuestoId, ordenSub++, capitulo.Id));

        return capitulo;
    }

    private static PartidaPresupuesto ConstruirPartida(PartidaCalculoDto dto, Guid capituloId, int orden)
    {
        var partida = new PartidaPresupuesto(Guid.NewGuid(), capituloId, dto.Codigo, dto.Resumen, dto.Precio);

        foreach (var lineaDto in dto.Lineas)
        {
            var linea = partida.Medicion.AnadirLinea();
            linea.Comentario = lineaDto.Comentario ?? string.Empty;
            linea.EsComentario = lineaDto.EsComentario;
            linea.Uds = lineaDto.Uds;
            linea.Largo = lineaDto.Largo;
            linea.Ancho = lineaDto.Ancho;
            linea.Alto = lineaDto.Alto;
            linea.Coeficiente = lineaDto.Coeficiente;
            linea.Formula = lineaDto.Formula;
        }

        return partida;
    }
}
