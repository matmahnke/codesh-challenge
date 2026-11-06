using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Tests.Domain
{
    public class OrderTests
    {
        [Fact]
        public void Create_ConvertsPriceToCents()
        {
            var order = Order.Create("PETR4", OrderSide.Buy, 584, 54.87m);

            Assert.Equal(5487, order.PriceCents);
            Assert.Equal(584 * 5487, order.SignedNotionalCents);
        }

        [Fact]
        public void SignedNotional_IsNegativeForSell()
        {
            var order = Order.Create("VALE3", OrderSide.Sell, 10, 1.50m);

            Assert.Equal(-1500, order.SignedNotionalCents);
        }

        [Theory]
        [InlineData("XPTO3", 1, 1.00)]
        [InlineData("PETR4", 0, 1.00)]
        [InlineData("PETR4", 100000, 1.00)]
        [InlineData("PETR4", 1, 0.00)]
        [InlineData("PETR4", 1, 1000.00)]
        [InlineData("PETR4", 1, 10.001)]
        public void Create_RejectsInvalidInput(string asset, int quantity, double price)
        {
            Assert.Throws<DomainException>(() => Order.Create(asset, OrderSide.Buy, quantity, (decimal)price));
        }
    }
}
