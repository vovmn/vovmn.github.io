using Immunitas.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Immunitas.Persistence.Configs.Geography;

internal class AddressConfig : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasOne(a => a.Street)
            .WithMany()
            .HasForeignKey(a => a.StreetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
