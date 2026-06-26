using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Proyectos;

public sealed record ProyectoDto(
    Guid Id,
    string Nombre,
    Guid? ClienteId,
    string? Direccion,
    DateOnly? Fecha,
    string Estado)
{
    public static ProyectoDto De(Proyecto p) =>
        new(p.Id, p.Nombre, p.ClienteId, p.Direccion, p.Fecha, p.Estado.ToString());
}
