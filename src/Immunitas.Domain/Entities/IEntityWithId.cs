namespace Immunitas.Domain.Entities
{
    /// <summary>
    /// Интерфейс сущности с идентификатором
    /// </summary>
    /// <typeparam name="TId">Тип идентификатора</typeparam>
    public interface IEntityWithId<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// Id сущности
        /// </summary>
        public TId Id { get; set; }
    }

    /// <summary>
    /// Абстрактный класс для представления сущности с Id в виде <see cref="int"/>
    /// </summary>
    public abstract class EntityBase : IEntityWithId<int>
    {
        /// <inheritdoc/>
        public int Id { get; set; }
    }
}
