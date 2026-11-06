using Domain;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Tests.IntegrationTests.Fixtures;

namespace Tests.IntegrationTests.Repositories
{
    public class OrderRepositoryTests : IAsyncLifetime
    {
        private readonly SqliteDatabase _db = new();

        public Task InitializeAsync() => _db.InitializeAsync();

        public Task DisposeAsync() => _db.DisposeAsync();

        private static Order AcceptedOrder(string asset = Assets.PETR4, OrderSide side = OrderSide.Buy)
        {
            var order = Order.Create(asset, side, 584, 54.87m);
            new AssetExposure(asset).TryApply(order);
            return order;
        }

        private static Order RejectedOrder(string asset = Assets.PETR4)
        {
            var order = Order.Create(asset, OrderSide.Buy, 1, 0.01m);
            new AssetExposure(asset, AssetExposure.LimitCents).TryApply(order);
            return order;
        }

        [Fact]
        public async Task Add_WithoutSaveChanges_DoesNotPersist()
        {
            // Arrange
            await using var context = _db.CreateContext();
            var sut = new OrderRepository(context);
            var order = AcceptedOrder();

            // Act
            sut.Add(order);

            // Asserts
            context.Entry(order).State.Should().Be(EntityState.Added);
            var count = await _db.ScalarAsync("SELECT COUNT(*) FROM Orders");
            count.Should().Be(0L);
        }

        [Fact]
        public async Task Add_AcceptedOrder_PersistsAllFields()
        {
            // Arrange
            var order = AcceptedOrder();
            await using (var context = _db.CreateContext())
            {
                new OrderRepository(context).Add(order);

                // Act
                await context.SaveChangesAsync();
            }

            // Assert
            await using var readContext = _db.CreateContext();
            var persisted = await readContext.Orders.SingleAsync(o => o.Id == order.Id);
            persisted.Asset.Should().Be(order.Asset);
            persisted.Side.Should().Be(OrderSide.Buy);
            persisted.Quantity.Should().Be(584);
            persisted.PriceCents.Should().Be(5487);
            persisted.Status.Should().Be(OrderStatus.Accepted);
            persisted.RejectionReason.Should().BeNull();
            persisted.CreatedAt.Should().BeCloseTo(order.CreatedAt, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public async Task Add_RejectedOrder_PersistsStatusAndReason()
        {
            // Arrange
            var order = RejectedOrder();
            await using (var context = _db.CreateContext())
            {
                new OrderRepository(context).Add(order);

                // Act
                await context.SaveChangesAsync();
            }

            // Assert
            await using var readContext = _db.CreateContext();
            var persisted = await readContext.Orders.SingleAsync(o => o.Id == order.Id);
            persisted.Status.Should().Be(OrderStatus.Rejected);
            persisted.RejectionReason.Should().Be(order.RejectionReason).And.StartWith("Ordem rejeitada:");
        }

        [Fact]
        public async Task Add_StoresEnumsAsStrings()
        {
            // Arrange
            await using var context = _db.CreateContext();
            new OrderRepository(context).Add(AcceptedOrder(Assets.VIIA4, OrderSide.Sell));

            // Act
            await context.SaveChangesAsync();

            // Assert
            (await _db.ScalarAsync("SELECT Side FROM Orders")).Should().Be("Sell");
            (await _db.ScalarAsync("SELECT Status FROM Orders")).Should().Be("Accepted");
        }

        [Fact]
        public async Task Add_MultipleOrders_PersistsAll()
        {
            // Arrange
            await using var context = _db.CreateContext();
            var sut = new OrderRepository(context);
            sut.Add(AcceptedOrder(Assets.PETR4));
            sut.Add(AcceptedOrder(Assets.VALE3, OrderSide.Sell));
            sut.Add(RejectedOrder(Assets.VIIA4));

            // Act
            await context.SaveChangesAsync();

            // Assert
            var count = await _db.ScalarAsync("SELECT COUNT(*) FROM Orders");
            count.Should().Be(3L);
        }
    }
}
