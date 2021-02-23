using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordPoliciesDemo.API.Infrastructure.Domain;

namespace PasswordPoliciesDemo.API.Infrastructure.Persistence
{
    public class DemoContext : DbContext
    {


        public DbSet<ApplicationUser> ApplicationUsers { get; set; }


        public DemoContext(DbContextOptions contextOptions) : base(contextOptions)
        {
            System.Diagnostics.Debug.WriteLine($"{GetType().Name}::ctor ->" + GetHashCode());
        }

        public sealed override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DemoContext).Assembly);
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = DateTimeOffset.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedOn = DateTimeOffset.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }


    }

   
}