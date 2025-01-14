using IdentityUserRegistration.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace IdentityUserRegistration.Seed;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasData(
            new Role
            {
                Id = "639d303f-7876-4fff-96ec-37f8bd3bf180",
                Name = "Visitor",
                NormalizedName = "VISITOR",
                Desctiption = "The visitor role for the user."
            }, 
            new Role 
            {
                Id = "45deb9d6-c1ae-44a6-8b3b-3b3b3b3b3b3b",
                Name = "Admin",
                NormalizedName = "ADMIN",
                Desctiption = "The admin role for the user."
            }
        );
    }
}