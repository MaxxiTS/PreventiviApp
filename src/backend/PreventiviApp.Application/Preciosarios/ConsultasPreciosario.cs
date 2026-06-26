using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;

namespace PreventiviApp.Application.Preciosarios;

// --- Listar preciosarios ---

public sealed record ListarPreciosariosQuery : IRequest<IReadOnlyList<PreciosarioDto>>;

internal sealed class ListarPreciosariosHandler(IPreciosarioRepository repositorio)
    : IRequestHandler<ListarPreciosariosQuery, IReadOnlyList<PreciosarioDto>>
{
    public async Task<IReadOnlyList<PreciosarioDto>> Handle(ListarPreciosariosQuery request, CancellationToken cancellationToken)
        => (await repositorio.ListarPreciosariosAsync(cancellationToken)).Select(PreciosarioDto.De).ToList();
}

// --- Navegar capítulos (raíces o subcapítulos de un padre) ---

public sealed record ListarCapitulosQuery(Guid PreciosarioId, Guid? PadreId = null)
    : IRequest<IReadOnlyList<CapituloDto>>;

internal sealed class ListarCapitulosHandler(IPreciosarioRepository repositorio)
    : IRequestHandler<ListarCapitulosQuery, IReadOnlyList<CapituloDto>>
{
    public async Task<IReadOnlyList<CapituloDto>> Handle(ListarCapitulosQuery request, CancellationToken cancellationToken)
        => (await repositorio.ListarCapitulosAsync(request.PreciosarioId, request.PadreId, cancellationToken))
            .Select(CapituloDto.De).ToList();
}

// --- Listar partidas de un capítulo ---

public sealed record ListarPartidasQuery(Guid CapituloId) : IRequest<IReadOnlyList<PartidaDto>>;

internal sealed class ListarPartidasHandler(IPreciosarioRepository repositorio)
    : IRequestHandler<ListarPartidasQuery, IReadOnlyList<PartidaDto>>
{
    public async Task<IReadOnlyList<PartidaDto>> Handle(ListarPartidasQuery request, CancellationToken cancellationToken)
        => (await repositorio.ListarPartidasAsync(request.CapituloId, cancellationToken))
            .Select(PartidaDto.De).ToList();
}

// --- Análisis de precios (descompuesto) de una partida ---

public sealed record ObtenerAnalisisPreciosQuery(Guid PartidaId) : IRequest<Result<AnalisisPreciosDto>>;

internal sealed class ObtenerAnalisisPreciosHandler(IPreciosarioRepository repositorio)
    : IRequestHandler<ObtenerAnalisisPreciosQuery, Result<AnalisisPreciosDto>>
{
    public async Task<Result<AnalisisPreciosDto>> Handle(ObtenerAnalisisPreciosQuery request, CancellationToken cancellationToken)
    {
        var partida = await repositorio.ObtenerPartidaAsync(request.PartidaId, cancellationToken);
        if (partida is null)
            return Result.Fallo<AnalisisPreciosDto>(Error.NoEncontrado($"No existe la partida {request.PartidaId}."));

        var descompuesto = await repositorio.ListarDescompuestoAsync(request.PartidaId, cancellationToken);
        var costeDirecto = descompuesto.Sum(d => d.Rendimiento * d.PrecioUnitario);

        var dto = new AnalisisPreciosDto(
            partida.Id, partida.Codigo, partida.Resumen, partida.Precio,
            costeDirecto, descompuesto.Select(DescompuestoDto.De).ToList());

        return Result.Ok(dto);
    }
}
