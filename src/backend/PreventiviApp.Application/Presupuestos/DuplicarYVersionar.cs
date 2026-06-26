using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Presupuestos;

// --- Duplicar una partida (con su medición) ---

public sealed record DuplicarPartidaCommand(Guid PartidaPresupuestoId) : IRequest<Result<Guid>>;

internal sealed class DuplicarPartidaHandler(IPresupuestoRepository presupuestos, IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<DuplicarPartidaCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(DuplicarPartidaCommand request, CancellationToken cancellationToken)
    {
        var partida = await presupuestos.ObtenerPartidaConMedicionAsync(request.PartidaPresupuestoId, cancellationToken);
        if (partida is null)
            return Result.Fallo<Guid>(Error.NoEncontrado($"No existe la partida de presupuesto {request.PartidaPresupuestoId}."));

        var copia = partida.Duplicar(partida.CapituloPresupuestoId);
        await presupuestos.AnadirPartidaAsync(copia, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Ok(copia.Id);
    }
}

// --- Duplicar un capítulo completo (copia profunda, como hermano) ---

public sealed record DuplicarCapituloCommand(Guid CapituloPresupuestoId) : IRequest<Result<Guid>>;

internal sealed class DuplicarCapituloHandler(IPresupuestoRepository presupuestos, IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<DuplicarCapituloCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(DuplicarCapituloCommand request, CancellationToken cancellationToken)
    {
        var referencia = await presupuestos.ObtenerCapituloAsync(request.CapituloPresupuestoId, cancellationToken);
        if (referencia is null)
            return Result.Fallo<Guid>(Error.NoEncontrado($"No existe el capítulo de presupuesto {request.CapituloPresupuestoId}."));

        var arbol = await presupuestos.ObtenerArbolAsync(referencia.PresupuestoId, cancellationToken);
        var nodo = arbol is null ? null : Buscar(arbol.CapitulosRaiz, request.CapituloPresupuestoId);
        if (nodo is null)
            return Result.Fallo<Guid>(Error.NoEncontrado($"No se pudo cargar el capítulo {request.CapituloPresupuestoId}."));

        var copia = nodo.Duplicar(nodo.PadreId);
        await presupuestos.AnadirCapituloAsync(copia, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Ok(copia.Id);

        static CapituloPresupuesto? Buscar(IEnumerable<CapituloPresupuesto> capitulos, Guid id)
        {
            foreach (var capitulo in capitulos)
            {
                if (capitulo.Id == id)
                    return capitulo;
                var encontrado = Buscar(capitulo.Subcapitulos, id);
                if (encontrado is not null)
                    return encontrado;
            }
            return null;
        }
    }
}

// --- Crear una nueva versión del presupuesto (snapshot) ---

public sealed record CrearNuevaVersionPresupuestoCommand(Guid PresupuestoId) : IRequest<Result<Guid>>;

internal sealed class CrearNuevaVersionPresupuestoHandler(IPresupuestoRepository presupuestos, IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<CrearNuevaVersionPresupuestoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CrearNuevaVersionPresupuestoCommand request, CancellationToken cancellationToken)
    {
        var arbol = await presupuestos.ObtenerArbolAsync(request.PresupuestoId, cancellationToken);
        if (arbol is null)
            return Result.Fallo<Guid>(Error.NoEncontrado($"No existe el presupuesto {request.PresupuestoId}."));

        var nueva = arbol.CrearNuevaVersion();
        await presupuestos.AnadirAsync(nueva, cancellationToken);
        foreach (var raiz in nueva.CapitulosRaiz)
            await presupuestos.AnadirCapituloAsync(raiz, cancellationToken);

        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Ok(nueva.Id);
    }
}
