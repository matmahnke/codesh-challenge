using Api.Contracts;
using FluentAssertions;
using System.Globalization;
using System.Text.Json;

namespace Tests.Api.Contracts
{
    public class CreateOrderRequestTests
    {
        [Fact]
        public void Deserialize_UsesPortugueseSnakeCasePropertyNames()
        {
            // Arrange
            var ativo = "PETR4";
            var lado = "C";
            var quantidade = 584;
            var preco = 54.87m;
            var json = $$"""
                {"ativo":"{{ativo}}","lado":"{{lado}}","quantidade":{{quantidade}},"preco":{{preco.ToString(CultureInfo.InvariantCulture)}}}
                """;

            // Act
            var request = JsonSerializer.Deserialize<CreateOrderRequest>(json);

            // Assert
            request.Should().NotBeNull();
            request!.Ativo.Should().Be(ativo);
            request.Lado.Should().Be(lado);
            request.Quantidade.Should().Be(quantidade);
            request.Preco.Should().Be(preco);
        }

        [Fact]
        public void Deserialize_MissingFeilds_LeavesThemNull()
        {
            // Arrange 
            var json = "{}";

            // Act
            var request = JsonSerializer.Deserialize<CreateOrderRequest>(json);

            // Assert
            request.Should().NotBeNull();
            request!.Ativo.Should().BeNull();
            request.Lado.Should().BeNull();
            request.Quantidade.Should().BeNull();
            request.Preco.Should().BeNull();
        }

        [Fact]
        public void Deserialize_WrongType_Throws()
        {
            // Arrange
            var json = """{"quantidade":"abc"}""";

            // Act
            var act = () => JsonSerializer.Deserialize<CreateOrderRequest>(json);

            // Assert
            act.Should().Throw<JsonException>();
        }
    }
}
