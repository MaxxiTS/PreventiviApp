using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Services;

namespace PreventiviApp.Application.Presupuestos;

public sealed record ResultadoActualizacionPreciosDto(
    int Actualizadas,
    int Respetadas,
    int SinCorrespondencia,
    int SinCambio);

/// <summary>
/// Copia los precios del preciosario al presupuesto (por código) respetando los
/// precios bloqueados (doc 12 §5.2). No modifica el preciosario.
/// </summary>
public sealed record ActualizarPreciosDesdePreciosarioCommand(Guid PresupuestoId, Guid PreciosarioId)
    : IRequest<Result<ResultadoActualizacionPreciosDto>>;

internal sealed class ActualizarPreciosDesdePreciosarioHandler(
    IPresupuestoRepository presupuestos,
    IPreciosarioRepository preciosarios,
    ActualizadorPreciosPresupuesto actualizador,
    IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<ActualizarPreciosDesdePreciosarioCommand, Result<ResultadoActualizacionPreciosDto>>
{
    public async Task<Result<ResultadoActualizacionPreciosDto>> Handle(
        ActualizarPreciosDesdePreciosarioCommand request, CancellationToken cancellationToken)
    {
        var presupuesto = await presupuestos.ObtenerPorIdAsync(request.PresupuestoId, cancellationToken);
        if (presupuesto is null)
            return Result.Fallo<ResultadoActualizacionPreciosDto>(
                Error.NoEncontrado($"No existe el presupuesto {request.PresupuestoId}."));

        var preciosario = await preciosarios.ObtenerPreciosarioAsync(request.PreciosarioId, cancellationToken);
        if (preciosario is null)
            return Result.Fallo<ResultadoActualizacionPreciosDto>(
                Error.NoEncontrado($"No existe el preciosario {request.PreciosarioId}."));

        var partidas = await presupuestos.ListarPartidasTrackedAsync(request.PresupuestoId, cancellationToken);
        var precios = await preciosarios.ObtenerPreciosPorCodigoAsync(request.PreciosarioId, cancellationToken);

        var resultado = actualizador.Actualizar(partidas, precios);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Ok(new ResultadoActualizacionPreciosDto(
            resultado.Actualizadas, resultado.Respetadas, resultado.SinCorrespondencia, resultado.SinCambio));
    }
}
