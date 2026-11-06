using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tests.IntegrationTests.Fixtures
{
    public class OrderApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"orders-it-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<OrderAccumulatorDbContext>>();
                services.AddDbContext<OrderAccumulatorDbContext>(options => options.UseSqlite($"Data Source={_dbPath}"));
            });
        }

        public async Task<T> WithDbContextAsync<T>(Func<OrderAccumulatorDbContext, Task<T>> query)
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrderAccumulatorDbContext>();
            return await query(context);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();

            SqliteConnection.ClearAllPools();
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }
    }
}
