using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Tests.Domain
{
    public class AssetExposureTests
    {
        private static Order Buy(int qty, decimal price) => Order.Create("PETR4", OrderSide.Buy, qty, price);
        private static Order Sell(int qty, decimal price) => Order.Create("PETR4", OrderSide.Sell, qty, price);

        [Fact]
        public void BuyIncreases_SellDecreases()
        {
            var exposure = new AssetExposure("PETR4");

            Assert.True(exposure.TryApply(Buy(584, 54.87m)));
            Assert.True(exposure.TryApply(Sell(100, 10.00m)));

            Assert.Equal(584 * 54.87m - 1_000m, exposure.Exposure);
        }

        [Fact]
        public void AcceptedOrder_IsMarkedAccepted_AndBumpsVersion()
        {
            var exposure = new AssetExposure("PETR4");
            var order = Buy(1, 1m);

            exposure.TryApply(order);

            Assert.Equal(OrderStatus.Accepted, order.Status);
            Assert.Null(order.RejectionReason);
            Assert.Equal(1, exposure.Version);
        }

        [Fact]
        public void ExactlyAtLimit_IsAccepted()
        {
            var exposure = new AssetExposure("PETR4", AssetExposure.LimitCents - 100);

            Assert.True(exposure.TryApply(Buy(1, 1.00m)));
            Assert.Equal(1_000_000m, exposure.Exposure);
        }

        [Fact]
        public void AboveLimit_IsRejected_AndExposureIsUnchanged()
        {
            var exposure = new AssetExposure("PETR4", AssetExposure.LimitCents);
            var order = Buy(1, 0.01m);

            Assert.False(exposure.TryApply(order));

            Assert.Equal(1_000_000m, exposure.Exposure);
            Assert.Equal(0, exposure.Version);
            Assert.Equal(OrderStatus.Rejected, order.Status);
            Assert.False(string.IsNullOrWhiteSpace(order.RejectionReason));
        }

        [Fact]
        public void BelowNegativeLimit_IsRejected()
        {
            var exposure = new AssetExposure("PETR4", -AssetExposure.LimitCents);

            Assert.False(exposure.TryApply(Sell(1, 0.01m)));
            Assert.Equal(-1_000_000m, exposure.Exposure);
        }

        [Fact]
        public void SellFromLimit_IsAccepted()
        {
            var exposure = new AssetExposure("PETR4", AssetExposure.LimitCents);

            Assert.True(exposure.TryApply(Sell(10, 100m)));
            Assert.Equal(999_000m, exposure.Exposure);
        }

        [Fact]
        public void OrderForAnotherAsset_Throws()
        {
            var exposure = new AssetExposure("VALE3");

            Assert.Throws<DomainException>(() => exposure.TryApply(Buy(1, 1m)));
        }
    }
}
