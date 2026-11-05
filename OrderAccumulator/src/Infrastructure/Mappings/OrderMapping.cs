using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings
{
    public sealed class OrderMapping : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Asset)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.Side)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasIndex(x => x.Asset);
        }
    }
}
