using Application.Models;

namespace Application.Transactions
{
    public interface IOrderTransaction
    {
        Task<CreateOrderResult> ProcessAsync(CreateOrderCommand command, CancellationToken ct);
    }
}
