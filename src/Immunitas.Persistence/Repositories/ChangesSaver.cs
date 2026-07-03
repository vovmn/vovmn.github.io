using Immunitas.Domain.Repositories;

namespace Immunitas.Persistence.Repositories;

/// <summary>
/// Имплементация интерфейса IChangesSaver для сохранения изменений в базе данных.
/// </summary>
/// <param name="context">DbContext приложения</param>
public class ChangesSaver(AppDbContext context) : IChangesSaver
{
    /// <summary>
    /// Сохраняет изменения в базе данных асинхронно, вызывая метод SaveChangesAsync у DbContext.
    /// </summary>
    public Task CommitAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
}
