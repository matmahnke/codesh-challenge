using Api.Contracts;
using FluentAssertions;
using System.Text.Json;

namespace Tests.Api.Contracts
{
    public class CreateOrderResponseTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            // Arrange
            var exposicao = 123.45m;

            // Act
            var response = new CreateOrderResponse(true, exposicao, null);

            // Assert
            response.Sucesso.Should().BeTrue();
            response.ExposicaoAtual.Should().Be(exposicao);
            response.MsgErro.Should().BeNull();
        }

        [Fact]
        public void Failure_ReturnsUnsuccessfulResponseWithZeroExposure()
        {
            // Arrange
            var msgErro = "Erro qualquer.";

            // Act
            var response = CreateOrderResponse.Failure(msgErro);

            // Assert
            response.Sucesso.Should().BeFalse();
            response.ExposicaoAtual.Should().Be(0m);
            response.MsgErro.Should().Be(msgErro);
        }

        [Fact]
        public void Serialize_UsesPortugueseSnakeCasePropertyNames()
        {
            // Arrange
            var exposicao = 500m;
            var msgErro = "Limite excedido.";
            var response = new CreateOrderResponse(false, exposicao, msgErro);

            // Act
            var json = JsonSerializer.Serialize(response);

            // Assert
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            root.GetProperty("sucesso").GetBoolean().Should().BeFalse();
            root.GetProperty("exposicao_atual").GetDecimal().Should().Be(exposicao);
            root.GetProperty("msg_erro").GetString().Should().Be(msgErro);
            root.EnumerateObject().Select(p => p.Name)
                .Should().BeEquivalentTo("sucesso", "exposicao_atual", "msg_erro");
        }

        [Fact]
        public void Serialize_NullError_WritesNull()
        {
            // Arrange
            var response = new CreateOrderResponse(true, 10m, null);

            // Act
            var json = JsonSerializer.Serialize(response);

            // Assert
            using var doc = JsonDocument.Parse(json);
            doc.RootElement.GetProperty("msg_erro").ValueKind.Should().Be(JsonValueKind.Null);
        }
    }
}
