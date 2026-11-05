using Domain.Enums;

namespace Api.Contracts
{
    public class CreateOrderRequest
    {
        public string Ativo { get; set; }
        public OrderSide Lado { get; set; }
        public int Quantidade { get; set; }
        public decimal Preco { get; set; }
    }
}
