using FluentValidation;
using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Presupuestos;

public sealed record AnadirPartidaPresupuestoCommand(
    Guid CapituloPresupuestoId,
    string Codigo,
    string Resumen,
    decimal Precio,
    IReadOnlyList<LineaMedicionDto> Lineas,
    Guid? PartidaOrigenId = null) : IRequest<Result<Guid>>;

public sealed class AnadirPartidaPresupuestoValidator : AbstractValidator<AnadirPartidaPresupuestoCommand>
{
    public AnadirPartidaPresupuestoValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Resumen).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Precio).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.Lineas).NotNull();
    }
}

internal sealed class AnadirPartidaPresupuestoHandler(
    IPresupuestoRepository presupuestos,
    IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<AnadirPartidaPresupuestoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AnadirPartidaPresupuestoCommand request, CancellationToken cancellationToken)
    {
        var capitulo = await presupuestos.ObtenerCapituloAsync(request.CapituloPresupuestoId, cancellationToken);
        if (capitulo is null)
            return Result.Fallo<Guid>(Error.NoEncontrado($"No existe el capítulo de presupuesto {request.CapituloPresupuestoId}."));

        var partida = new PartidaPresupuesto(
            Guid.NewGuid(), request.CapituloPresupuestoId, request.Codigo.Trim(), request.Resumen.Trim(),
            request.Precio, request.PartidaOrigenId);

        foreach (var lineaDto in request.Lineas)
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

        await presupuestos.AnadirPartidaAsync(partida, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Ok(partida.Id);
    }
}
