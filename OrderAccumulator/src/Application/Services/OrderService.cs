using Application.Models;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public sealed class OrderService(
        IAssetExposureRepository exposures,
        IOrderRepository orders) : IOrderService
    {
        public async Task<CreateOrderResult> ProcessAsync(CreateOrderCommand command, CancellationToken ct)
        {
            var order = Order.Create(command.Asset, command.Side, command.Quantity, command.Price);

            var exposure = await exposures.GetByAssetAsync(command.Asset, ct)
                ?? throw new InvalidOperationException($"Exposição do ativo '{command.Asset}' não encontrada.");

            var accepted = exposure.TryApply(order);
            orders.Add(order);

            return accepted
                ? CreateOrderResult.Accepted(exposure.Exposure)
                : CreateOrderResult.Rejected(exposure.Exposure, order.RejectionReason!);
        }
    }
}
