using StackExchange.Redis;

namespace Jedi.Caching.Distributed
{
    public static class RedisConfigurationHelper
    {
        public static ConfigurationOptions RedisConfigurationMapping(RedisConfiguration configuration = default(RedisConfiguration))
        {
            var redisConfig = configuration ?? new RedisConfiguration();
            StackExchange.Redis.ConfigurationOptions config = new ConfigurationOptions();
            config.EndPoints.Clear();
            redisConfig.EndPoints.ForEach(p => config.EndPoints.Add(p));
            config.DefaultDatabase = redisConfig.DefaultDatabase;
            config.ConnectRetry = redisConfig.ConnectionRetryAttemps;
            config.ConnectTimeout = redisConfig.ConnectionTimeout;
            config.SyncTimeout = redisConfig.SyncTimeout;
            config.AllowAdmin = redisConfig.AllowAdmin;
            config.AbortOnConnectFail = redisConfig.AbortConnect;
            return config;
        }
    }
}
