using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Application
{
    public sealed class OrderServiceConcurrencyTests : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"orders-{Guid.NewGuid():N}.db");

        public OrderServiceConcurrencyTests()
        {
            using var db = CreateContext();
            db.Database.Migrate();
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            File.Delete(_path);
        }

        private OrderAccumulatorDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<OrderAccumulatorDbContext>()
                .UseSqlite($"Data Source={_path}")
                .Options);

        private async Task<CreateOrderResult> Send(CreateOrderCommand command)
        {
            await using var db = CreateContext();
            return await OrderServiceTests.CreateService(db).ProcessAsync(command, CancellationToken.None);
        }

        [Fact]
        public async Task ParallelOrders_NeverExceedTheLimit()
        {
            var command = new CreateOrderCommand("PETR4", OrderSide.Buy, 500, 100m);

            var results = await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => Task.Run(() => Send(command))));

            Assert.Equal(20, results.Count(r => r.Success));
            Assert.Equal(30, results.Count(r => !r.Success));

            await using var db = CreateContext();
            var exposure = await db.AssetExposures.SingleAsync(e => e.Asset == "PETR4");
            Assert.Equal(1_000_000m, exposure.Exposure);
            Assert.Equal(50, await db.Orders.CountAsync());
        }
    }
}
