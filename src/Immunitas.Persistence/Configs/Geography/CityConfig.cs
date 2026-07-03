using Immunitas.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Immunitas.Persistence.Configs.Geography;

internal class CityConfig : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasOne(c => c.Region)
            .WithMany()
            .HasForeignKey(c => c.RegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
