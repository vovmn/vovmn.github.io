using Immunitas.Domain.Entities.Measurements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NpgsqlTypes;

namespace Immunitas.Persistence.Configs.Measurements;

/// <summary>
/// Конфигурация сущности <see cref="CytometerMeasurement"/>
/// </summary>
internal class CytometerMeasurementConfig : IEntityTypeConfiguration<CytometerMeasurement>
{
    public void Configure(EntityTypeBuilder<CytometerMeasurement> builder)
    {
        // Распределение лейкоцитов по размерам
        builder.Property(e => e.WbcDistribution)
            .HasConversion<PointArrayConverter>()
            .HasColumnType("point[]")
            .IsRequired();

        // Распределение эритроцитов по размерам
        builder.Property(e => e.RbcDistribution)
            .HasConversion<PointArrayConverter>()
            .HasColumnType("point[]")
            .IsRequired();

        // Распределение тромбоцитов по размерам
        builder.Property(e => e.PltDistribution)
            .HasConversion<PointArrayConverter>()
            .HasColumnType("point[]")
            .IsRequired();

        builder.HasOne(cm => cm.Sample)
            .WithMany()
            .HasForeignKey(cm => cm.SampleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.Antigen)
            .WithMany()
            .HasForeignKey(cm => cm.AntigenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Конвертер списка объектов <see cref="Point"/> в понятный для Postgres - <see cref="NpgsqlPoint"/> и обратно
/// </summary>
public class PointArrayConverter : ValueConverter<Point[], NpgsqlPoint[]>
{
    public PointArrayConverter()
        : base(
            points => points == null
                ? Array.Empty<NpgsqlPoint>()
                : points.Select(p => new NpgsqlPoint(p.X, p.Y)).ToArray(),
            npgsqlPoints => npgsqlPoints == null
                ? Array.Empty<Point>()
                : npgsqlPoints.Select(p => new Point(p.X, p.Y)).ToArray())
    { }
}
