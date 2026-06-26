// =============================================================================
// Importador del formato de intercambio de preciosarios "DCF" (variante texto).
// =============================================================================
//
// El formato DCF real es propietario y no está publicado de forma estable. Este
// importador implementa una variante de INTERCAMBIO POR TEXTO, por líneas, simple
// y documentada, que cubre el subconjunto canónico del dominio y sirve de base
// extensible (patrón Strategy) a otros formatos como BC3/FIEBDC-3 en el futuro.
//
// Reglas generales:
//   - Codificación UTF-8. Una instrucción por línea.
//   - Campos separados por ';'. Espacios sobrantes en los extremos se recortan.
//   - Líneas vacías y líneas que empiezan por '#' (comentarios) se ignoran.
//   - Decimales en CultureInfo.InvariantCulture (punto como separador decimal).
//   - El primer campo es el DISCRIMINADOR de tipo de registro.
//   - Las relaciones se expresan por CÓDIGO y se resuelven con tablas de símbolos.
//   - Tolerante: las líneas mal formadas o con referencias rotas se registran
//     como incidencias y NO abortan la importación.
//
// Registros soportados:
//
//   PRECIOSARIO;nombre;version
//       Cabecera. Debe aparecer una vez (la primera gana; las siguientes => warning).
//
//   UNIDAD;codigo;nombre
//       Catálogo de unidades de medida.
//
//   CAPITULO;codigo;titulo;codigoPadre(opcional)
//       Capítulo del árbol. 'codigoPadre' referencia otro CAPITULO ya declarado.
//
//   RECURSO;tipo(MO/MT/MQ/OT);codigo;descripcion;unidad;precio
//       Recurso básico. 'unidad' referencia un UNIDAD por código (opcional).
//       Mapeo de tipo: MO->ManoObra, MT->Material, MQ->Maquinaria, OT->Otros.
//
//   PARTIDA;codigoCapitulo;codigo;resumen;unidad;precio
//       Unidad de obra. 'codigoCapitulo' referencia un CAPITULO. Se genera además
//       un registro Precio con el precio de la partida.
//
//   DESCOMPUESTO;codigoPartida;codigoRecurso;rendimiento
//       Línea de análisis de precios. Referencia una PARTIDA y un RECURSO; el
//       precio unitario se toma del recurso referenciado.
//
// Ejemplo: ver tools/dcf-importer/ejemplo.dcf
// =============================================================================

using System.Globalization;
using System.Security.Cryptography;
using PreventiviApp.Application.Abstractions.Importacion;
using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Enums;

namespace PreventiviApp.Infrastructure.Importacion.Dcf;

/// <summary>
/// Importador de preciosarios en formato de intercambio de texto "DCF". Parser por
/// líneas en streaming, tolerante a errores, con reporte de progreso y cancelación.
/// </summary>
public sealed class DcfImporter : IImportadorPreciosario
{
    private const char Separador = ';';

    /// <summary>Cada cuántas líneas se reporta progreso (evita ruido en archivos grandes).</summary>
    private const int IntervaloProgreso = 500;

    private readonly List<ErrorImportacion> _errores = new();

    public IReadOnlyList<ErrorImportacion> Errores => _errores;

