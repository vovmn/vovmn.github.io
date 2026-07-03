namespace Immunitas.Domain.Repositories;

/// <summary>
/// Интерфейс для сохранения изменений в базе данных.
/// </summary>
public interface IChangesSaver
{
    /// <summary>
    /// Сохраняет изменения в базе данных асинхронно.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
