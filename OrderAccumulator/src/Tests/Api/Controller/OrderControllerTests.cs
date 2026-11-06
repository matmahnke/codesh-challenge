using Api.Contracts;
using Api.Controllers;
using Application.Models;
using Application.Transactions;
using Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests.Api.Controller
{
    public class OrderControllerTests
    {
        private readonly IOrderTransaction _transaction = Substitute.For<IOrderTransaction>();
        private readonly IValidator<CreateOrderRequest> _validator = Substitute.For<IValidator<CreateOrderRequest>>();
        private readonly OrdersController _sut;

        public OrderControllerTests()
        {
            _validator.ValidateAsync(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>())
                .Returns(new ValidationResult());

            _transaction.ProcessAsync(Arg.Any<CreateOrderCommand>(), Arg.Any<CancellationToken>())
                .Returns(CreateOrderResult.Accepted(0m));

            _sut = new OrdersController(_transaction, _validator);
        }

        private static CreateOrderRequest Valid(string lado = "C") => new()
        {
            Ativo = "PETR4",
            Lado = lado,
            Quantidade = 584,
            Preco = 54.87m
        };

        private void GivenValidationErrors(params string[] messages) =>
            _validator.ValidateAsync(Arg.Any<CreateOrderRequest>(), Arg.Any<CancellationToken>())
                .Returns(new ValidationResult(messages.Select(m => new ValidationFailure("campo", m))));

        [Fact]
        public async Task ValidRequest_Accepted_ReturnsOkWithExposure()
        {
            // Arrange
            var exposure = 32044.08m;
            _transaction.ProcessAsync(Arg.Any<CreateOrderCommand>(), Arg.Any<CancellationToken>())
                .Returns(CreateOrderResult.Accepted(exposure));

            // Act
            var actionResult = await _sut.Create(Valid(), CancellationToken.None);

            // Assert
            var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(StatusCodes.Status200OK);
            var response = ok.Value.Should().BeOfType<CreateOrderResponse>().Subject;
            response.Sucesso.Should().BeTrue();
            response.ExposicaoAtual.Should().Be(exposure);
            response.MsgErro.Should().BeNull();
        }

        [Fact]
        public async Task ValidRequest_RejectedByLimit_ReturnsOkWithErrorAndCurrentExposure()
        {
            // Arrange
            var exposure = 500m;
            var reason = "Limite de exposição excedido.";
            _transaction.ProcessAsync(Arg.Any<CreateOrderCommand>(), Arg.Any<CancellationToken>())
                .Returns(CreateOrderResult.Rejected(exposure, reason));

            // Act
            var actionResult = await _sut.Create(Valid(), CancellationToken.None);

            // Assert
            var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = ok.Value.Should().BeOfType<CreateOrderResponse>().Subject;
            response.Sucesso.Should().BeFalse();
            response.ExposicaoAtual.Should().Be(exposure);
            response.MsgErro.Should().Be(reason);
        }

        [Theory]
        [InlineData("C", OrderSide.Buy)]
        [InlineData("V", OrderSide.Sell)]
        public async Task ValidRequest_MapsRequestToCommand(string lado, OrderSide expectedSide)
        {
            // Arrange
            var request = Valid(lado);

            // Act
            await _sut.Create(request, CancellationToken.None);

            // Assert
            await _transaction.Received(1).ProcessAsync(
                new CreateOrderCommand(request.Ativo!, expectedSide, request.Quantidade!.Value, request.Preco!.Value),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ValidRequest_ForwardsCancellationTokenToValidatorAndTransaction()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var request = Valid();

            // Act
            await _sut.Create(request, cts.Token);

            // Assert
            await _validator.Received(1).ValidateAsync(request, cts.Token);
            await _transaction.Received(1).ProcessAsync(Arg.Any<CreateOrderCommand>(), cts.Token);
        }

        [Fact]
        public async Task InvalidRequest_ReturnsBadRequest_WithoutCallingTransaction()
        {
            // Arrange
            var error = "Ativo deve ser PETR4, VALE3, VIIA4.";
            GivenValidationErrors(error);

            // Act
            var actionResult = await _sut.Create(Valid(), CancellationToken.None);

            // Assert
            var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            var response = badRequest.Value.Should().BeOfType<CreateOrderResponse>().Subject;
            response.Sucesso.Should().BeFalse();
            response.ExposicaoAtual.Should().Be(0m);
            response.MsgErro.Should().Be(error);
            await _transaction.DidNotReceiveWithAnyArgs().ProcessAsync(default!, default);
        }

        [Fact]
        public async Task InvalidRequest_MultipleErrors_JoinsMessages()
        {
            // Arrange
            var assetError = "Ativo é obrigatório.";
            var sideError = "Lado é obrigatório.";
            GivenValidationErrors(assetError, sideError);

            // Act
            var actionResult = await _sut.Create(new CreateOrderRequest(), CancellationToken.None);

            // Assert
            var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequest.Value.Should().BeOfType<CreateOrderResponse>().Subject;
            response.MsgErro.Should().Be($"{assetError}; {sideError}");
            await _transaction.DidNotReceiveWithAnyArgs().ProcessAsync(default!, default);
        }

        [Fact]
        public async Task TransactionThrows_PropagatesException()
        {
            // Arrange
            _transaction.ProcessAsync(Arg.Any<CreateOrderCommand>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException());

            // Act
            var act = () => _sut.Create(Valid(), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
