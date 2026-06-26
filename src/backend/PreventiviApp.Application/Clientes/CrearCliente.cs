using FluentValidation;
using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Clientes;

public sealed record CrearClienteCommand(
    string Nombre,
    string? Nif = null,
    string? Direccion = null,
    string? Contacto = null,
    string? Email = null,
    string? Telefono = null) : IRequest<Result<ClienteDto>>;

public sealed class CrearClienteValidator : AbstractValidator<CrearClienteCommand>
{
    public CrearClienteValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Nif).MaximumLength(20);
    }
}

internal sealed class CrearClienteHandler(IClienteRepository repositorio, IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<CrearClienteCommand, Result<ClienteDto>>
{
    public async Task<Result<ClienteDto>> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = new Cliente(Guid.NewGuid(), request.Nombre.Trim())
        {
            Nif = request.Nif,
            Direccion = request.Direccion,
            Contacto = request.Contacto,
            Email = request.Email,
            Telefono = request.Telefono,
        };

        await repositorio.AnadirAsync(cliente, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Ok(ClienteDto.De(cliente));
    }
}
