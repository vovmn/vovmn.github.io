using Immunitas.Domain.Entities.Laboratories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Immunitas.Persistence.Configs.Laboratories;

internal class LaboratoryConfig : IEntityTypeConfiguration<Laboratory>
{
    public void Configure(EntityTypeBuilder<Laboratory> builder)
    {
        builder.HasOne(l => l.Address)
            .WithMany()
            .HasForeignKey(l => l.AddressId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
