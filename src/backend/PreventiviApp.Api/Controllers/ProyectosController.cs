using MediatR;
using Microsoft.AspNetCore.Mvc;
using PreventiviApp.Application.Proyectos;
using PreventiviApp.Domain.Enums;

namespace PreventiviApp.Api.Controllers;

[Route("api/proyectos")]
public sealed class ProyectosController(ISender sender) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(CrearProyectoCommand comando, CancellationToken ct)
        => Responder(await sender.Send(comando, ct));

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await sender.Send(new ListarProyectosQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener(Guid id, CancellationToken ct)
        => Responder(await sender.Send(new ObtenerProyectoQuery(id), ct));

    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoRequest cuerpo, CancellationToken ct)
        => Responder(await sender.Send(new CambiarEstadoProyectoCommand(id, cuerpo.NuevoEstado), ct));

    public sealed record CambiarEstadoRequest(EstadoProyecto NuevoEstado);
}
