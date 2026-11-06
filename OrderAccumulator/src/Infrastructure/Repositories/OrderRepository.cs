using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class OrderRepository(OrderAccumulatorDbContext context) : IOrderRepository
    {
        public void Add(Order order) => context.Orders.Add(order);
    }
}
