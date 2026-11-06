using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;

namespace Tests.Domain.Entities
{
    public class AssetExposureTests
    {
        private const long NearLimitCents = AssetExposure.LimitCents - 1_000;

        [Fact]
        public void Constructor_DefaultsToZeroExposure()
        {
            // Arrange
            var asset = "PETR4";

            // Act
            var exposure = new AssetExposure(asset);

            // Assert
            exposure.Asset.Should().Be(asset);
            exposure.ExposureCents.Should().Be(0);
            exposure.Exposure.Should().Be(0m);
            exposure.Version.Should().Be(0);
        }

        [Fact]
        public void Exposure_ConvertsCentsToDecimal()
        {
            // Arrange
            var exposure = new AssetExposure("PETR4", 123_456);

            // Act
            var value = exposure.Exposure;

            // Assert
            value.Should().Be(1234.56m);
        }

        [Fact]
        public void TryApply_Buy_IncreasesExposure_AcceptsOrder_AndBumpsVersion()
        {
            // Arrange
            var exposure = new AssetExposure("PETR4");
            var order = Order.Create("PETR4", OrderSide.Buy, 584, 54.87m);

            // Act
            var applied = exposure.TryApply(order);

            // Assert
            applied.Should().BeTrue();
            exposure.ExposureCents.Should().Be(3_204_408);
            exposure.Exposure.Should().Be(32044.08m);
            exposure.Version.Should().Be(1);
            order.Status.Should().Be(OrderStatus.Accepted);
            order.RejectionReason.Should().BeNull();
        }

        [Fact]
        public void TryApply_Sell_DecreasesExposure_AllowingNegative()
        {
            // Arrange
            var exposure = new AssetExposure("VALE3", 1_000);
            var order = Order.Create("VALE3", OrderSide.Sell, 3, 10m);

            // Act
            var applied = exposure.TryApply(order);

            // Assert
            applied.Should().BeTrue();
            exposure.ExposureCents.Should().Be(-2_000);
            exposure.Exposure.Should().Be(-20m);
        }

        [Fact]
        public void TryApply_MultipleOrders_AccumulatesExposureAndVersion()
        {
            // Arrange
            var exposure = new AssetExposure("PETR4");
            var orders = new[]
            {
                Order.Create("PETR4", OrderSide.Buy, 10, 100m),
                Order.Create("PETR4", OrderSide.Sell, 4, 100m),
                Order.Create("PETR4", OrderSide.Buy, 1, 0.5m)
            };

            // Act
            foreach (var order in orders)
                exposure.TryApply(order);

            // Assert
            exposure.Exposure.Should().Be(600.5m);
            exposure.Version.Should().Be(3);
        }

        [Fact]
        public void TryApply_ReachingLimitExactly_IsAccepted()
        {
            // Arrange
            var exposure = new AssetExposure("PETR4", NearLimitCents);
            var order = Order.Create("PETR4", OrderSide.Buy, 1, 10m);

            // Act
            var applied = exposure.TryApply(order);

            // Assert
            applied.Should().BeTrue();
            exposure.ExposureCents.Should().Be(AssetExposure.LimitCents);
        }

        [Fact]
        public void TryApply_ReachingNegativeLimitExactly_IsAccepted()
        {
            // Arrange
            var exposure = new AssetExposure("PETR4", -NearLimitCents);
            var order = Order.Create("PETR4", OrderSide.Sell, 1, 10m);

            // Act
            var applied = exposure.TryApply(order);

            // Assert
            applied.Should().BeTrue();
            exposure.ExposureCents.Should().Be(-AssetExposure.LimitCents);
        }

        [Fact]
        public void TryApply_ExceedingLimit_RejectsOrder_AndKeepsState()
        {
            // Arrange
            var asset = "PETR4";
            var exposure = new AssetExposure(asset, NearLimitCents);
            var order = Order.Create(asset, OrderSide.Buy, 1, 10.01m);

            // Act
            var applied = exposure.TryApply(order);

            // Assert
            applied.Should().BeFalse();
            exposure.ExposureCents.Should().Be(NearLimitCents);
            exposure.Version.Should().Be(0);
            order.Status.Should().Be(OrderStatus.Rejected);
            order.RejectionReason.Should().Be(
                $"Ordem rejeitada: a exposição de {asset} iria para 1.000.000,01, " +
                "ultrapassando o limite de 1.000.000,00.");
        }

        [Fact]
        public void TryApply_ExceedingNegativeLimit_RejectsOrder()
        {
            // Arrange
            var asset = "VIIA4";
            var exposure = new AssetExposure(asset, -NearLimitCents);
            var order = Order.Create(asset, OrderSide.Sell, 1, 10.01m);

            // Act
            var applied = exposure.TryApply(order);

            // Assert
            applied.Should().BeFalse();
            exposure.ExposureCents.Should().Be(-NearLimitCents);
            order.Status.Should().Be(OrderStatus.Rejected);
            order.RejectionReason.Should().Contain(asset).And.Contain("-1.000.000,01");
        }

        [Fact]
        public void TryApply_SingleHugeOrder_IsRejected()
        {
            // Arrange
            var exposure = new AssetExposure("PETR4");
            var order = Order.Create("PETR4", OrderSide.Buy, 99_999, 999.99m);

            // Act
            var applied = exposure.TryApply(order);

            // Assert
            applied.Should().BeFalse();
            exposure.ExposureCents.Should().Be(0);
        }

        [Fact]
        public void TryApply_OrderThatReducesExposureAwayFromLimit_IsAccepted()
        {
            // Arrange
            var exposure = new AssetExposure("PETR4", AssetExposure.LimitCents);
            var order = Order.Create("PETR4", OrderSide.Sell, 1, 1m);

            // Act
            var applied = exposure.TryApply(order);

            // Assert
            applied.Should().BeTrue();
            exposure.ExposureCents.Should().Be(AssetExposure.LimitCents - 100);
        }

        [Fact]
        public void TryApply_AcceptAfterReject_ClearsRejectionReason()
        {
            // Arrange
            var order = Order.Create("PETR4", OrderSide.Buy, 1, 10.01m);
            new AssetExposure("PETR4", NearLimitCents).TryApply(order);
            var freshExposure = new AssetExposure("PETR4");

            // Act
            var applied = freshExposure.TryApply(order);

            // Assert
            applied.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Accepted);
            order.RejectionReason.Should().BeNull();
        }

        [Fact]
        public void TryApply_OrderFromDifferentAsset_Throws_AndKeepsState()
        {
            // Arrange
            var exposureAsset = "PETR4";
            var orderAsset = "VALE3";
            var initialCents = 500;
            var exposure = new AssetExposure(exposureAsset, initialCents);
            var order = Order.Create(orderAsset, OrderSide.Buy, 1, 1m);

            // Act
            var act = () => exposure.TryApply(order);

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage($"Ordem do ativo '{orderAsset}' não pode ser aplicada à exposição de '{exposureAsset}'.");
            exposure.ExposureCents.Should().Be(initialCents);
            exposure.Version.Should().Be(0);
            order.RejectionReason.Should().BeNull();
        }
    }
}
