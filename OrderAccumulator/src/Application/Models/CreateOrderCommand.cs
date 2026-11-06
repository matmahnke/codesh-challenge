using Domain.Enums;

namespace Application.Models
{
    public sealed record CreateOrderCommand(string Asset, OrderSide Side, int Quantity, decimal Price);
}
