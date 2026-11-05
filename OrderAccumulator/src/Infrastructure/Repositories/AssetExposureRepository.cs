using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class AssetExposureRepository(OrderAccumulatorDbContext context) : IAssetExposureRepository
    {
        public void Add(AssetExposure exposure)
        {
            context.AssetExposures.Add(exposure);
        }

        public async Task<AssetExposure?> GetByAssetAsync(string asset, CancellationToken ct)
        {
            return await context.AssetExposures.FirstOrDefaultAsync(x => x.Asset == asset, ct);
        }
    }
}
