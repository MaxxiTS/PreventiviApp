using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;

namespace PreventiviApp.Application.Proyectos;

// --- Listar proyectos ---

public sealed record ListarProyectosQuery : IRequest<IReadOnlyList<ProyectoDto>>;

internal sealed class ListarProyectosHandler(IProyectoRepository repositorio)
    : IRequestHandler<ListarProyectosQuery, IReadOnlyList<ProyectoDto>>
{
    public async Task<IReadOnlyList<ProyectoDto>> Handle(ListarProyectosQuery request, CancellationToken cancellationToken)
    {
        var proyectos = await repositorio.ListarAsync(cancellationToken);
        return proyectos.Select(ProyectoDto.De).ToList();
    }
}

// --- Obtener un proyecto ---

public sealed record ObtenerProyectoQuery(Guid Id) : IRequest<Result<ProyectoDto>>;

internal sealed class ObtenerProyectoHandler(IProyectoRepository repositorio)
    : IRequestHandler<ObtenerProyectoQuery, Result<ProyectoDto>>
{
    public async Task<Result<ProyectoDto>> Handle(ObtenerProyectoQuery request, CancellationToken cancellationToken)
    {
        var proyecto = await repositorio.ObtenerPorIdAsync(request.Id, cancellationToken);
        return proyecto is null
            ? Result.Fallo<ProyectoDto>(Error.NoEncontrado($"No existe el proyecto {request.Id}."))
            : Result.Ok(ProyectoDto.De(proyecto));
    }
}
