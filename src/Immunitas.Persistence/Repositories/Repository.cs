using Immunitas.Domain.Entities;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq.Expressions;

namespace Immunitas.Persistence.Repositories
{
    /// <summary>
    /// Базовая реализация репозитория для работы с сущностями через Entity Framework Core.
    /// Предоставляет CRUD-операции и реализует интерфейс <see cref="IQueryable{T}"/> для построения LINQ-запросов.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности домена.</typeparam>
    /// <typeparam name="TId">Тип уникального идентификатора сущности.</typeparam>
    public class Repository<TEntity, TId> : IRepository<TEntity, TId>
        where TEntity : class, IEntityWithId<TId>
        where TId : IEquatable<TId>
    {
        internal AppDbContext context { get; set; }
        internal IQueryable<TEntity> sourceQuery { get; set; }

        /// <inheritdoc />
        public Repository(AppDbContext context)
        {
            this.context = context;
            sourceQuery = context.Set<TEntity>().AsQueryable();
        }

        /// <inheritdoc />
        public Type ElementType => sourceQuery.ElementType;

        /// <inheritdoc />
        public Expression Expression => sourceQuery.Expression;

        /// <inheritdoc />
        public IQueryProvider Provider => sourceQuery.Provider;

        /// <inheritdoc />
        public IEnumerator<TEntity> GetEnumerator()
        {
            return sourceQuery.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IQueryable<TEntity> GetById(TId id)
        {
            return sourceQuery.Where(x => x.Id.Equals(id));
        }

        /// <inheritdoc />
        public IQueryable<TEntity> GetByIds(IEnumerable<TId> ids) =>
            sourceQuery.Where(o => ids.Contains(o.Id));

        /// <inheritdoc />
        public void Add(TEntity entity)
        {
            context.Add(entity);
        }

        /// <inheritdoc />
        public void AddRange(IEnumerable<TEntity> entities)
        {
            context.AddRange(entities);
        }

        /// <inheritdoc />
        public void Remove(TId instanceId)
        {
            var entity = GetById(instanceId).FirstOrDefault();
            if (entity is null)
                return;
            context.Set<TEntity>().Remove(entity);
        }

        /// <inheritdoc />
        public void Remove(TEntity entity)
        {
            context.Remove(entity);
        }

        /// <inheritdoc />
        public async Task RemoveAsync(TId instanceId, CancellationToken cancellationToken = default)
        {
            var entity = await GetById(instanceId).FirstOrDefaultAsync(cancellationToken);
            if (entity is null)
                return;
            context.Set<TEntity>().Remove(entity);
        }

        /// <inheritdoc />
        public void RemoveRange(IEnumerable<TId> ids)
        {
            var list = GetByIds(ids).ToList();
            context.Set<TEntity>().RemoveRange(list);
        }

        /// <inheritdoc />
        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            context.RemoveRange(entities);
        }

        /// <inheritdoc />
        public async Task RemoveRangeAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
        {
            var listAsync = await GetByIds(ids).ToListAsync(cancellationToken);
            context.Set<TEntity>().RemoveRange(listAsync);
        }
    }

    /// <summary>
    /// Специализированная реализация репозитория для сущностей с идентификатором типа <see cref="int"/>.
    /// Наследует всю функциональность от <see cref="Repository{TEntity, TId}"/> с указанием типа ID как Guid.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности домена с идентификатором типа <see cref="int"/>.</typeparam>
    public class Repository<TEntity>(AppDbContext appDbContext)
        : Repository<TEntity, int>(appDbContext), IRepository<TEntity>
        where TEntity : class, IEntityWithId<int>;
}