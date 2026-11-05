using System.Text.Json.Serialization;

namespace Api.Contracts
{
    public class CreateOrderResponse
    {
        [JsonPropertyName("sucesso")]
        public bool Sucesso { get; set; }

        [JsonPropertyName("exposicao_atual")]
        public decimal ExposicaoAtual { get; set; }

        [JsonPropertyName("msg_erro")]
        public string MensagemDeErro { get; set; }

        public CreateOrderResponse(bool sucesso, decimal exposicaoAtual, string mensagemDeErro) // TODO: Rename it to english
        {
            Sucesso = sucesso;
            ExposicaoAtual = exposicaoAtual;
            MensagemDeErro = mensagemDeErro;
        }
    }
}
