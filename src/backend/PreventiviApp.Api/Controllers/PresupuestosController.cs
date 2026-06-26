using MediatR;
using Microsoft.AspNetCore.Mvc;
using PreventiviApp.Application.Presupuestos;

namespace PreventiviApp.Api.Controllers;

[Route("api/presupuestos")]
public sealed class PresupuestosController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// Calcula los totales de un presupuesto (PEM, baja, descuentos, IVA y total)
    /// a partir de su árbol de capítulos/partidas/mediciones. No persiste nada.
    /// </summary>
    [HttpPost("calcular")]
    public async Task<IActionResult> Calcular(CalcularPresupuestoCommand comando, CancellationToken ct)
        => Responder(await sender.Send(comando, ct));
}