    public async Task<Result<PreciosarioImportado>> ImportarAsync(
        Stream contenido,
        string nombreArchivo,
        IProgress<ProgresoImportacion>? progreso = null,
        CancellationToken cancellationToken = default)
    {
        _errores.Clear();

        if (contenido is null || (contenido.CanSeek && contenido.Length == 0))
            return Result.Fallo<PreciosarioImportado>(
                Error.Validacion("El archivo a importar está vacío o no es legible."));

        // Copiamos a memoria para poder leer dos veces (hash + parseo) de forma
        // robusta aunque el stream de origen no admita reposicionamiento.
        await using var buffer = new MemoryStream();
        try
        {
            await contenido.CopyToAsync(buffer, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result.Fallo<PreciosarioImportado>(
                Error.Validacion($"No se pudo leer el archivo: {ex.Message}"));
        }

        if (buffer.Length == 0)
            return Result.Fallo<PreciosarioImportado>(
                Error.Validacion("El archivo a importar está vacío."));

        progreso?.Report(new ProgresoImportacion(0, "Lectura", 0));

        var hash = CalcularHash(buffer);
        buffer.Position = 0;

        var contexto = new ContextoImportacion(Guid.NewGuid());

        long totalBytes = buffer.Length;
        int numeroLinea = 0;
        int registrosProcesados = 0;
        bool cabeceraLeida = false;

        using var lector = new StreamReader(buffer, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? linea;
        while ((linea = await lector.ReadLineAsync(cancellationToken)) is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            numeroLinea++;

            var contenidoLinea = linea.Trim();
            if (contenidoLinea.Length == 0 || contenidoLinea.StartsWith('#'))
                continue;

            var campos = SepararCampos(contenidoLinea);
            var discriminador = campos[0].ToUpperInvariant();

            switch (discriminador)
            {
                case "PRECIOSARIO":
                    ProcesarPreciosario(contexto, campos, numeroLinea, ref cabeceraLeida);
                    break;
                case "UNIDAD":
                    ProcesarUnidad(contexto, campos, numeroLinea);
                    break;
                case "CAPITULO":
                    ProcesarCapitulo(contexto, campos, numeroLinea);
                    break;
                case "RECURSO":
                    ProcesarRecurso(contexto, campos, numeroLinea);
                    break;
                case "PARTIDA":
                    ProcesarPartida(contexto, campos, numeroLinea);
                    break;
                case "DESCOMPUESTO":
                    ProcesarDescompuesto(contexto, campos, numeroLinea);
                    break;
                default:
                    RegistrarAdvertencia("registro_desconocido",
                        $"Tipo de registro desconocido '{campos[0]}'; se ignora la línea.", numeroLinea);
                    break;
            }

            registrosProcesados++;

            if (registrosProcesados % IntervaloProgreso == 0)
                progreso?.Report(new ProgresoImportacion(
                    CalcularPorcentaje(buffer.Position, totalBytes), "Parseo", registrosProcesados));
        }

        if (contexto.Preciosario is null)
            return Result.Fallo<PreciosarioImportado>(
                Error.Validacion("El archivo no contiene una cabecera PRECIOSARIO válida."));

        contexto.Preciosario.OrigenDcf = nombreArchivo;
        contexto.Preciosario.HashArchivo = hash;

        progreso?.Report(new ProgresoImportacion(100, "Completado", registrosProcesados));

        var grafo = new PreciosarioImportado(
            contexto.Preciosario,
            contexto.Unidades,
            contexto.Capitulos,
            contexto.Recursos,
            contexto.Partidas,
            contexto.Descompuestos,
            contexto.Precios);

        return Result.Ok(grafo);
    }

    // ---------------------------------------------------------------------
    // Procesadores de registro
    // ---------------------------------------------------------------------

    private void ProcesarPreciosario(
        ContextoImportacion ctx, string[] campos, int linea, ref bool cabeceraLeida)
    {
        if (cabeceraLeida)
        {
            RegistrarAdvertencia("cabecera_duplicada",
                "Ya existe una cabecera PRECIOSARIO; se conserva la primera.", linea);
            return;
        }

        if (campos.Length < 3 || string.IsNullOrWhiteSpace(campos[1]))
        {
            RegistrarError("dato_faltante",
                "Cabecera PRECIOSARIO mal formada; se requiere nombre y versión.", linea);
            return;
        }

        var version = campos.Length > 2 ? campos[2] : string.Empty;
        ctx.Preciosario = new Preciosario(ctx.PreciosarioId, campos[1], version)
        {
            Fuente = "DCF",
        };
        cabeceraLeida = true;
    }

    private void ProcesarUnidad(ContextoImportacion ctx, string[] campos, int linea)
    {
        if (campos.Length < 3 || string.IsNullOrWhiteSpace(campos[1]))
        {
            RegistrarError("dato_faltante", "UNIDAD mal formada; se requiere código y nombre.", linea);
            return;
        }

        var codigo = campos[1];
        if (ctx.UnidadesPorCodigo.ContainsKey(codigo))
        {
            RegistrarAdvertencia("duplicado_codigo",
                $"Unidad con código '{codigo}' duplicada; se conserva la primera.", linea);
            return;
        }

        var unidad = new Unidad(Guid.NewGuid(), codigo, campos[2]);
        ctx.Unidades.Add(unidad);
        ctx.UnidadesPorCodigo[codigo] = unidad.Id;
    }

    private void ProcesarCapitulo(ContextoImportacion ctx, string[] campos, int linea)
    {
        if (campos.Length < 3 || string.IsNullOrWhiteSpace(campos[1]))
        {
            RegistrarError("dato_faltante", "CAPITULO mal formado; se requiere código y título.", linea);
            return;
        }

        if (ctx.Preciosario is null)
        {
            RegistrarError("orden_invalido",
                "CAPITULO declarado antes de la cabecera PRECIOSARIO; se descarta.", linea);
            return;
        }

        var codigo = campos[1];
        if (ctx.CapitulosPorCodigo.ContainsKey(codigo))
        {
            RegistrarAdvertencia("duplicado_codigo",
                $"Capítulo con código '{codigo}' duplicado; se conserva el primero.", linea);
            return;
        }

        Guid? padreId = null;
        var codigoPadre = campos.Length > 3 ? campos[3].Trim() : string.Empty;
        if (codigoPadre.Length > 0)
        {
            if (ctx.CapitulosPorCodigo.TryGetValue(codigoPadre, out var idPadre))
            {
                padreId = idPadre;
            }
            else
            {
                RegistrarError("referencia_rota",
                    $"Capítulo padre '{codigoPadre}' no encontrado para el capítulo '{codigo}'.", linea);
            }
        }

        var orden = ctx.Capitulos.Count + 1;
        var capitulo = new Capitulo(Guid.NewGuid(), ctx.PreciosarioId, codigo, campos[2], orden, padreId);
        ctx.Capitulos.Add(capitulo);
        ctx.CapitulosPorCodigo[codigo] = capitulo.Id;
    }

    private void ProcesarRecurso(ContextoImportacion ctx, string[] campos, int linea)
    {
        // RECURSO;tipo;codigo;descripcion;unidad;precio
        if (campos.Length < 6 || string.IsNullOrWhiteSpace(campos[2]))
        {
            RegistrarError("dato_faltante",
                "RECURSO mal formado; se requieren tipo, código, descripción, unidad y precio.", linea);
            return;
        }

        if (ctx.Preciosario is null)
        {
            RegistrarError("orden_invalido",
                "RECURSO declarado antes de la cabecera PRECIOSARIO; se descarta.", linea);
            return;
        }

        if (!TryMapearTipo(campos[1], out var tipo))
        {
            RegistrarError("dato_faltante",
                $"Tipo de recurso '{campos[1]}' no reconocido (use MO/MT/MQ/OT).", linea);
            return;
        }

        var codigo = campos[2];
        if (ctx.RecursosPorCodigo.ContainsKey(codigo))
        {
            RegistrarAdvertencia("duplicado_codigo",
                $"Recurso con código '{codigo}' duplicado; se conserva el primero.", linea);
            return;
        }

        var unidadId = ResolverUnidad(ctx, campos[4], codigo, linea);

        if (!TryParsearDecimal(campos[5], out var precio))
        {
            RegistrarError("precio_invalido",
                $"Precio inválido '{campos[5]}' en el recurso '{codigo}'.", linea);
            return;
        }

        var recurso = new Recurso(Guid.NewGuid(), ctx.PreciosarioId, tipo, codigo, campos[3], precio, unidadId);
        ctx.Recursos.Add(recurso);
        ctx.RecursosPorCodigo[codigo] = recurso.Id;
        ctx.PreciosRecursoPorCodigo[codigo] = precio;
        ctx.TiposRecursoPorCodigo[codigo] = tipo;
    }

    private void ProcesarPartida(ContextoImportacion ctx, string[] campos, int linea)
    {
        // PARTIDA;codigoCapitulo;codigo;resumen;unidad;precio
        if (campos.Length < 6 || string.IsNullOrWhiteSpace(campos[2]))
        {
            RegistrarError("dato_faltante",
                "PARTIDA mal formada; se requieren capítulo, código, resumen, unidad y precio.", linea);
            return;
        }

        if (ctx.Preciosario is null)
        {
            RegistrarError("orden_invalido",
                "PARTIDA declarada antes de la cabecera PRECIOSARIO; se descarta.", linea);
            return;
        }

        if (!ctx.CapitulosPorCodigo.TryGetValue(campos[1], out var capituloId))
        {
            RegistrarError("referencia_rota",
                $"Capítulo '{campos[1]}' no encontrado para la partida '{campos[2]}'.", linea);
            return;
        }

        var codigo = campos[2];
        if (ctx.PartidasPorCodigo.ContainsKey(codigo))
        {
            RegistrarAdvertencia("duplicado_codigo",
                $"Partida con código '{codigo}' duplicada; se conserva la primera.", linea);
            return;
        }

        var unidadId = ResolverUnidad(ctx, campos[4], codigo, linea);

        if (!TryParsearDecimal(campos[5], out var precio))
        {
            RegistrarError("precio_invalido",
                $"Precio inválido '{campos[5]}' en la partida '{codigo}'.", linea);
            return;
        }

        var partida = new Partida(Guid.NewGuid(), capituloId, codigo, campos[3], precio, unidadId);
        ctx.Partidas.Add(partida);
        ctx.PartidasPorCodigo[codigo] = partida.Id;

        // Cada partida genera su Precio canónico.
        ctx.Precios.Add(new Precio(Guid.NewGuid(), partida.Id, precio));
    }

    private void ProcesarDescompuesto(ContextoImportacion ctx, string[] campos, int linea)
    {
        // DESCOMPUESTO;codigoPartida;codigoRecurso;rendimiento
        if (campos.Length < 4)
        {
            RegistrarError("dato_faltante",
                "DESCOMPUESTO mal formado; se requieren partida, recurso y rendimiento.", linea);
            return;
        }

        if (!ctx.PartidasPorCodigo.TryGetValue(campos[1], out var partidaId))
        {
            RegistrarError("referencia_rota",
                $"Partida '{campos[1]}' no encontrada para el descompuesto.", linea);
            return;
        }

        if (!ctx.RecursosPorCodigo.TryGetValue(campos[2], out var recursoId))
        {
            RegistrarError("referencia_rota",
                $"Recurso '{campos[2]}' no encontrado para el descompuesto de la partida '{campos[1]}'.", linea);
            return;
        }

        if (!TryParsearDecimal(campos[3], out var rendimiento))
        {
            RegistrarError("precio_invalido",
                $"Rendimiento inválido '{campos[3]}' en el descompuesto de la partida '{campos[1]}'.", linea);
            return;
        }

        var precioUnitario = ctx.PreciosRecursoPorCodigo[campos[2]];
        var tipo = ctx.TiposRecursoPorCodigo[campos[2]];

        ctx.Descompuestos.Add(new Descompuesto(
            Guid.NewGuid(), partidaId, recursoId, tipo, rendimiento, precioUnitario));
    }

    // ---------------------------------------------------------------------
    // Utilidades
    // ---------------------------------------------------------------------

    private Guid? ResolverUnidad(ContextoImportacion ctx, string codigoUnidad, string codigoEntidad, int linea)
    {
        var codigo = codigoUnidad.Trim();
        if (codigo.Length == 0)
            return null;

        if (ctx.UnidadesPorCodigo.TryGetValue(codigo, out var unidadId))
            return unidadId;

        RegistrarAdvertencia("referencia_rota",
            $"Unidad '{codigo}' no encontrada para '{codigoEntidad}'; se deja sin unidad.", linea);
        return null;
    }

    private static string[] SepararCampos(string linea)
    {
        var partes = linea.Split(Separador);
        for (var i = 0; i < partes.Length; i++)
            partes[i] = partes[i].Trim();
        return partes;
    }

    private static bool TryMapearTipo(string codigo, out TipoRecurso tipo)
    {
        tipo = codigo.Trim().ToUpperInvariant() switch
        {
            "MO" => TipoRecurso.ManoObra,
            "MT" => TipoRecurso.Material,
            "MQ" => TipoRecurso.Maquinaria,
            "OT" => TipoRecurso.Otros,
            _ => (TipoRecurso)(-1),
        };
        return Enum.IsDefined(typeof(TipoRecurso), tipo);
    }

    private static bool TryParsearDecimal(string valor, out decimal resultado)
        => decimal.TryParse(
            valor.Trim(),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out resultado);

    private static int CalcularPorcentaje(long consumidos, long total)
    {
        if (total <= 0)
            return 0;
        var pct = (int)(consumidos * 100 / total);
        return pct switch
        {
            < 0 => 0,
            > 100 => 100,
            _ => pct,
        };
    }

    private static string CalcularHash(MemoryStream buffer)
    {
        buffer.Position = 0;
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(buffer);
        return Convert.ToHexStringLower(bytes);
    }

    private void RegistrarAdvertencia(string codigo, string mensaje, int linea)
        => _errores.Add(new ErrorImportacion(SeveridadError.Advertencia, codigo, mensaje, linea));

    private void RegistrarError(string codigo, string mensaje, int linea)
        => _errores.Add(new ErrorImportacion(SeveridadError.Error, codigo, mensaje, linea));

    // ---------------------------------------------------------------------
    // Estado de una importación (tablas de símbolos y buffers en memoria)
    // ---------------------------------------------------------------------

    private sealed class ContextoImportacion
    {
        public ContextoImportacion(Guid preciosarioId) => PreciosarioId = preciosarioId;

        public Guid PreciosarioId { get; }
        public Preciosario? Preciosario { get; set; }

        public List<Unidad> Unidades { get; } = new();
        public List<Capitulo> Capitulos { get; } = new();
        public List<Recurso> Recursos { get; } = new();
        public List<Partida> Partidas { get; } = new();
        public List<Descompuesto> Descompuestos { get; } = new();
        public List<Precio> Precios { get; } = new();

        public Dictionary<string, Guid> UnidadesPorCodigo { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Guid> CapitulosPorCodigo { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Guid> RecursosPorCodigo { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, Guid> PartidasPorCodigo { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, decimal> PreciosRecursoPorCodigo { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, TipoRecurso> TiposRecursoPorCodigo { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
