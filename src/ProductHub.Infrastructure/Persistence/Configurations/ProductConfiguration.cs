using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductHub.Domain.Entities;

namespace ProductHub.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.Price);
            builder.HasIndex(p => p.CreatedAt);
        }
    }
}