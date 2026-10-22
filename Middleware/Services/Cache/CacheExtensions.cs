using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Middleware.Services.Cache
{
    public static class CacheExtensions
    {
        // Memurai/Redis cache register karta hai
        public static IServiceCollection AddAppCaching(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string redisConnection =
                configuration["Redis:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Redis:ConnectionString appsettings.json me nahi mila");

            ConfigurationOptions redisOptions =
                ConfigurationOptions.Parse(redisConnection);

            redisOptions.AbortOnConnectFail = false;  // Redis band ho to app na atke
            redisOptions.ConnectTimeout = 2000;       // max 2 second
            redisOptions.SyncTimeout = 2000;
            redisOptions.AsyncTimeout = 2000;

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = redisOptions;
                options.InstanceName = "Fundoo:";
            });

            services.AddSingleton<ICacheService, CacheService>();

            return services;
        }

        // Middleware pipeline me lagata hai
        public static IApplicationBuilder UseAppCaching(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResponseCacheMiddleware>();
        }
    }
}