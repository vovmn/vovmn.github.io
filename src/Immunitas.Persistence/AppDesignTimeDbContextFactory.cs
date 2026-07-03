using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Persistence
{
    /// <summary>
    /// Используется для создания и применения миграция в Design-time (через командную строку)
    /// </summary>
    internal class AppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Укажите строку подключения для design-time
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Immunitas_Dev;User Id=postgres;Password=postgres;Trust Server Certificate=true");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
