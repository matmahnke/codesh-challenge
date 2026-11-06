using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.IntegrationTests.Fixtures
{
    public class SqliteDatabase : IAsyncLifetime
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        public async Task InitializeAsync()
        {
            await _connection.OpenAsync();

            await using var context = CreateContext();
            await context.Database.MigrateAsync();
        }

        public OrderAccumulatorDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<OrderAccumulatorDbContext>()
                .UseSqlite(_connection)
                .Options;

            return new OrderAccumulatorDbContext(options);
        }

        public async Task<object?> ScalarAsync(string sql)
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = sql;
            return await command.ExecuteScalarAsync();
        }

        public async Task DisposeAsync() => await _connection.DisposeAsync();
    }
}
