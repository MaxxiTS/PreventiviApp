using PreventiviApp.Domain.Common;

namespace PreventiviApp.Application.Abstractions.Importacion;

/// <summary>
/// Estrategia de importación de un preciosario desde un formato concreto (DCF, BC3,
/// IFC...). Implementa el patrón Strategy: cada formato aporta su propio importador
/// y todos convergen al modelo canónico <see cref="PreciosarioImportado"/>.
/// </summary>
public interface IImportadorPreciosario
{
    /// <summary>
    /// Lee el <paramref name="contenido"/> en streaming y construye el grafo del
    /// preciosario en memoria, resolviendo relaciones por código. No persiste nada.
    /// </summary>
    /// <param name="contenido">Stream del archivo a importar.</param>
    /// <param name="nombreArchivo">Nombre del archivo de origen (para trazabilidad).</param>
    /// <param name="progreso">Receptor opcional de eventos de progreso.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>
    /// <see cref="Result.Ok{T}"/> con el grafo importado, o
    /// <see cref="Result.Fallo{T}"/> si el archivo es ilegible o está vacío.
    /// </returns>
    Task<Result<PreciosarioImportado>> ImportarAsync(
        Stream contenido,
        string nombreArchivo,
        IProgress<ProgresoImportacion>? progreso = null,
        CancellationToken cancellationToken = default);

    /// <summary>Errores y advertencias acumulados durante la última importación.</summary>
    IReadOnlyList<ErrorImportacion> Errores { get; }
}
