using Application.Services;
using Domain.Interfaces;

namespace Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
