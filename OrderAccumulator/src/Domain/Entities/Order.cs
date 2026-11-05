using Domain.Enums;

namespace Domain.Entities
{
    public sealed class Order
    {
        public Guid Id { get; set; }
        public string Asset { get; set; }
        public OrderSide Side { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal Amount => Quantity * Price;

        public Order(string asset, OrderSide side, int quantity, decimal price)
        {
            Id = Guid.NewGuid();
            Asset = asset;
            Side = side;
            Quantity = quantity;
            Price = price;
        }
    }
}
