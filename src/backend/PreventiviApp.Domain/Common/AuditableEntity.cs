namespace PreventiviApp.Domain.Common;

/// <summary>
/// Entidad con metadatos de auditoría y control de sincronización (offline-first).
/// Las columnas <c>actualizado_en</c>/<c>version</c> alimentan la resolución de
/// conflictos del change log (ver docs/10-sincronizacion-offline.md).
/// </summary>
public abstract class AuditableEntity : Entity
{
    protected AuditableEntity(Guid id) : base(id) { }

    public DateTimeOffset CreadoEn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ActualizadoEn { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Contador optimista de versión para sincronización.</summary>
    public long Version { get; set; }

    /// <summary>Borrado lógico (soft delete); nunca se borra físicamente para sync.</summary>
    public bool Eliminado { get; set; }
}
