using Immunitas.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Immunitas.Persistence.Configs.Users;

internal class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasOne(u => u.Laboratory)
               .WithMany()
               .HasForeignKey(u => u.LaboratoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
