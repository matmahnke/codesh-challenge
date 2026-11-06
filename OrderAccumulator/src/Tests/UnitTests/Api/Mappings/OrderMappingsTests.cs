using Api.Contracts;
using Api.Mappings;
using Application.Models;
using Domain.Enums;
using FluentAssertions;
using FluentValidation.Results;
using System.Diagnostics;

namespace Tests.UnitTests.Api.Mappings
{
    public class OrderMappingsTests
    {
        [Theory]
        [InlineData("C", OrderSide.Buy)]
        [InlineData("V", OrderSide.Sell)]
        public void ToCommand_MapsAllFields(string lado, OrderSide expectedSide)
        {
            // Arrange
            var ativo = "VALE3";
            var quantidade = 10;
            var preco = 99.99m;
            var request = new CreateOrderRequest { Ativo = ativo, Lado = lado, Quantidade = quantidade, Preco = preco };

            // Act
            var command = request.ToCommand();

            // Assert
            command.Should().Be(new CreateOrderCommand(ativo, expectedSide, quantidade, preco));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("c")]
        [InlineData("X")]
        public void ToCommand_UnvalidatedSide_ThrowsUnreachable(string? lado)
        {
            // Arrange
            var request = new CreateOrderRequest { Ativo = "PETR4", Lado = lado, Quantidade = 1, Preco = 1m };

            // Act
            var act = () => request.ToCommand();

            // Assert
            act.Should().Throw<UnreachableException>().WithMessage($"*'{lado}'*");
        }

        [Fact]
        public void ResultToResponse_Accepted_MapsFields()
        {
            // Arrange
            var exposure = 1500.50m;
            var result = CreateOrderResult.Accepted(exposure);

            // Act
            var response = result.ToResponse();

            // Assert
            response.Sucesso.Should().BeTrue();
            response.ExposicaoAtual.Should().Be(exposure);
            response.MsgErro.Should().BeNull();
        }

        [Fact]
        public void ResultToResponse_Rejected_MapsFields()
        {
            // Arrange
            var exposure = 800m;
            var reason = "Limite excedido.";
            var result = CreateOrderResult.Rejected(exposure, reason);

            // Act
            var response = result.ToResponse();

            // Assert
            response.Sucesso.Should().BeFalse();
            response.ExposicaoAtual.Should().Be(exposure);
            response.MsgErro.Should().Be(reason);
        }

        [Fact]
        public void ValidationToResponse_SingleError_ReturnsFailureWithMessage()
        {
            // Arrange
            var error = "Ativo é obrigatório.";
            var validation = new ValidationResult([new ValidationFailure("Ativo", error)]);

            // Act
            var response = validation.ToResponse();

            // Assert
            response.Sucesso.Should().BeFalse();
            response.ExposicaoAtual.Should().Be(0m);
            response.MsgErro.Should().Be(error);
        }

        [Fact]
        public void ValidationToResponse_MultipleErrors_JoinsMessagesInOrder()
        {
            // Arrange
            var assetError = "Ativo é obrigatório.";
            var sideError = "Lado é obrigatório.";
            var priceError = "Preço é obrigatório.";
            var validation = new ValidationResult(
            [
                new ValidationFailure("Ativo", assetError),
                new ValidationFailure("Lado", sideError),
                new ValidationFailure("Preco", priceError)
            ]);

            // Act
            var response = validation.ToResponse();

            // Assert
            response.MsgErro.Should().Be($"{assetError}; {sideError}; {priceError}");
        }
    }
}
