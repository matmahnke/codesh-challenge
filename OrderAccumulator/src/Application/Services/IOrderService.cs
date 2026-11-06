using Application.Models;

namespace Application.Services
{
    public interface IOrderService
    {
        Task<CreateOrderResult> ProcessAsync(CreateOrderCommand command, CancellationToken ct);
    }
}
