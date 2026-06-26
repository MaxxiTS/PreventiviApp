using MediatR;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;

namespace PreventiviApp.Application.Presupuestos;

public sealed record LineaComparacionDto(
    string Codigo,
    string Estado,
    decimal? PrecioA, decimal? PrecioB,
    decimal? MedicionA, decimal? MedicionB,
    decimal? ImporteA, decimal? ImporteB,
    decimal DeltaImporte);

public sealed record ComparacionDto(decimal DeltaTotal, IReadOnlyList<LineaComparacionDto> Lineas);

/// <summary>Compara dos versiones de presupuesto por código de partida (doc 12 §7).</summary>
public sealed record CompararVersionesQuery(Guid PresupuestoAId, Guid PresupuestoBId)
    : IRequest<Result<ComparacionDto>>;

internal sealed class CompararVersionesHandler(
    IPresupuestoRepository presupuestos,
    CalculadoraPresupuesto calculadora,
    EvaluadorMedicion mediciones,
    ComparadorPresupuestos comparador)
    : IRequestHandler<CompararVersionesQuery, Result<ComparacionDto>>
{
    public async Task<Result<ComparacionDto>> Handle(CompararVersionesQuery request, CancellationToken cancellationToken)
    {
        var a = await presupuestos.ObtenerArbolAsync(request.PresupuestoAId, cancellationToken);
        if (a is null)
            return Result.Fallo<ComparacionDto>(Error.NoEncontrado($"No existe el presupuesto {request.PresupuestoAId}."));

        var b = await presupuestos.ObtenerArbolAsync(request.PresupuestoBId, cancellationToken);
        if (b is null)
            return Result.Fallo<ComparacionDto>(Error.NoEncontrado($"No existe el presupuesto {request.PresupuestoBId}."));

        var resultado = comparador.Comparar(Aplanar(a), Aplanar(b));

        var lineas = resultado.Lineas
            .Select(l => new LineaComparacionDto(
                l.Codigo, l.Estado.ToString(),
                l.PrecioA, l.PrecioB, l.MedicionA, l.MedicionB, l.ImporteA, l.ImporteB, l.DeltaImporte))
            .ToList();

        return Result.Ok(new ComparacionDto(resultado.DeltaTotal, lineas));

        IReadOnlyList<PartidaSnapshot> Aplanar(Presupuesto presupuesto)
        {
            var snapshots = new List<PartidaSnapshot>();

            void Recorrer(CapituloPresupuesto capitulo)
            {
                foreach (var partida in capitulo.Partidas)
                    snapshots.Add(new PartidaSnapshot(
                        partida.Codigo,
                        partida.Precio,
                        mediciones.CalcularTotal(partida.Medicion),
                        calculadora.CalcularImportePartida(partida)));

                foreach (var subcapitulo in capitulo.Subcapitulos)
                    Recorrer(subcapitulo);
            }

            foreach (var raiz in presupuesto.CapitulosRaiz)
                Recorrer(raiz);

            return snapshots;
        }
    }
}
