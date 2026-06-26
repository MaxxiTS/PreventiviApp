using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>Unidad de medida (m, m², m³, ud, kg, h...).</summary>
public sealed class Unidad : AuditableEntity
{
    public Unidad(Guid id, string codigo, string nombre) : base(id)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    public string Codigo { get; set; }
    public string Nombre { get; set; }
}
