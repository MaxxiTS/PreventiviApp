using FluentValidation;
using MediatR;
using PreventiviApp.Application.Abstractions.Importacion;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;

namespace PreventiviApp.Application.Preciosarios;

public sealed record ResultadoReimportacionDto(
    int NuevasDetectadas,
    int Actualizadas,
    int Respetadas,
    int SinCambio,
    int Obsoletas);

/// <summary>
/// Reimporta un DCF sobre un preciosario existente aplicando la política conservadora
/// del doc 12 §6: actualiza precios no bloqueados, respeta los bloqueados y marca como
/// obsoletas las partidas ausentes. No modifica presupuestos.
///
/// Nota: en esta versión la conciliación cubre actualización de precios y obsolescencia;
/// el alta automática de partidas/capítulos/recursos nuevos se reporta (NuevasDetectadas)
/// y se abordará en un incremento posterior.
/// </summary>
public sealed record ReimportarDcfCommand(Guid PreciosarioId, Stream Contenido, string NombreArchivo)
    : IRequest<Result<ResultadoReimportacionDto>>;

public sealed class ReimportarDcfValidator : AbstractValidator<ReimportarDcfCommand>
{
    public ReimportarDcfValidator()
    {
        RuleFor(x => x.Contenido).NotNull();
        RuleFor(x => x.NombreArchivo).NotEmpty().MaximumLength(260);
    }
}

internal sealed class ReimportarDcfHandler(
    IImportadorPreciosario importador,
    IPreciosarioRepository preciosarios,
    ConciliadorPreciosario conciliador,
    IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<ReimportarDcfCommand, Result<ResultadoReimportacionDto>>
{
    public async Task<Result<ResultadoReimportacionDto>> Handle(ReimportarDcfCommand request, CancellationToken cancellationToken)
    {
        var preciosario = await preciosarios.ObtenerPreciosarioAsync(request.PreciosarioId, cancellationToken);
        if (preciosario is null)
            return Result.Fallo<ResultadoReimportacionDto>(
                Error.NoEncontrado($"No existe el preciosario {request.PreciosarioId}."));

        var importacion = await importador.ImportarAsync(
            request.Contenido, request.NombreArchivo, progreso: null, cancellationToken);
        if (importacion.EsFallo)
            return Result.Fallo<ResultadoReimportacionDto>(importacion.Error);

        var entrantes = importacion.Valor.Partidas
            .Select(p => new PartidaEntranteDcf(p.Codigo, p.Precio))
            .ToList();

        var existentes = await preciosarios.ObtenerEstadoPartidasCatalogoAsync(request.PreciosarioId, cancellationToken);
        var decisiones = conciliador.Conciliar(existentes, entrantes);

        var tracked = await preciosarios.ObtenerPartidasTrackedAsync(request.PreciosarioId, cancellationToken);
        var partidasPorCodigo = new Dictionary<string, Partida>(StringComparer.OrdinalIgnoreCase);
        foreach (var partida in tracked)
            partidasPorCodigo[partida.Codigo] = partida;

        foreach (var decision in decisiones)
        {
            switch (decision.Accion)
            {
                case AccionConciliacion.Actualizar when partidasPorCodigo.TryGetValue(decision.Codigo, out var aActualizar):
                    aActualizar.ActualizarPrecio(decision.PrecioNuevo);
                    break;
                case AccionConciliacion.Obsoleta when partidasPorCodigo.TryGetValue(decision.Codigo, out var aMarcar):
                    aMarcar.MarcarObsoleta();
                    break;
            }
        }

        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        var dto = new ResultadoReimportacionDto(
            NuevasDetectadas: decisiones.Count(x => x.Accion == AccionConciliacion.Alta),
            Actualizadas: decisiones.Count(x => x.Accion == AccionConciliacion.Actualizar),
            Respetadas: decisiones.Count(x => x.Accion == AccionConciliacion.Respetar),
            SinCambio: decisiones.Count(x => x.Accion == AccionConciliacion.SinCambio),
            Obsoletas: decisiones.Count(x => x.Accion == AccionConciliacion.Obsoleta));

        return Result.Ok(dto);
    }
}
