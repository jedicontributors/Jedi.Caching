using StackExchange.Redis;

namespace Jedi.Caching.Distributed
{
    sealed class CacheBuilder
    {
        private IDistributedCacheService _distributedCacheService;
        public static CacheBuilder Builder() => new CacheBuilder();
        public CacheBuilder WithRedisConfiguration(RedisConfiguration configuration)
        {
            var config = RedisConfigurationHelper.RedisConfigurationMapping(configuration);

            _distributedCacheService = new RedisCacheService(ConnectionMultiplexer.Connect(config));

            return this;
        }
        public IDistributedCacheService BuildDistributedCache() => _distributedCacheService;
    }
}