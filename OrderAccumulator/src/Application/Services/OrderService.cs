using Application.Models;
using Application.Persistence;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class OrderService(IAssetExposureRepository assetExposureRepository, IUnitOfWork unitOfWork) : IOrderService
    {
        public async Task<CreateOrderResult> ProcessAsync(CreateOrderCommand command, CancellationToken ct)
        {
            await using (unitOfWork)
            {
                await unitOfWork.BeginTransactionAsync(ct);
                var order = new Order(command.Asset, command.Side, command.Quantity, command.Price);

                var exposure = await assetExposureRepository.GetByAssetAsync(order.Asset, ct);

                if (exposure == null)
                    exposure = new AssetExposure(order.Asset);

                exposure.ApplyOrder(order);

                assetExposureRepository.Add(exposure);

                await unitOfWork.CommitAsync(ct);

                return new CreateOrderResult(true, exposure.Exposure, string.Empty);
            }
        }
    }
}
