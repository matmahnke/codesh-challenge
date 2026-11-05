namespace Domain.Entities
{
    public sealed class AssetExposure
    {
        public string Asset { get; set; }
        public decimal Exposure { get; set; }

        private const decimal Limit = 1_000_000;

        public AssetExposure(string asset)
        {
            Asset = asset;
        }

        public void ApplyOrder(Order order)
        {
            if (order.Asset != Asset)
                throw new ArgumentException("Invalid Asset");

            var diff = order.Side == Enums.OrderSide.C ? order.Amount : -order.Amount;

            var newExposure = Exposure + diff;

            if (Math.Abs(newExposure) > Limit)
                throw new Exception("Exposure limit Exceeded"); // TODO: Turn it into a new custom exception

            Exposure = newExposure;
        }
    }
}
