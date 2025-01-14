using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityUserRegistration.Seed;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        builder.HasData(
            new IdentityUserRole<string>
            {
                UserId = "ffaea5d5-0fa5-49f1-9651-fbea4f1d263b",
                RoleId = "639d303f-7876-4fff-96ec-37f8bd3bf180"
            }
        );
    }
}
