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
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Side)
                .HasConversion<string>()
                .HasMaxLength(4)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.PriceCents)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.RejectionReason)
                .HasMaxLength(300);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Ignore(x => x.SignedNotionalCents);

            builder.HasOne<AssetExposure>()
                .WithMany()
                .HasForeignKey(x => x.Asset)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.Asset, x.CreatedAt });
        }
    }
}
