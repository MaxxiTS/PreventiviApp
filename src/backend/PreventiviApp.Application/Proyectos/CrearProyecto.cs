using FluentValidation;
using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Proyectos;

public sealed record CrearProyectoCommand(
    string Nombre,
    Guid? ClienteId = null,
    string? Direccion = null,
    DateOnly? Fecha = null) : IRequest<Result<ProyectoDto>>;

public sealed class CrearProyectoValidator : AbstractValidator<CrearProyectoCommand>
{
    public CrearProyectoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Direccion).MaximumLength(500);
    }
}

internal sealed class CrearProyectoHandler(
    IProyectoRepository proyectos,
    IClienteRepository clientes,
    IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<CrearProyectoCommand, Result<ProyectoDto>>
{
    public async Task<Result<ProyectoDto>> Handle(CrearProyectoCommand request, CancellationToken cancellationToken)
    {
        if (request.ClienteId is { } clienteId)
        {
            var cliente = await clientes.ObtenerPorIdAsync(clienteId, cancellationToken);
            if (cliente is null)
                return Result.Fallo<ProyectoDto>(Error.NoEncontrado($"No existe el cliente {clienteId}."));
        }

        var proyecto = new Proyecto(Guid.NewGuid(), request.Nombre.Trim(), request.ClienteId)
        {
            Direccion = request.Direccion,
            Fecha = request.Fecha,
        };

        await proyectos.AnadirAsync(proyecto, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Ok(ProyectoDto.De(proyecto));
    }
}
