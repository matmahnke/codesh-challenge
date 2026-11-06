using Application.Models;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests.Application.Services
{
    public class OrderServiceTests
    {
        private readonly IAssetExposureRepository _exposures = Substitute.For<IAssetExposureRepository>();
        private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _sut = new OrderService(_exposures, _orders);
        }

        private AssetExposure GivenExposure(string asset, long exposureCents = 0)
        {
            var exposure = new AssetExposure(asset, exposureCents);
            _exposures.GetByAssetAsync(asset, Arg.Any<CancellationToken>()).Returns(exposure);
            return exposure;
        }

        [Fact]
        public async Task Buy_WithinLimit_ReturnsAccepted_AndUpdatesExposure()
        {
            // Arrange
            var exposure = GivenExposure("PETR4");
            var command = new CreateOrderCommand("PETR4", OrderSide.Buy, 584, 54.87m);

            // Act
            var result = await _sut.ProcessAsync(command, CancellationToken.None);

            // Assert
            result.Should().Be(CreateOrderResult.Accepted(32044.08m));
            exposure.Exposure.Should().Be(32044.08m);
        }

        [Fact]
        public async Task Sell_WithinLimit_ReturnsAcceptedWithNegativeExpossure()
        {
            // Arrange
            GivenExposure("VALE3");
            var command = new CreateOrderCommand("VALE3", OrderSide.Sell, 10, 100m);

            // Act
            var result = await _sut.ProcessAsync(command, CancellationToken.None);

            // Assert
            result.Should().Be(CreateOrderResult.Accepted(-1000m));
        }

        [Fact]
        public async Task AcceptedOrder_IsAddedToRepository_WithCommandData()
        {
            // Arrange
            var command = new CreateOrderCommand("PETR4", OrderSide.Buy, 584, 54.87m);
            GivenExposure(command.Asset);

            // Act
            await _sut.ProcessAsync(command, CancellationToken.None);

            // Assert
            _orders.Received(1).Add(Arg.Is<Order>(o =>
                o.Asset == command.Asset &&
                o.Side == command.Side &&
                o.Quantity == command.Quantity &&
                o.PriceCents == 5487 &&
                o.Status == OrderStatus.Accepted));
        }

        [Fact]
        public async Task ExceedingLimit_ReturnsRejected_WithUnchangedExposureAndReason()
        {
            // Arrange
            var asset = "PETR4";
            var nearLimt = AssetExposure.LimitCents - 1_000;
            var exposure = GivenExposure(asset, nearLimt);
            var command = new CreateOrderCommand(asset, OrderSide.Buy, 1, 10.01m);

            // Act
            var result = await _sut.ProcessAsync(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.CurrentExposure.Should().Be(nearLimt / 100m);
            result.ErrorMessage.Should().StartWith("Ordem rejeitada:").And.Contain(asset);
            exposure.ExposureCents.Should().Be(nearLimt);
        }

        [Fact]
        public async Task RejectedOrder_IsStillAddedToRepository()
        {
            // Arrange
            GivenExposure("PETR4", AssetExposure.LimitCents);
            var command = new CreateOrderCommand("PETR4", OrderSide.Buy, 1, 0.01m);

            //Act
            await _sut.ProcessAsync(command, CancellationToken.None);
            // Assert
            _orders.Received(1).Add(Arg.Is<Order>(o =>
                o.Status == OrderStatus.Rejected && o.RejectionReason != null));
        }

        [Fact]
        public async Task LooksUpExposureByAsset_ForwardingCancellationToken()
        {
            // Arrange
            var asset ="VIIA4";
            GivenExposure(asset);
            using var cts = new CancellationTokenSource();
            var command = new CreateOrderCommand(asset, OrderSide.Buy, 1, 1m);

            // Act
            await _sut.ProcessAsync(command, cts.Token);

            // Assert
            await _exposures.Received(1).GetByAssetAsync(asset, cts.Token);
        }

        [Fact]
        public async Task ExposureNotFound_Throws_AndDoesNotAddOrder()
        {
            // Arange
            _exposures.GetByAssetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((AssetExposure?)null);
            var asset = "PETR4";
            var command = new CreateOrderCommand(asset, OrderSide.Buy, 1, 1m);

            // Act
            var act = () => _sut.ProcessAsync(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Exposição do ativo '{asset}' não encontrada.");
            _orders.DidNotReceiveWithAnyArgs().Add(default!);
        }

        [Fact]
        public async Task InvalidCommand_ThrowsDomainException_BeforeTouchingRepositories()
        {
            // Arrange
            var command = new CreateOrderCommand("XPTO3", OrderSide.Buy, 1, 1m);

            // Act
            var act = () => _sut.ProcessAsync(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DomainException>();
            await _exposures.DidNotReceiveWithAnyArgs().GetByAssetAsync(default!, default);
            _orders.DidNotReceiveWithAnyArgs().Add(default!);
        }

        [Fact]
        public async Task RepositoryThrows_PropagatesException_AndDoesNotAddOrder()
        {
            // Arrange
            _exposures.GetByAssetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new TimeoutException());
            var command = new CreateOrderCommand("PETR4", OrderSide.Buy, 1, 1m);

            // Act
            var act = () => _sut.ProcessAsync(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<TimeoutException>();
            _orders.DidNotReceiveWithAnyArgs().Add(default!);
        }


        [Fact]
        public async Task SequentialOrders_AccumulateOnSameExposure()
        {
            // Arrange
            var exposure = GivenExposure("PETR4");
            var buy = new CreateOrderCommand("PETR4", OrderSide.Buy, 10, 100m);
            var sell = new CreateOrderCommand("PETR4", OrderSide.Sell, 4, 100m);

            // Act
            await _sut.ProcessAsync(buy, CancellationToken.None);
            var result = await _sut.ProcessAsync(sell, CancellationToken.None);

            // Assert
            result.Should().Be(CreateOrderResult.Accepted(600m));
            exposure.Version.Should().Be(2);
            _orders.ReceivedWithAnyArgs(2).Add(default!); 
        }
    }
}
