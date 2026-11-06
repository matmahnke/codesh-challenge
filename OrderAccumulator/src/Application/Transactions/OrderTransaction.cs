using Application.Models;
using Application.Persistence;
using Application.Services;

namespace Application.Transactions
{
    public sealed class OrderTransaction(IOrderService orderService, IUnitOfWork unitOfWork) : IOrderTransaction
    {
        public async Task<CreateOrderResult> ProcessAsync(CreateOrderCommand command, CancellationToken ct)
        {
            // o dispose faz o rollback.
            await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

            var result = await orderService.ProcessAsync(command, ct);

            await transaction.CommitAsync(ct);

            return result;
        }
    }
}
