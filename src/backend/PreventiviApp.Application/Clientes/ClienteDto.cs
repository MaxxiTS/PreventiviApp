using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Clientes;

public sealed record ClienteDto(
    Guid Id,
    string Nombre,
    string? Nif,
    string? Direccion,
    string? Contacto,
    string? Email,
    string? Telefono)
{
    public static ClienteDto De(Cliente c) =>
        new(c.Id, c.Nombre, c.Nif, c.Direccion, c.Contacto, c.Email, c.Telefono);
}
