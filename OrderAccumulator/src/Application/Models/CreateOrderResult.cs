namespace Application.Models
{
    public sealed record CreateOrderResult(bool Success, decimal CurrentExposure, string? ErrorMessage)
    {
        public static CreateOrderResult Accepted(decimal exposure) => new(true, exposure, null);

        public static CreateOrderResult Rejected(decimal exposure, string reason) => new(false, exposure, reason);
    }
}
