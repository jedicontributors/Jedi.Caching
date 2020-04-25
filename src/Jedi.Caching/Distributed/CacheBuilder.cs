using StackExchange.Redis;
using System;

namespace Jedi.Caching.Distributed
{
    sealed class CacheBuilder
    {
        private IDistributedCacheService _distributedCacheService;
        private static Lazy<ConnectionMultiplexer> lazyConnection;
        public static CacheBuilder Builder() => new CacheBuilder();
        public CacheBuilder WithRedisConfiguration(RedisConfiguration configuration)
        {
            var config = RedisConfigurationHelper.RedisConfigurationMapping(configuration);

            lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(config));

            _distributedCacheService = new RedisCacheService(lazyConnection);

            return this;
        }
        public IDistributedCacheService BuildDistributedCache() => _distributedCacheService;
    }
}