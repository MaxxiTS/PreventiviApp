using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Preciosario (base de datos de precios) importado de un archivo DCF. Contiene
/// el árbol de capítulos y partidas con sus análisis de precios.
/// </summary>
public sealed class Preciosario : AuditableEntity
{
    public Preciosario(Guid id, string nombre, string version) : base(id)
    {
        Nombre = nombre;
        VersionPreciosario = version;
    }

    public string Nombre { get; set; }
    public string VersionPreciosario { get; set; }
    public string? Fuente { get; set; }
    public DateOnly? FechaPublicacion { get; set; }

    /// <summary>Origen del archivo DCF importado (nombre/ruta).</summary>
    public string? OrigenDcf { get; set; }

    /// <summary>Hash del archivo importado (para idempotencia de reimportación).</summary>
    public string? HashArchivo { get; set; }
}
