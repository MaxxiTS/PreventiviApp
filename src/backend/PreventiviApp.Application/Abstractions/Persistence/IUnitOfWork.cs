namespace PreventiviApp.Application.Abstractions.Persistence;

/// <summary>Confirma los cambios pendientes de la unidad de trabajo en la persistencia.</summary>
public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
