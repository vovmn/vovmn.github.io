using System;
using Immunitas.Domain.Entities.Surveys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Immunitas.Persistence.Configs.Surveys;

public class SurveyConfig : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.HasOne(s => s.Laboratory)
            .WithMany()
            .HasForeignKey(s => s.LaboratoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Patient)
            .WithMany()
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Cytometer)
            .WithMany()
            .HasForeignKey(s => s.CytometerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
