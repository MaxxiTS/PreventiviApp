using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Domain.Services;

/// <summary>Resumen de una operación de «Actualizar precios» (doc 12 §5–§6).</summary>
public readonly record struct ResultadoActualizacionPrecios(
    int Actualizadas,
    int Respetadas,
    int SinCorrespondencia,
    int SinCambio);

/// <summary>
/// Aplica los precios de un preciosario a las partidas de un presupuesto, por código,
/// respetando los precios bloqueados (doc 12 §5.2/§5.3 y §6).
/// </summary>
public sealed class ActualizadorPreciosPresupuesto
{
    /// <param name="partidas">Partidas del presupuesto a revisar.</param>
    /// <param name="preciosPorCodigo">Precio vigente del preciosario indexado por código de partida.</param>
    public ResultadoActualizacionPrecios Actualizar(
        IEnumerable<PartidaPresupuesto> partidas,
        IReadOnlyDictionary<string, decimal> preciosPorCodigo)
    {
        int actualizadas = 0, respetadas = 0, sinCorrespondencia = 0, sinCambio = 0;

        foreach (var partida in partidas)
        {
            if (!preciosPorCodigo.TryGetValue(partida.Codigo, out var nuevoPrecio))
            {
                sinCorrespondencia++; // huérfana: el código ya no existe en el preciosario
                continue;
            }

            if (partida.PrecioBloqueado)
            {
                respetadas++; // nunca se sobrescribe un precio bloqueado
                continue;
            }

            if (partida.ActualizarPrecioDesdePreciosario(nuevoPrecio))
                actualizadas++;
            else
                sinCambio++;
        }

        return new ResultadoActualizacionPrecios(actualizadas, respetadas, sinCorrespondencia, sinCambio);
    }
}
