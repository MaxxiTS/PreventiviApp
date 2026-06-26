using MediatR;
using Microsoft.AspNetCore.Mvc;
using PreventiviApp.Application.Clientes;

namespace PreventiviApp.Api.Controllers;

[Route("api/clientes")]
public sealed class ClientesController(ISender sender) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(CrearClienteCommand comando, CancellationToken ct)
        => Responder(await sender.Send(comando, ct));

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await sender.Send(new ListarClientesQuery(), ct));
}
