using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using System.Globalization;

namespace Tests.UnitTests.Domain.Entities
{
    public class OrderTests
    {
        private static decimal D(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

        [Fact]
        public void Create_ValidInput_SetsAllProperties()
        {
            // Arrange
            var asset = "PETR4";
            var side = OrderSide.Buy;
            var quantity = 584;
            var before = DateTime.UtcNow;

            // Act
            var order = Order.Create(asset, side, quantity, 54.87m);

            // Assert
            order.Id.Should().NotBe(Guid.Empty);
            order.Asset.Should().Be(asset);
            order.Side.Should().Be(side);
            order.Quantity.Should().Be(quantity);
            order.PriceCents.Should().Be(5487);
            order.RejectionReason.Should().BeNull();
            order.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
            order.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public void Create_GeneratesUniqueIds()
        {
            // Arrange
            var asset = "PETR4";

            // Act
            var first = Order.Create(asset, OrderSide.Buy, 1, 1m);
            var second = Order.Create(asset, OrderSide.Buy, 1, 1m);

            // Assert
            first.Id.Should().NotBe(second.Id);
        }

        [Theory]
        [InlineData("PETR4")]
        [InlineData("VALE3")]
        [InlineData("VIIA4")]
        public void Create_SupportedAsset_Succeeds(string asset)
        {
            // Arrange
            var side = OrderSide.Buy;

            // Act
            var act = () => Order.Create(asset, side, 1, 1m);

            // Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("XPTO3")]
        [InlineData("petr4")]
        [InlineData("")]
        public void Create_UnsupportedAsset_Throws(string asset)
        {
            // Arrange
            var side = OrderSide.Buy;

            // Act
            var act = () => Order.Create(asset, side, 1, 1m);

            // Assert
            act.Should().Throw<DomainException>().WithMessage($"Ativo '{asset}' não suportado.");
        }

        [Fact]
        public void Create_UndefinedSide_Throws()
        {
            // Arrange
            var side = (OrderSide)99;

            // Act
            var act = () => Order.Create("PETR4", side, 1, 1m);

            // Assert
            act.Should().Throw<DomainException>().WithMessage("Lado inválido.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(99_999)]
        public void Create_QuantityWithinRange_Succeeds(int quantity)
        {
            // Arrange
            var asset = "PETR4";

            // Act
            var order = Order.Create(asset, OrderSide.Buy, quantity, 1m);

            // Assert
            order.Quantity.Should().Be(quantity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(100_000)]
        [InlineData(int.MaxValue)]
        public void Create_QuantityOutOfRange_Throws(int quantity)
        {
            // Arrange
            var asset = "PETR4";

            // Act
            var act = () => Order.Create(asset, OrderSide.Buy, quantity, 1m);

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("Quantidade deve ser um inteiro entre 1 e 99999.");
        }

        [Theory]
        [InlineData("0.01", 1)]
        [InlineData("10.5", 1050)]
        [InlineData("999.99", 99_999)]
        public void Create_PriceWithinRange_ConvertsToCents(string price, long expectedCents)
        {
            // Arrange
            var parsedPrice = D(price);

            // Act
            var order = Order.Create("PETR4", OrderSide.Buy, 1, parsedPrice);

            // Assert
            order.PriceCents.Should().Be(expectedCents);
        }

        [Theory]
        [InlineData("10.001")]
        [InlineData("0.005")]
        public void Create_PriceNotMultipleOfOneCent_Throws(string price)
        {
            // Arrange
            var parsedPrice = D(price);

            // Act
            var act = () => Order.Create("PETR4", OrderSide.Buy, 1, parsedPrice);

            // Assert
            act.Should().Throw<DomainException>().WithMessage("Preço deve ser múltiplo de 0.01.");
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-0.01")]
        [InlineData("1000")]
        [InlineData("1000.01")]
        public void Create_PriceOutOfRange_Throws(string price)
        {
            // Arrange
            var parsedPrice = D(price);

            // Act
            var act = () => Order.Create("PETR4", OrderSide.Buy, 1, parsedPrice);

            // Assert
            act.Should().Throw<DomainException>().WithMessage("Preço deve ser positivo e menor que 1000.");
        }

        [Theory]
        [InlineData(OrderSide.Buy, 3_204_408)]
        [InlineData(OrderSide.Sell, -3_204_408)]
        public void SignedNotionalCents_IsPositiveForBuyAndNegativeForSell(OrderSide side, long expected)
        {
            // Arrange
            var order = Order.Create("PETR4", side, 584, 54.87m);

            // Act
            var notional = order.SignedNotionalCents;

            // Assert
            notional.Should().Be(expected);
        }

        [Fact]
        public void SignedNotionalCents_MaxQuantityAndPrice_DoesNotOverflow()
        {
            // Arrange
            var order = Order.Create("PETR4", OrderSide.Sell, 99_999, 999.99m);

            // Act
            var notional = order.SignedNotionalCents;

            // Assert
            notional.Should().Be(-99_999L * 99_999L);
        }
    }
}
