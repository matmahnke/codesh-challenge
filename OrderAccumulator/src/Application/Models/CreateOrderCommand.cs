using Domain.Enums;

namespace Application.Models
{
    public class CreateOrderCommand
    {
        public CreateOrderCommand(string asset, OrderSide side, int quantity, decimal price)
        {
            Asset = asset;
            Side = side;
            Quantity = quantity;
            Price = price;
        }
        public string Asset { get; set; }
        public OrderSide Side { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
