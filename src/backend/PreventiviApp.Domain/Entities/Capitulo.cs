using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Capítulo del preciosario. Árbol jerárquico (autorreferencia <see cref="PadreId"/>)
/// que agrupa partidas y subcapítulos.
/// </summary>
public sealed class Capitulo : AuditableEntity
{
    public Capitulo(
        Guid id,
        Guid preciosarioId,
        string codigo,
        string titulo,
        int orden,
        Guid? padreId = null) : base(id)
    {
        PreciosarioId = preciosarioId;
        Codigo = codigo;
        Titulo = titulo;
        Orden = orden;
        PadreId = padreId;
    }

    public Guid PreciosarioId { get; private set; }
    public Guid? PadreId { get; set; }
    public string Codigo { get; set; }
    public string Titulo { get; set; }
    public int Orden { get; set; }
}
