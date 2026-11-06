using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IOrderRepository
    {
        void Add(Order order);
    }
}
