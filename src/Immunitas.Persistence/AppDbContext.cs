using Immunitas.Domain.Entities.Antigens;
using Immunitas.Domain.Entities.Cytometers;
using Immunitas.Domain.Entities.Measurements;
using Immunitas.Domain.Entities.Patients;
using Immunitas.Domain.Entities.Users;
using Immunitas.Domain.Entities.Laboratories;
using Immunitas.Domain.Entities.Samples;
using Immunitas.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Immunitas.Persistence;

/// <summary>
/// Контекст приложения для настройки БД
/// </summary>
public class AppDbContext : DbContext
{
    private readonly ISeedDataGenerator? seedDataGenerator;

    /// <param name="options">Опции</param>
    public AppDbContext(DbContextOptions<AppDbContext> options, ISeedDataGenerator seedDataGenerator) : base(options)
    {
        this.seedDataGenerator = seedDataGenerator;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
       
    }

    /// <summary>
    /// Пациенты
    /// </summary>
    public DbSet<Patient> Patients { get; set; }

    /// <summary>
    /// Пользователи
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Лаборатории
    /// </summary>
    public DbSet<Laboratory> Laboratories { get; set; }

    /// <summary>
    /// Проточные цитометры
    /// </summary>
    public DbSet<Cytometer> Cytometers { get; set; }

    /// <summary>
    /// Антигены
    /// </summary>
    public DbSet<Antigen> Antigens { get; set; }

    /// <summary>
    /// Образцы крови
    /// </summary>
    public DbSet<Sample> Samples { get; set; }

    /// <summary>
    /// Измерения цитометра
    /// </summary>
    public DbSet<CytometerMeasurement> CytometerMeasurements { get; set; }

    /// <summary>
    /// Адреса
    /// </summary>
    public DbSet<Address> Addresses { get; set; }

    /// <summary>
    /// Улицы
    /// </summary>
    public DbSet<Street> Streets { get; set; }

    /// <summary>
    /// Города
    /// </summary>
    public DbSet<City> Cities { get; set; }

    /// <summary>
    /// Страны
    /// </summary>
    public DbSet<Country> Countries { get; set; }

    /// <summary>
    /// Refresh-токены пользователей
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// Применяет настройки к сущностям
    /// </summary>
    /// <param name="modelBuilder">Построитель модели</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Применяем настройки к моделям
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Заполняет таблицы первичными тестовыми данными
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (seedDataGenerator is not null)
        {
            optionsBuilder.UseSeeding(seedDataGenerator.GenerateSeedData);
            optionsBuilder.UseAsyncSeeding(seedDataGenerator.GenerateSeedDataAsync);
        }
    }

    /// <summary>
    /// Настраивает соглашения
    /// </summary>
    /// <param name="configurationBuilder"></param>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new SnakeCaseNamingConvention());
    }
}