using MediatR;
using PreventiviApp.Application.Abstractions.Documentos;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;

namespace PreventiviApp.Application.Presupuestos;

// --- Obtener el presupuesto completo con totales calculados ---

public sealed record ObtenerPresupuestoQuery(Guid Id) : IRequest<Result<PresupuestoDetalleDto>>;

internal sealed class ObtenerPresupuestoHandler(
    IPresupuestoRepository repositorio,
    CalculadoraPresupuesto calculadora,
    EvaluadorMedicion mediciones)
    : IRequestHandler<ObtenerPresupuestoQuery, Result<PresupuestoDetalleDto>>
{
    public async Task<Result<PresupuestoDetalleDto>> Handle(ObtenerPresupuestoQuery request, CancellationToken cancellationToken)
    {
        var presupuesto = await repositorio.ObtenerArbolAsync(request.Id, cancellationToken);
        if (presupuesto is null)
            return Result.Fallo<PresupuestoDetalleDto>(Error.NoEncontrado($"No existe el presupuesto {request.Id}."));

        var resultado = calculadora.CalcularPresupuesto(presupuesto);
        var capitulos = presupuesto.CapitulosRaiz.Select(MapearCapitulo).ToList();

        var dto = new PresupuestoDetalleDto(
            presupuesto.Id, presupuesto.Nombre, presupuesto.NumeroVersion,
            resultado.Pem, resultado.PemAjustado, resultado.BaseImponible, resultado.CuotaIva, resultado.Total,
            capitulos);

        return Result.Ok(dto);

        CapituloDetalleDto MapearCapitulo(CapituloPresupuesto capitulo)
        {
            var partidas = capitulo.Partidas
                .Select(p => new PartidaDetalleDto(
                    p.Id, p.Codigo, p.Resumen, p.Precio,
                    mediciones.CalcularTotal(p.Medicion),
                    calculadora.CalcularImportePartida(p)))
                .ToList();

            var subcapitulos = capitulo.Subcapitulos.Select(MapearCapitulo).ToList();

            return new CapituloDetalleDto(
                capitulo.Id, capitulo.Codigo, capitulo.Titulo,
                calculadora.CalcularSubtotalCapitulo(capitulo),
                partidas, subcapitulos);
        }
    }
}

// --- Listar presupuestos de un proyecto (con total calculado) ---

public sealed record ListarPresupuestosPorProyectoQuery(Guid ProyectoId)
    : IRequest<IReadOnlyList<PresupuestoResumenDto>>;

internal sealed class ListarPresupuestosPorProyectoHandler(
    IPresupuestoRepository repositorio,
    CalculadoraPresupuesto calculadora)
    : IRequestHandler<ListarPresupuestosPorProyectoQuery, IReadOnlyList<PresupuestoResumenDto>>
{
    public async Task<IReadOnlyList<PresupuestoResumenDto>> Handle(ListarPresupuestosPorProyectoQuery request, CancellationToken cancellationToken)
    {
        var presupuestos = await repositorio.ListarPorProyectoAsync(request.ProyectoId, cancellationToken);

        var resumenes = new List<PresupuestoResumenDto>(presupuestos.Count);
        foreach (var p in presupuestos)
        {
            var arbol = await repositorio.ObtenerArbolAsync(p.Id, cancellationToken);
            var total = arbol is null ? 0m : calculadora.CalcularPresupuesto(arbol).Total;
            resumenes.Add(new PresupuestoResumenDto(p.Id, p.Nombre, p.NumeroVersion, p.Estado.ToString(), total));
        }

        return resumenes;
    }
}

// --- Generar el PDF del presupuesto ---

public sealed record GenerarPdfPresupuestoQuery(Guid Id) : IRequest<Result<byte[]>>;

internal sealed class GenerarPdfPresupuestoHandler(
    IPresupuestoRepository repositorio,
    IGeneradorPresupuestoPdf generador)
    : IRequestHandler<GenerarPdfPresupuestoQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GenerarPdfPresupuestoQuery request, CancellationToken cancellationToken)
    {
        var presupuesto = await repositorio.ObtenerArbolAsync(request.Id, cancellationToken);
        if (presupuesto is null)
            return Result.Fallo<byte[]>(Error.NoEncontrado($"No existe el presupuesto {request.Id}."));

        var pdf = generador.Generar(presupuesto);
        return Result.Ok(pdf);
    }
}
