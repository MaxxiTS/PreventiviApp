using FluentValidation;
using MediatR;
using PreventiviApp.Application.Abstractions.Importacion;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Common;

namespace PreventiviApp.Application.Preciosarios;

/// <summary>
/// Importa un preciosario desde un archivo DCF: parsea el contenido a un grafo
/// canónico y, si tiene éxito, lo persiste de forma transaccional.
/// </summary>
public sealed record ImportarDcfCommand(Stream Contenido, string NombreArchivo)
    : IRequest<Result<ResumenImportacion>>;

public sealed class ImportarDcfValidator : AbstractValidator<ImportarDcfCommand>
{
    public ImportarDcfValidator()
    {
        RuleFor(x => x.Contenido).NotNull();
        RuleFor(x => x.NombreArchivo).NotEmpty().MaximumLength(260);
    }
}

internal sealed class ImportarDcfHandler(
    IImportadorPreciosario importador,
    IPreciosarioRepository repositorio,
    IUnitOfWork unidadDeTrabajo)
    : IRequestHandler<ImportarDcfCommand, Result<ResumenImportacion>>
{
    public async Task<Result<ResumenImportacion>> Handle(
        ImportarDcfCommand request, CancellationToken cancellationToken)
    {
        var resultado = await importador.ImportarAsync(
            request.Contenido, request.NombreArchivo, progreso: null, cancellationToken);

        if (resultado.EsFallo)
            return Result.Fallo<ResumenImportacion>(resultado.Error);

        var grafo = resultado.Valor;

        await repositorio.AnadirPreciosarioAsync(grafo, cancellationToken);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        var errores = importador.Errores.Count(e => e.Severidad == SeveridadError.Error);
        var advertencias = importador.Errores.Count(e => e.Severidad == SeveridadError.Advertencia);

        var resumen = new ResumenImportacion(
            Capitulos: grafo.Capitulos.Count,
            Partidas: grafo.Partidas.Count,
            Recursos: grafo.Recursos.Count,
            Descompuestos: grafo.Descompuestos.Count,
            Precios: grafo.Precios.Count,
            Errores: errores,
            Advertencias: advertencias);

        return Result.Ok(resumen);
    }
}
