using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Medición de una <see cref="PartidaPresupuesto"/>: conjunto ordenado de
/// <see cref="LineaMedicion"/>. Su total es la suma de los parciales (lo calcula
/// <c>EvaluadorMedicion</c>).
/// </summary>
public sealed class Medicion : AuditableEntity
{
    private readonly List<LineaMedicion> _lineas = [];

    public Medicion(Guid id, Guid partidaPresupuestoId) : base(id)
        => PartidaPresupuestoId = partidaPresupuestoId;

    public Guid PartidaPresupuestoId { get; private set; }

    public IReadOnlyList<LineaMedicion> Lineas => _lineas;

    public LineaMedicion AnadirLinea()
    {
        var linea = new LineaMedicion(Guid.NewGuid(), Id, _lineas.Count + 1);
        _lineas.Add(linea);
        return linea;
    }

    public void AnadirLinea(LineaMedicion linea) => _lineas.Add(linea);

    public void EliminarLinea(LineaMedicion linea) => _lineas.Remove(linea);
}
