using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordPoliciesDemo.API.Infrastructure.Domain;

namespace PasswordPoliciesDemo.API.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.HasIndex(x => x.Username).IsUnique();
            builder.Property(x => x.Username).HasMaxLength(100);


            builder.HasIndex(u => u.FirstName);
            builder.Property(x => x.FirstName).HasMaxLength(150);

            builder.HasIndex(u => u.LastName);
            builder.Property(x => x.LastName).HasMaxLength(150);




        }
    }
}
