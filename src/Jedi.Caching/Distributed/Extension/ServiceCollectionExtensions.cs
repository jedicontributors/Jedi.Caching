using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jedi.Caching.Distributed.Extension
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRedisCaching(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConfig = new RedisConfiguration();
            var redisConfigurationSection = configuration.GetSection("JediCacheSettings:RedisConfiguration");

            redisConfigurationSection.Bind(redisConfig);

            services.AddSingleton<IDistributedCacheService>(
                          CacheBuilder.Builder()
                         .WithRedisConfiguration(redisConfig)
                         .BuildDistributedCache()
                         );
        }

        public static void AddRedisCaching(this IServiceCollection services, RedisConfiguration redisConfiguration)
        {
            services.AddSingleton<IDistributedCacheService>(
                         CacheBuilder.Builder()
                        .WithRedisConfiguration(redisConfiguration)
                        .BuildDistributedCache()
                        );
        }
    }
}
