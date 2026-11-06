using Domain;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mappings
{
    public sealed class AssetExposureMapping : IEntityTypeConfiguration<AssetExposure>
    {
        public void Configure(EntityTypeBuilder<AssetExposure> builder)
        {
            builder.ToTable("AssetExposures");

            builder.HasKey(x => x.Asset);

            builder.Property(x => x.Asset)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.ExposureCents)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsConcurrencyToken()
                .IsRequired();

            builder.Ignore(x => x.Exposure);

            builder.HasData(Assets.Supported.Select(asset => new AssetExposure(asset)));
        }
    }
}
