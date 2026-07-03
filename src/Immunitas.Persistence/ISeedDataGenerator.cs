using Microsoft.EntityFrameworkCore;

namespace Immunitas.Persistence;

public interface ISeedDataGenerator
{
    void GenerateSeedData(DbContext context, bool storageOperationsPerformed);
    Task GenerateSeedDataAsync(DbContext context, bool storageOperationsPerformed, CancellationToken cancellationToken);
}