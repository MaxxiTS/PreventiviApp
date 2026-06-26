using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Abstractions.Importacion;

/// <summary>
/// Evento de progreso emitido durante la importación vía <see cref="IProgress{T}"/>.
/// </summary>
/// <param name="Porcentaje">Avance estimado, 0..100 (por bytes consumidos del stream).</param>
/// <param name="Fase">Nombre legible de la fase actual (lectura, parseo, mapeo...).</param>
/// <param name="RegistrosProcesados">Número de registros/líneas procesados hasta el momento.</param>
public sealed record ProgresoImportacion(int Porcentaje, string Fase, int RegistrosProcesados);

/// <summary>Severidad de una incidencia detectada durante la importación.</summary>
public enum SeveridadError
{
    /// <summary>La importación continúa; el registro se procesa de forma degradada o se omite.</summary>
    Advertencia,

    /// <summary>El registro afectado no se persiste; la importación continúa salvo error fatal.</summary>
    Error,
}

/// <summary>Incidencia (advertencia o error) registrada durante la importación.</summary>
/// <param name="Severidad">Nivel de la incidencia.</param>
/// <param name="Codigo">Código estable del tipo de incidencia (p. ej. "referencia_rota").</param>
/// <param name="Mensaje">Mensaje legible para el informe.</param>
/// <param name="Linea">Número de línea del archivo donde se detectó, si aplica.</param>
public sealed record ErrorImportacion(
    SeveridadError Severidad,
    string Codigo,
    string Mensaje,
    int? Linea = null);

/// <summary>
/// Grafo de un preciosario importado, en memoria. Reúne el agregado raíz
/// (<see cref="Preciosario"/>) y todas las entidades hijas resueltas por código.
/// NO persiste por sí mismo: lo consume el repositorio de persistencia.
/// </summary>
public sealed class PreciosarioImportado
{
    public PreciosarioImportado(
        Preciosario preciosario,
        IReadOnlyList<Unidad> unidades,
        IReadOnlyList<Capitulo> capitulos,
        IReadOnlyList<Recurso> recursos,
        IReadOnlyList<Partida> partidas,
        IReadOnlyList<Descompuesto> descompuestos,
        IReadOnlyList<Precio> precios)
    {
        Preciosario = preciosario;
        Unidades = unidades;
        Capitulos = capitulos;
        Recursos = recursos;
        Partidas = partidas;
        Descompuestos = descompuestos;
        Precios = precios;
    }

    /// <summary>Agregado raíz del preciosario importado.</summary>
    public Preciosario Preciosario { get; }

    public IReadOnlyList<Unidad> Unidades { get; }
    public IReadOnlyList<Capitulo> Capitulos { get; }
    public IReadOnlyList<Recurso> Recursos { get; }
    public IReadOnlyList<Partida> Partidas { get; }
    public IReadOnlyList<Descompuesto> Descompuestos { get; }
    public IReadOnlyList<Precio> Precios { get; }
}

/// <summary>Resumen agregado del resultado de una importación (para el informe final).</summary>
public sealed record ResumenImportacion(
    int Capitulos,
    int Partidas,
    int Recursos,
    int Descompuestos,
    int Precios,
    int Errores,
    int Advertencias);
