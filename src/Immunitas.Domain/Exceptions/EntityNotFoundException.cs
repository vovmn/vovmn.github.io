namespace Immunitas.Domain.Exceptions
{
    public class EntityNotFoundException(string entityName, object key) : Exception
    {
        public string EntityName { get; } = entityName;
        public object Key { get; } = key;
    }
}
