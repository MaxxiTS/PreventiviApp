using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>Cliente al que pertenecen uno o varios proyectos.</summary>
public sealed class Cliente : AuditableEntity
{
    public Cliente(Guid id, string nombre) : base(id) => Nombre = nombre;

    public string Nombre { get; set; }
    public string? Nif { get; set; }
    public string? Direccion { get; set; }
    public string? Contacto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}
