using FluentValidation;
using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Presupuestos;

public sealed record CrearPresupuestoCommand(
    Guid ProyectoId,
    string Nombre,
    decimal IvaPct = 21m,
    decimal CostesIndirectosPct = 0m,
    decimal BajaPct = 0m,
    decimal DescuentosImporte = 0m) : IRequest<Result<Guid>>;

public sealed class CrearPresupuestoValidator : AbstractValidator<CrearPresupuestoCommand>
{
    public CrearPresupuestoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IvaPct).InclusiveBetween(0m, 100m);
        RuleFor(x => x.CostesIndirectosPct).InclusiveBetween(0m, 100m);
        RuleFor(x => x.BajaPct).InclusiveBetween(0m, 100m);
        RuleFor(x => x.DescuentosImporte).GreaterThanOrEqualTo(0m);
    }
}

internal sealed class CrearPresupuestoHandler(
    IProyectoRepository proyectos,
    IPresupuestoRepository presupuestos,
    IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<CrearPresupuestoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearPresupuestoCommand request, CancellationToken cancellationToken)
    {
        var proyecto = await proyectos.ObtenerPorIdAsync(request.ProyectoId, cancellationToken);
        if (proyecto is null)
            return Result.Fallo<Guid>(Error.NoEncontrado($"No existe el proyecto {request.ProyectoId}."));

        var iva = new Iva(Guid.NewGuid(), "IVA", request.IvaPct);
        var presupuesto = new Presupuesto(Guid.NewGuid(), request.ProyectoId, request.Nombre.Trim(), iva)
        {
            CostesIndirectosPct = request.CostesIndirectosPct,
            BajaPct = request.BajaPct,
            DescuentosImporte = request.DescuentosImporte,
        };

        await presupuestos.AnadirAsync(presupuesto, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Ok(presupuesto.Id);
    }
}
