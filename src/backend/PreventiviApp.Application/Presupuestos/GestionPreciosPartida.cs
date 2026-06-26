using FluentValidation;
using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;

namespace PreventiviApp.Application.Presupuestos;

// --- Editar el precio de una partida (edición manual, permitida aunque esté bloqueada) ---

public sealed record EditarPrecioPartidaCommand(Guid PartidaPresupuestoId, decimal NuevoPrecio)
    : IRequest<Result<Guid>>;

public sealed class EditarPrecioPartidaValidator : AbstractValidator<EditarPrecioPartidaCommand>
{
    public EditarPrecioPartidaValidator() => RuleFor(x => x.NuevoPrecio).GreaterThanOrEqualTo(0m);
}

internal sealed class EditarPrecioPartidaHandler(IPresupuestoRepository presupuestos, IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<EditarPrecioPartidaCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(EditarPrecioPartidaCommand request, CancellationToken cancellationToken)
    {
        var partida = await presupuestos.ObtenerPartidaTrackedAsync(request.PartidaPresupuestoId, cancellationToken);
        if (partida is null)
            return Result.Fallo<Guid>(Error.NoEncontrado($"No existe la partida de presupuesto {request.PartidaPresupuestoId}."));

        partida.EditarPrecio(request.NuevoPrecio);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Ok(partida.Id);
    }
}

// --- Bloquear / desbloquear el precio de una partida ---

public sealed record BloquearPrecioPartidaCommand(Guid PartidaPresupuestoId, bool Bloqueado)
    : IRequest<Result<Guid>>;

internal sealed class BloquearPrecioPartidaHandler(IPresupuestoRepository presupuestos, IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<BloquearPrecioPartidaCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(BloquearPrecioPartidaCommand request, CancellationToken cancellationToken)
    {
        var partida = await presupuestos.ObtenerPartidaTrackedAsync(request.PartidaPresupuestoId, cancellationToken);
        if (partida is null)
            return Result.Fallo<Guid>(Error.NoEncontrado($"No existe la partida de presupuesto {request.PartidaPresupuestoId}."));

        if (request.Bloqueado)
            partida.BloquearPrecio();
        else
            partida.DesbloquearPrecio();

        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Ok(partida.Id);
    }
}
