using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class AssetExposureRepository(OrderAccumulatorDbContext context) : IAssetExposureRepository
    {
        public Task<AssetExposure?> GetByAssetAsync(string asset, CancellationToken ct)
            => context.AssetExposures.FirstOrDefaultAsync(x => x.Asset == asset, ct);
    }
}
