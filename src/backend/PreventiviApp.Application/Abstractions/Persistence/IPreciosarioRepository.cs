using PreventiviApp.Application.Abstractions.Importacion;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;

namespace PreventiviApp.Application.Abstractions.Persistence;

/// <summary>
/// Persistencia del grafo de un preciosario importado. La implementación concreta
/// (EF Core, upsert idempotente por código) vive en Infrastructure.
/// </summary>
public interface IPreciosarioRepository
{
    /// <summary>
    /// Añade a la unidad de trabajo el preciosario y todas sus entidades hijas.
    /// La confirmación se realiza con <see cref="IUnitOfWork.GuardarCambiosAsync"/>.
    /// </summary>
    Task AnadirPreciosarioAsync(PreciosarioImportado grafo, CancellationToken ct);

    // --- Lectura / navegación del catálogo ---

    Task<IReadOnlyList<Preciosario>> ListarPreciosariosAsync(CancellationToken ct = default);

    Task<Preciosario?> ObtenerPreciosarioAsync(Guid id, CancellationToken ct = default);

    /// <summary>Capítulos de un preciosario; si <paramref name="padreId"/> es null, devuelve las raíces.</summary>
    Task<IReadOnlyList<Capitulo>> ListarCapitulosAsync(Guid preciosarioId, Guid? padreId, CancellationToken ct = default);

    Task<IReadOnlyList<Partida>> ListarPartidasAsync(Guid capituloId, CancellationToken ct = default);

    Task<Partida?> ObtenerPartidaAsync(Guid partidaId, CancellationToken ct = default);

    /// <summary>Líneas de descompuesto (análisis de precios) de una partida.</summary>
    Task<IReadOnlyList<Descompuesto>> ListarDescompuestoAsync(Guid partidaId, CancellationToken ct = default);

    /// <summary>Precio vigente de cada partida del preciosario, indexado por código (para «Actualizar precios»).</summary>
    Task<IReadOnlyDictionary<string, decimal>> ObtenerPreciosPorCodigoAsync(Guid preciosarioId, CancellationToken ct = default);

    /// <summary>Estado (código, precio, bloqueo) de las partidas del catálogo, para la conciliación al reimportar.</summary>
    Task<IReadOnlyList<EstadoPartidaCatalogo>> ObtenerEstadoPartidasCatalogoAsync(Guid preciosarioId, CancellationToken ct = default);

    /// <summary>Partidas del preciosario en seguimiento (tracked), para aplicar la reimportación.</summary>
    Task<IReadOnlyList<Partida>> ObtenerPartidasTrackedAsync(Guid preciosarioId, CancellationToken ct = default);
}
