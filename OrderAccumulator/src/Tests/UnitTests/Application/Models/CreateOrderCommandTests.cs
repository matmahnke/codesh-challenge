using Application.Models;
using Domain.Enums;
using FluentAssertions;

namespace Tests.UnitTests.Application.Models
{
    public class CreateOrderCommandTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            // Arrange
            var asset = "PETR4";
            var side = OrderSide.Sell;
            var quantity = 10;
            var price = 54.87m;

            // Act
            var command = new CreateOrderCommand(asset, side, quantity, price);

            // Assert
            command.Asset.Should().Be(asset);
            command.Side.Should().Be(side);
            command.Quantity.Should().Be(quantity);
            command.Price.Should().Be(price);
        }

        [Fact]
        public void Equality_IsByValue()
        {
            // Arrange
            var asset = "PETR4";
            var side = OrderSide.Buy;
            var quantity = 10;
            var price = 54.87m;

            // Act
            var first = new CreateOrderCommand(asset, side, quantity, price);
            var second = new CreateOrderCommand(asset, side, quantity, price);

            // Assert
            first.Should().Be(second);
            first.GetHashCode().Should().Be(second.GetHashCode());
        }

        [Fact]
        public void Equality_DiffersWhenAnyFieldDiffers()
        {
            // Arrange
            var baseline = new CreateOrderCommand("PETR4", OrderSide.Buy, 10, 54.87m);

            // Act
            var differentAsset = baseline with { Asset = "VALE3" };
            var differentSide = baseline with { Side = OrderSide.Sell };
            var differentQuantity = baseline with { Quantity = 11 };
            var differentPrice = baseline with { Price = 54.88m };

            // Assert
            baseline.Should().NotBe(differentAsset);
            baseline.Should().NotBe(differentSide);
            baseline.Should().NotBe(differentQuantity);
            baseline.Should().NotBe(differentPrice);
        }
    }
}
