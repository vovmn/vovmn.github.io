using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
/// Предоставляет соглашение (Convention) для EF Core, которое автоматически настраивает
/// свойства первичного ключа типа <see cref="Guid"/> для генерации значений на стороне БД.
/// </summary>
public class GuidPrimaryKeyConvention : IEntityTypeAddedConvention
{
    public void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        var primaryKey = entityTypeBuilder.Metadata.FindPrimaryKey();
        if (primaryKey == null || primaryKey.Properties.Count != 1)
            return;

        var property = primaryKey.Properties[0];
        if (property.ClrType == typeof(Guid))
        {
            // Для PostgreSQL
            property.SetDefaultValueSql("gen_random_uuid()");
        }
    }
}