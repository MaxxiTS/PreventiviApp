using FluentValidation;
using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Presupuestos;

public sealed record AnadirCapituloPresupuestoCommand(
    Guid PresupuestoId,
    string Codigo,
    string Titulo,
    Guid? PadreId = null,
    int Orden = 1) : IRequest<Result<Guid>>;

public sealed class AnadirCapituloPresupuestoValidator : AbstractValidator<AnadirCapituloPresupuestoCommand>
{
    public AnadirCapituloPresupuestoValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(500);
    }
}

internal sealed class AnadirCapituloPresupuestoHandler(
    IPresupuestoRepository presupuestos,
    IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<AnadirCapituloPresupuestoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AnadirCapituloPresupuestoCommand request, CancellationToken cancellationToken)
    {
        var presupuesto = await presupuestos.ObtenerPorIdAsync(request.PresupuestoId, cancellationToken);
        if (presupuesto is null)
            return Result.Fallo<Guid>(Error.NoEncontrado($"No existe el presupuesto {request.PresupuestoId}."));

        if (request.PadreId is { } padreId)
        {
            var padre = await presupuestos.ObtenerCapituloAsync(padreId, cancellationToken);
            if (padre is null)
                return Result.Fallo<Guid>(Error.NoEncontrado($"No existe el capítulo padre {padreId}."));
        }

        var capitulo = new CapituloPresupuesto(
            Guid.NewGuid(), request.PresupuestoId, request.Codigo.Trim(), request.Titulo.Trim(),
            request.Orden, request.PadreId);

        await presupuestos.AnadirCapituloAsync(capitulo, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Ok(capitulo.Id);
    }
}
