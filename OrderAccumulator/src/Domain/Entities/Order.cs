using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities
{
    public sealed class Order
    {
        public const int MaxQuantityExclusive = 100_000;
        public const long MaxPriceCentsExclusive = 1_000_00;

        public Guid Id { get; private set; }
        public string Asset { get; private set; } = null!;
        public OrderSide Side { get; private set; }
        public int Quantity { get; private set; }
        public long PriceCents { get; private set; }
        public OrderStatus Status { get; private set; }
        public string? RejectionReason { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public long SignedNotionalCents => (Side == OrderSide.Buy ? 1 : -1) * Quantity * PriceCents;

        private Order() { }

        public static Order Create(string asset, OrderSide side, int quantity, decimal price)
        {
            if (!Assets.IsSupported(asset))
                throw new DomainException($"Ativo '{asset}' não suportado.");

            if (!Enum.IsDefined(side))
                throw new DomainException("Lado inválido.");

            if (quantity <= 0 || quantity >= MaxQuantityExclusive)
                throw new DomainException($"Quantidade deve ser um inteiro entre 1 e {MaxQuantityExclusive - 1}.");

            var cents = price * 100;
            if (cents != decimal.Truncate(cents))
                throw new DomainException("Preço deve ser múltiplo de 0.01.");

            var priceCents = (long)cents;
            if (priceCents <= 0 || priceCents >= MaxPriceCentsExclusive)
                throw new DomainException("Preço deve ser positivo e menor que 1000.");

            return new Order
            {
                Id = Guid.NewGuid(),
                Asset = asset,
                Side = side,
                Quantity = quantity,
                PriceCents = priceCents,
                CreatedAt = DateTime.UtcNow
            };
        }

        internal void Accept()
        {
            Status = OrderStatus.Accepted;
            RejectionReason = null;
        }

        internal void Reject(string reason)
        {
            Status = OrderStatus.Rejected;
            RejectionReason = reason;
        }
    }
}
