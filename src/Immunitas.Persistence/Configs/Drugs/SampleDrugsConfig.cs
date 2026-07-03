using Immunitas.Domain.Entities.Drugs;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Persistence.Configs.Drugs;

internal class SampleDrugsConfig : IEntityTypeConfiguration<SampleDrugs>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<SampleDrugs> builder)
    {
        builder.HasOne(sd => sd.Sample)
            .WithMany()
            .HasForeignKey(sd => sd.SampleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sd => sd.MedicalDrug)
            .WithMany()
            .HasForeignKey(sd => sd.MedicalDrugId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
