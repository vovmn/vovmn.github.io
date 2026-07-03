using Immunitas.Domain.Entities;

namespace Immunitas.Domain.Repositories
{
    /// <summary>
    /// Предоставляет базовый интерфейс репозитория для операций CRUD (Create, Read, Update, Delete) с сущностями.
    /// Наследует от <see cref="IQueryable{T}"/>, что позволяет строить запросы к данным на основе этого интерфейса.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности домена, с которой работает репозиторий. Должен быть классом и реализовывать <see cref="IEntityWithId{TId}"/>.</typeparam>
    /// <typeparam name="TId">Тип уникального идентификатора сущности. Должен реализовывать <see cref="IEquatable{T}"/> для корректного сравнения.</typeparam>
    public interface IRepository<TEntity, TId> : IQueryable<TEntity>
        where TEntity : class, IEntityWithId<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Возвращает запрос <see cref="IQueryable{T}"/> для сущности с указанным идентификатором.
        /// </summary>
        /// <param name="id">Уникальный идентификатор сущности.</param>
        /// <returns>Запрос <see cref="IQueryable{TEntity}"/>, который при выполнении вернет одну сущность или null, если сущность не найдена.</returns>
        /// <remarks>
        /// Метод возвращает запрос, а не результат, что позволяет дополнительно его настроить (например, добавить включение связанных данных)
        /// перед выполнением на сервере БД.
        /// </remarks>
        IQueryable<TEntity> GetById(TId id);

        /// <summary>
        /// Возвращает запрос <see cref="IQueryable{T}"/> для сущностей, идентификаторы которых присутствуют в предоставленной коллекции.
        /// </summary>
        /// <param name="ids">Коллекция уникальных идентификаторов сущностей.</param>
        /// <returns>Запрос <see cref="IQueryable{TEntity}"/>, который при выполнении вернет коллекцию найденных сущностей.</returns>
        IQueryable<TEntity> GetByIds(IEnumerable<TId> ids);

        /// <summary>
        /// Добавляет новую сущность в контекст данных для последующего сохранения.
        /// </summary>
        /// <param name="entity">Добавляемая сущность.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если <paramref name="entity"/> является null.</exception>
        void Add(TEntity entity);

        /// <summary>
        /// Добавляет коллекцию новых сущностей в контекст данных для последующего сохранения.
        /// </summary>
        /// <param name="entities">Коллекция добавляемых сущностей.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если <paramref name="entities"/> является null.</exception>
        void AddRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Помечает сущность с указанным идентификатором на удаление в контексте данных.
        /// Для асинхронного удаления используйте <see cref="RemoveAsync(TId, CancellationToken)"/>.
        /// </summary>
        /// <param name="id">Уникальный идентификатор сущности, которую необходимо удалить.</param>
        /// <exception cref="ArgumentException">Вызывается, если сущность с указанным <paramref name="id"/> не найдена.</exception>
        void Remove(TId id);

        /// <summary>
        /// Помечает предоставленную сущность на удаление в контексте данных.
        /// </summary>
        /// <param name="entity">Сущность, которую необходимо удалить.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если <paramref name="entity"/> является null.</exception>
        void Remove(TEntity entity);

        /// <summary>
        /// Асинхронно помечает сущность с указанным идентификатором на удаление в контексте данных.
        /// </summary>
        /// <param name="id">Уникальный идентификатор сущности, которую необходимо удалить.</param>
        /// <param name="cancellationToken">Токен для отслеживания запросов отмены. Значение по умолчанию - default.</param>
        /// <returns>Задача, представляющая асинхронную операцию удаления.</returns>
        /// <exception cref="ArgumentException">Вызывается, если сущность с указанным <paramref name="id"/> не найдена.</exception>
        Task RemoveAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Помечает набор сущностей с указанными идентификаторами на удаление в контексте данных.
        /// Для асинхронного удаления используйте <see cref="RemoveRangeAsync(IEnumerable{TId}, CancellationToken)"/>.
        /// </summary>
        /// <param name="ids">Коллекция уникальных идентификаторов сущностей, которые необходимо удалить.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если <paramref name="ids"/> является null.</exception>
        void RemoveRange(IEnumerable<TId> ids);

        /// <summary>
        /// Помечает предоставленную коллекцию сущностей на удаление в контексте данных.
        /// </summary>
        /// <param name="entities">Коллекция сущностей, которые необходимо удалить.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если <paramref name="entities"/> является null.</exception>
        void RemoveRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Асинхронно помечает набор сущностей с указанными идентификаторами на удаление в контексте данных.
        /// </summary>
        /// <param name="ids">Коллекция уникальных идентификаторов сущностей, которые необходимо удалить.</param>
        /// <param name="cancellationToken">Токен для отслеживания запросов отмены. Значение по умолчанию - default.</param>
        /// <returns>Задача, представляющая асинхронную операцию удаления.</returns>
        /// <exception cref="ArgumentNullException">Вызывается, если <paramref name="ids"/> является null.</exception>
        Task RemoveRangeAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Специализированный интерфейс репозитория для сущностей, использующих <see cref="int"/> в качестве типа идентификатора.
    /// Упрощает сигнатуры методов, избавляя от необходимости указывать тип ID.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности домена, с которой работает репозиторий. Должен быть классом и реализовывать <see cref="IEntityWithId{int}"/>.</typeparam>
    public interface IRepository<TEntity> : IRepository<TEntity, int>
        where TEntity : class, IEntityWithId<int>
    {
        // Наследует все методы из IRepository<TEntity, int>
    }
}