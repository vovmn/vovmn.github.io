using Immunitas.Domain.Entities.Antigens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Immunitas.Persistence.Configs.Antigens;

public class OrganConfig : IEntityTypeConfiguration<Organ>
{
    public void Configure(EntityTypeBuilder<Organ> builder)
    {
        builder.HasMany(o => o.Antigens)
            .WithOne(a => a.Organ)
            .HasForeignKey(a => a.OrganId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ImmuneSystem)
            .WithMany(s => s.Organs)
            .HasForeignKey(o => o.ImmuneSystemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
