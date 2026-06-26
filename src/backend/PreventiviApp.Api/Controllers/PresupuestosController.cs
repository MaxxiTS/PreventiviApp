using MediatR;
using Microsoft.AspNetCore.Mvc;
using PreventiviApp.Application.Presupuestos;

namespace PreventiviApp.Api.Controllers;

[Route("api/presupuestos")]
public sealed class PresupuestosController(ISender sender) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(CrearPresupuestoCommand comando, CancellationToken ct)
        => Responder(await sender.Send(comando, ct));

    /// <summary>Listar presupuestos de un proyecto (con total calculado).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] Guid proyectoId, CancellationToken ct)
        => Ok(await sender.Send(new ListarPresupuestosPorProyectoQuery(proyectoId), ct));

    /// <summary>Presupuesto completo con importes y totales calculados.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken ct)
        => Responder(await sender.Send(new ObtenerPresupuestoQuery(id), ct));

    [HttpPost("{id:guid}/capitulos")]
    public async Task<IActionResult> AnadirCapitulo(Guid id, [FromBody] AnadirCapituloRequest cuerpo, CancellationToken ct)
        => Responder(await sender.Send(
            new AnadirCapituloPresupuestoCommand(id, cuerpo.Codigo, cuerpo.Titulo, cuerpo.PadreId, cuerpo.Orden), ct));

    [HttpPost("capitulos/{capituloId:guid}/partidas")]
    public async Task<IActionResult> AnadirPartida(Guid capituloId, [FromBody] AnadirPartidaRequest cuerpo, CancellationToken ct)
        => Responder(await sender.Send(
            new AnadirPartidaPresupuestoCommand(
                capituloId, cuerpo.Codigo, cuerpo.Resumen, cuerpo.Precio, cuerpo.Lineas, cuerpo.PartidaOrigenId), ct));

    /// <summary>Genera el PDF del presupuesto.</summary>
    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken ct)
    {
        var resultado = await sender.Send(new GenerarPdfPresupuestoQuery(id), ct);
        return resultado.Exito
            ? File(resultado.Valor, "application/pdf", $"presupuesto-{id}.pdf")
            : Problema(resultado.Error);
    }

    /// <summary>Calcula los totales de un presupuesto sin persistirlo (a partir de su árbol).</summary>
    [HttpPost("calcular")]
    public async Task<IActionResult> Calcular(CalcularPresupuestoCommand comando, CancellationToken ct)
        => Responder(await sender.Send(comando, ct));

    public sealed record AnadirCapituloRequest(string Codigo, string Titulo, Guid? PadreId = null, int Orden = 1);

    public sealed record AnadirPartidaRequest(
        string Codigo,
        string Resumen,
        decimal Precio,
        IReadOnlyList<LineaMedicionDto> Lineas,
        Guid? PartidaOrigenId = null);
}
