namespace PreventiviApp.Domain.Common;

/// <summary>
/// Raíz común de las entidades del dominio. Usa <see cref="Guid"/> (UUID v7 en
/// persistencia, ver ADR 0006) como identidad, lo que permite generar IDs en el
/// cliente sin coordinación con el servidor (offline-first).
/// </summary>
public abstract class Entity
{
    protected Entity(Guid id) => Id = id;

    /// <summary>Constructor para el materializador de EF Core. No usar en código de dominio.</summary>
    protected Entity() { }

    /// <summary>Identidad. Se asigna en el constructor; inmutable.</summary>
    public Guid Id { get; }

    public override bool Equals(object? obj)
        => obj is Entity other && other.GetType() == GetType() && other.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? a, Entity? b) => Equals(a, b);

    public static bool operator !=(Entity? a, Entity? b) => !Equals(a, b);
}
