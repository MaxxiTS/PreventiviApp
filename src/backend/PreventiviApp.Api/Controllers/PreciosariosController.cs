using MediatR;
using Microsoft.AspNetCore.Mvc;
using PreventiviApp.Application.Preciosarios;

namespace PreventiviApp.Api.Controllers;

[Route("api/preciosarios")]
public sealed class PreciosariosController(ISender sender) : ApiControllerBase
{
    /// <summary>Importa un preciosario desde un archivo DCF (multipart/form-data).</summary>
    [HttpPost("importar")]
    public async Task<IActionResult> Importar(IFormFile archivo, CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest("Debe adjuntar un archivo no vacío.");

        await using var stream = archivo.OpenReadStream();
        var resultado = await sender.Send(new ImportarDcfCommand(stream, archivo.FileName), ct);
        return Responder(resultado);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await sender.Send(new ListarPreciosariosQuery(), ct));

    /// <summary>Capítulos del preciosario. Sin <c>padreId</c> devuelve las raíces.</summary>
    [HttpGet("{preciosarioId:guid}/capitulos")]
    public async Task<IActionResult> Capitulos(Guid preciosarioId, [FromQuery] Guid? padreId, CancellationToken ct)
        => Ok(await sender.Send(new ListarCapitulosQuery(preciosarioId, padreId), ct));

    [HttpGet("capitulos/{capituloId:guid}/partidas")]
    public async Task<IActionResult> Partidas(Guid capituloId, CancellationToken ct)
        => Ok(await sender.Send(new ListarPartidasQuery(capituloId), ct));

    /// <summary>Análisis de precios (descompuesto) de una partida.</summary>
    [HttpGet("partidas/{partidaId:guid}/analisis")]
    public async Task<IActionResult> Analisis(Guid partidaId, CancellationToken ct)
        => Responder(await sender.Send(new ObtenerAnalisisPreciosQuery(partidaId), ct));
}
