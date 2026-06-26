using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Abstractions.Documentos;

/// <summary>
/// Genera el documento PDF de un presupuesto a partir de su árbol de capítulos,
/// partidas y mediciones, aplicando el cierre económico del motor de dominio.
/// </summary>
public interface IGeneradorPresupuestoPdf
{
    /// <summary>Renderiza el presupuesto a un PDF en memoria.</summary>
    byte[] Generar(Presupuesto presupuesto);
}
