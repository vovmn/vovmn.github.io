using Immunitas.Domain.Entities.Samples;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Immunitas.Persistence.Configs.Samples;

internal class SampleConfig : IEntityTypeConfiguration<Sample>
{
    public void Configure(EntityTypeBuilder<Sample> builder)
    {
        builder.HasOne(s => s.Patient)
            .WithMany(p => p.Samples)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new {s.PatientId, s.Barcode})
            .IsUnique();

        builder.HasMany(s => s.CytometerMeasurements)
            .WithOne(cm => cm.Sample)
            .HasForeignKey(cm => cm.SampleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.SampleDrugs)
            .WithOne(sd => sd.Sample)
            .HasForeignKey(sd => sd.SampleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
