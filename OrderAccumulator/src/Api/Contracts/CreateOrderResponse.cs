using System.Text.Json.Serialization;

namespace Api.Contracts
{
    public sealed class CreateOrderResponse(bool sucesso, decimal exposicaoAtual, string? msgErro)
    {
        [JsonPropertyName("sucesso")]
        public bool Sucesso { get; } = sucesso;

        [JsonPropertyName("exposicao_atual")]
        public decimal ExposicaoAtual { get; } = exposicaoAtual;

        [JsonPropertyName("msg_erro")]
        public string? MsgErro { get; } = msgErro;

        /// <summary>Erro antes do processamento da ordem (validação/infra): exposição não é conhecida.</summary>
        public static CreateOrderResponse Failure(string msgErro) => new(false, 0m, msgErro);
    }
}
