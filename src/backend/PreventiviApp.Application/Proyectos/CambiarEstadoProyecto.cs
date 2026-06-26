using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Enums;

namespace PreventiviApp.Application.Proyectos;

public sealed record CambiarEstadoProyectoCommand(Guid Id, EstadoProyecto NuevoEstado)
    : IRequest<Result<ProyectoDto>>;

internal sealed class CambiarEstadoProyectoHandler(IProyectoRepository repositorio, IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<CambiarEstadoProyectoCommand, Result<ProyectoDto>>
{
    public async Task<Result<ProyectoDto>> Handle(CambiarEstadoProyectoCommand request, CancellationToken cancellationToken)
    {
        var proyecto = await repositorio.ObtenerPorIdAsync(request.Id, cancellationToken);
        if (proyecto is null)
            return Result.Fallo<ProyectoDto>(Error.NoEncontrado($"No existe el proyecto {request.Id}."));

        var transicion = proyecto.CambiarEstado(request.NuevoEstado);
        if (transicion.EsFallo)
            return Result.Fallo<ProyectoDto>(transicion.Error);

        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Ok(ProyectoDto.De(proyecto));
    }
}
