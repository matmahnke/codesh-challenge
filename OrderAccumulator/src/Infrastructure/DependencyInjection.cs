using Application.Persistence;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<OrderAccumulatorDbContext>(options => options.UseSqlite(connectionString));

            services.AddScoped<IAssetExposureRepository, AssetExposureRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            return services;
        }

        public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<OrderAccumulatorDbContext>();

            await context.Database.MigrateAsync();
        }
    }
}
