using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Immunitas.Persistence
{
    /// <summary>
    /// Помощник для осуществления удаления и создания БД, и применения миграций к БД
    /// </summary>
    public static class MigrationManager
    {
        public static WebApplication MigrateDatabase(this WebApplication webApp)
        {
            using (var scope = webApp.Services.CreateScope())
            {
                using var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Удаляет БД
                appContext.Database.EnsureDeleted();

                // Создает и применяет миграции
                appContext.Database.Migrate();
            }
            return webApp;
        }
    }
}
