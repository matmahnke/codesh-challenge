using Application.Models;

namespace Domain.Interfaces
{
    public interface IOrderService
    {
        Task<CreateOrderResult> ProcessAsync(CreateOrderCommand command, CancellationToken ct);
    }
}
