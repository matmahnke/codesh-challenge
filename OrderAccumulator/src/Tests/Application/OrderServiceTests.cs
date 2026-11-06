using Application.Models;
using Application.Persistence;
using Application.Services;
using Application.Transactions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Application
{
    public sealed class OrderServiceTests : IDisposable
    {
        private readonly SqliteConnection _connection;

        public OrderServiceTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            using var db = CreateContext();
            db.Database.Migrate();
        }

        public void Dispose() => _connection.Dispose();

        private OrderAccumulatorDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<OrderAccumulatorDbContext>()
                .UseSqlite(_connection)
                .Options);

        internal static IOrderTransaction CreateService(OrderAccumulatorDbContext db) =>
            new OrderTransaction(
                new OrderService(new AssetExposureRepository(db), new OrderRepository(db)),
                new Infrastructure.UnitOfWork.UnitOfWork(db));

        private async Task<CreateOrderResult> Send(CreateOrderCommand command)
        {
            await using var db = CreateContext();
            return await CreateService(db).ProcessAsync(command, CancellationToken.None);
        }

        [Fact]
        public async Task SeveralOrdersForSameAsset_Accumulate()
        {
            await Send(new("PETR4", OrderSide.Buy, 584, 54.87m));
            var result = await Send(new("PETR4", OrderSide.Sell, 100, 10m));

            Assert.True(result.Success);
            Assert.Equal(584 * 54.87m - 1_000m, result.CurrentExposure);

            await using var db = CreateContext();
            Assert.Equal(2, await db.Orders.CountAsync(o => o.Status == OrderStatus.Accepted));
        }

        [Fact]
        public async Task ExposureIsIndependentPerAsset()
        {
            await Send(new("PETR4", OrderSide.Buy, 1000, 100m));
            var result = await Send(new("VALE3", OrderSide.Buy, 1, 1m));

            Assert.Equal(1m, result.CurrentExposure);
        }

        [Fact]
        public async Task RejectedOrder_IsStoredAsRejected_AndDoesNotChangeExposure()
        {
            await Send(new("VALE3", OrderSide.Buy, 10_000, 99m));
            var result = await Send(new("VALE3", OrderSide.Buy, 1_000, 11m));

            Assert.False(result.Success);
            Assert.Equal(990_000m, result.CurrentExposure);
            Assert.NotNull(result.ErrorMessage);

            await using var db = CreateContext();
            var exposure = await db.AssetExposures.SingleAsync(e => e.Asset == "VALE3");
            Assert.Equal(990_000m, exposure.Exposure);

            var rejected = await db.Orders.SingleAsync(o => o.Status == OrderStatus.Rejected);
            Assert.Equal(result.ErrorMessage, rejected.RejectionReason);
        }

        [Fact]
        public async Task ConcurrentUpdate_IsDetected()
        {
            await using var first = CreateContext();
            await using var second = CreateContext();

            var a = await first.AssetExposures.SingleAsync(e => e.Asset == "PETR4");
            var b = await second.AssetExposures.SingleAsync(e => e.Asset == "PETR4");

            a.TryApply(Order.Create("PETR4", OrderSide.Buy, 1, 1m));
            b.TryApply(Order.Create("PETR4", OrderSide.Buy, 1, 1m));

            await using (var tx = await new Infrastructure.UnitOfWork.UnitOfWork(first).BeginTransactionAsync())
                await tx.CommitAsync();

            await using var stale = await new Infrastructure.UnitOfWork.UnitOfWork(second).BeginTransactionAsync();
            await Assert.ThrowsAsync<ConcurrencyConflictException>(() => stale.CommitAsync());
        }
    }
}
