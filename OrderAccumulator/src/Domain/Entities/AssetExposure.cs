using System.Globalization;
using Domain.Exceptions;

namespace Domain.Entities
{
    public sealed class AssetExposure
    {
        public const long LimitCents = 1_000_000_00;

        public string Asset { get; private set; } = null!;
        public long ExposureCents { get; private set; }

        public int Version { get; private set; }

        public decimal Exposure => ExposureCents / 100m;

        private AssetExposure() { }

        public AssetExposure(string asset, long exposureCents = 0)
        {
            Asset = asset;
            ExposureCents = exposureCents;
        }

        public bool TryApply(Order order)
        {
            if (order.Asset != Asset)
                throw new DomainException($"Ordem do ativo '{order.Asset}' não pode ser aplicada à exposição de '{Asset}'.");

            var newExposure = ExposureCents + order.SignedNotionalCents;

            if (Math.Abs(newExposure) > LimitCents)
            {
                order.Reject(
                    $"Ordem rejeitada: a exposição de {Asset} iria para {FormatCents(newExposure)}, " +
                    $"ultrapassando o limite de {FormatCents(LimitCents)}.");
                return false;
            }

            ExposureCents = newExposure;
            Version++;
            order.Accept();
            return true;
        }

        private static string FormatCents(long cents) =>
            (cents / 100m).ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
    }
}
