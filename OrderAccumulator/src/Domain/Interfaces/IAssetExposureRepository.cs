using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IAssetExposureRepository
    {
        Task<AssetExposure?> GetByAssetAsync(string asset, CancellationToken ct);
    }
}
