namespace PreventiviApp.Domain.Enums;

/// <summary>Estado del ciclo de vida de un proyecto.</summary>
public enum EstadoProyecto
{
    Borrador = 0,
    Activo = 1,
    Cerrado = 2,
    Archivado = 3,
}

/// <summary>Estado del ciclo de vida de un presupuesto.</summary>
public enum EstadoPresupuesto
{
    Borrador = 0,
    Aprobado = 1,
    Adjudicado = 2,
    Cerrado = 3,
}

/// <summary>Naturaleza de un recurso dentro del análisis de precios (descompuesto).</summary>
public enum TipoRecurso
{
    ManoObra = 0,
    Material = 1,
    Maquinaria = 2,
    Otros = 3,
}
