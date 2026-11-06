namespace Api.Configurations
{
    public static class CorsConfigurations
    {
        public const string FrontendCorsPolicy = "Frontend";

        public static IServiceCollection AddCustomCors(
            this IServiceCollection services)
        {
            return services.AddCors(options =>
            {
                options.AddPolicy(FrontendCorsPolicy, policy =>
                {
                    policy.AllowAnyOrigin() // Todo: Add the specific frontend URL, by appsettings
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }

        public static void UseCustomCors(this WebApplication app)
        {
            app.UseCors(FrontendCorsPolicy);
        }
    }
}
