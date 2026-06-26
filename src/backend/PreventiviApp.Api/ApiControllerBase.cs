using Microsoft.AspNetCore.Mvc;
using PreventiviApp.Domain.Common;

namespace PreventiviApp.Api;

/// <summary>
/// Controlador base que traduce el <see cref="Result"/>/<see cref="Result{T}"/> del
/// dominio a respuestas HTTP (200/404/409/400) con ProblemDetails.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Responder<T>(Result<T> result)
        => result.Exito ? Ok(result.Valor) : Problema(result.Error);

    protected IActionResult Problema(Error error)
    {
        var estado = error.Codigo switch
        {
            "dominio.no_encontrado" => StatusCodes.Status404NotFound,
            "dominio.conflicto" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        return Problem(detail: error.Mensaje, statusCode: estado, title: error.Codigo);
    }
}
