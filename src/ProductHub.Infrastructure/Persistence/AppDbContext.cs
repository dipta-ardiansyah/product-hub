using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductHub.Domain.Entities;
using ProductHub.Infrastructure.Identity;

namespace ProductHub.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Product> Products => Set<Product>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}