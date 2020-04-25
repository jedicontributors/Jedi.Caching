using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jedi.Caching.Distributed
{
    class RedisCacheService : IDistributedCacheService
    {
        public RedisCacheService(ConnectionMultiplexer connection)
        {
            Connection = connection;
        }

        ConnectionMultiplexer Connection { get; set; }
        IDatabase Db { get { return Connection.GetDatabase(); } }
        public void ClearAllCaches()
        {
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);
                server.FlushAllDatabases();
            }
        }

        public async Task ClearAllCachesAsync()
        {
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);
                await server.FlushAllDatabasesAsync();
            }
        }

        public bool Delete(string key) => Db.KeyDelete(key);

        public async Task<bool> DeleteAsync(string key) => await Db.KeyDeleteAsync(key);

        public T Get<T>(string key)
        {
            var value = Db.StringGet(key, CommandFlags.PreferSlave);

            return value.HasValue ? JsonConvert.DeserializeObject<T>(value) : default(T);
        }

        public object Get(string key, Type cacheType)
        {
            var value = Db.StringGet(key, CommandFlags.PreferSlave);

            return value.HasValue ? JsonConvert.DeserializeObject(value, cacheType, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }) : null;
        }

        public List<string> GetAllKeys()
        {
            var endpoint = Connection.GetEndPoints(true).FirstOrDefault();
            List<string> redisKeyList = new List<string>();

            var server = Connection.GetServer(endpoint);

            var redisKeys = server.Keys().ToList();

            foreach (var redisKey in redisKeys)

                if (!redisKeyList.Contains(redisKey))
                    redisKeyList.Add(redisKey);

            return redisKeyList;
        }

        public async Task<T> GetAsync<T>(string key) => await GetFromCacheAsync<T>(key);


        public Dictionary<string, string> GetInfo()
        {
            var info = Db.ScriptEvaluate("return redis.call('INFO')").ToString();

            return RedisLuaScriptHelper.ParseInfo(info);
        }

        public async Task<Dictionary<string, string>> GetInfoAsync()
        {
            var info = (await Db.ScriptEvaluateAsync("return redis.call('INFO')")).ToString();

            return RedisLuaScriptHelper.ParseInfo(info);
        }

        public T GetOrFetch<T>(string key, Func<T> fetchFunction, TimeSpan? expireTime = null)
        {
            T returnValue;
            try
            {
                returnValue = GetFromCache<T>(key);

                if (returnValue == null)
                    returnValue = fetchFunction();

            }
            catch (Exception ex)
            {
                if (ex is RedisException || ex is RedisConnectionException || ex is RedisTimeoutException || ex is RedisServerException)
                    returnValue = fetchFunction();
                else
                    throw ex;
            }

            TrySet(key, expireTime, returnValue);

            return returnValue;
        }

        public async Task<T> GetOrFetchAsync<T>(string key, Func<Task<T>> fetchFunction, TimeSpan? expireTime = null)
        {
            T returnValue;
            try
            {
                returnValue = await GetFromCacheAsync<T>(key);

                if (returnValue == null)
                    returnValue = await fetchFunction();

            }
            catch (Exception ex)
            {
                if (ex is RedisException || ex is RedisConnectionException || ex is RedisTimeoutException || ex is RedisServerException)
                    returnValue = await fetchFunction();
                else
                    throw ex;
            }

            await TrySetAsync(key, expireTime, returnValue);

            return returnValue;
        }

        public T HashGet<T>(string key, string fieldName)
        {
            var redisValue = Db.HashGet(key, fieldName, CommandFlags.PreferSlave);
            return redisValue.HasValue ? TConverter.ChangeType<T>(redisValue.ToString()) : default(T);
        }

        public Dictionary<string, string> HashGetAll(string key) => Db.HashGetAll(key, CommandFlags.PreferSlave).ToStringDictionary();


        public T HashGetAll<T>(string key) => Db.HashGetAll(key, CommandFlags.PreferSlave).ConvertFromRedis<T>();


        public async Task<Dictionary<string, string>> HashGetAllAsync(string key) => (await Db.HashGetAllAsync(key, CommandFlags.PreferSlave)).ToStringDictionary();


        public async Task<T> HashGetAllAsync<T>(string key) => (await Db.HashGetAllAsync(key, CommandFlags.PreferSlave)).ConvertFromRedis<T>();


        public async Task<T> HashGetAsync<T>(string key, string fieldName)
        {
            var redisValue = await Db.HashGetAsync(key, fieldName, CommandFlags.PreferSlave);

            return redisValue.HasValue ? await Task.Factory.StartNew(() => (TConverter.ChangeType<T>(redisValue.ToString()))) : default(T);
        }

        public void HashSet<T>(string key, T o) => Db.HashSet(key, o.ToHashEntries());

        public async Task HashSetAsync<T>(string key, T o) => await Db.HashSetAsync(key, o.ToHashEntries());

        public bool Insert<T>(string key, T cacheObject, TimeSpan? expireTime = null) => InsertCache<T>(key, cacheObject, expireTime);


        public async Task<bool> InsertAsync<T>(string key, T cacheObject, TimeSpan? expireTime = null) => await InsertCacheAsync<T>(key, cacheObject, expireTime);
        public bool KeyExist(string key) => Db.KeyExists(key);

        public Tuple<int, List<string>> Keys(int pageIndex = 0, int pageSize = 0, string pattern = "")
        {
            var endpoint = Connection.GetEndPoints(true).FirstOrDefault();
            List<string> redisKeyList = new List<string>();

            var server = Connection.GetServer(endpoint);

            var redisKeyCounts = server.Keys(pattern: pattern).Count();

            var redisKeys = server.Keys(pattern: pattern).Skip(pageSize * pageIndex)
                .Take(pageSize).ToList();

            foreach (var redisKey in redisKeys)

                if (!redisKeyList.Contains(redisKey))
                    redisKeyList.Add(redisKey);

            return new Tuple<int, List<string>>(redisKeyCounts, redisKeyList);
        }

        public Tuple<int, List<string>> Keys(string pattern = "", int takeCount = 0, int skipCount = 0)
        {
            var endpoint = Connection.GetEndPoints(true).FirstOrDefault();
            List<string> redisKeyList = new List<string>();

            var server = Connection.GetServer(endpoint);

            var redisKeyCounts = server.Keys(pattern: pattern).Count();

            var redisKeys = server.Keys(pattern: pattern).Skip(skipCount)
                .Take(takeCount).ToList();

            foreach (var redisKey in redisKeys)

                if (!redisKeyList.Contains(redisKey))
                    redisKeyList.Add(redisKey);

            return new Tuple<int, List<string>>(redisKeyCounts, redisKeyList);
        }

        public long RemoveKeysByPattern(string keyPattern)
        {
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);

                return Db.KeyDelete(server.Keys(pattern: keyPattern).ToArray());
            }
            return default(long);
        }

        public async Task<long> RemoveKeysByPatternAsync(string keyPattern)
        {
            var endpoints = Connection.GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = Connection.GetServer(endpoint);

                return await Db.KeyDeleteAsync(server.Keys(pattern: keyPattern).ToArray());
            }
            return default(long);
        }

        public bool Set<T>(string key, T cacheObject, TimeSpan? expireTime = null) => this.Set<T>(key, cacheObject);

        public bool Set(string key, Type cacheType, object cacheObject, TimeSpan? expireTime = null) => SetCache(key, cacheType, cacheObject, expireTime);

        public async Task<bool> SetAsync<T>(string key, T cacheObject, TimeSpan? expireTime = null) => await SetCacheAsync<T>(key, cacheObject, expireTime);

        public bool Update<T>(string key, T cacheObject, TimeSpan? expireTime = null) => UpdateCache<T>(key, cacheObject, expireTime);

        public async Task<bool> UpdateAsync<T>(string key, T cacheObject, TimeSpan? expireTime = null) => await UpdateCacheAsync(key, cacheObject, expireTime);

        private async Task<T> GetFromCacheAsync<T>(string key)
        {
            var value = await Db.StringGetAsync(key, CommandFlags.PreferSlave);

            return value.HasValue ? await Task.Factory.StartNew(() => (JsonConvert.DeserializeObject<T>(value))) : default(T);
        }
        private T GetFromCache<T>(string key)
        {
            var value = Db.StringGet(key, CommandFlags.PreferSlave);

            return value.HasValue ? JsonConvert.DeserializeObject<T>(value) : default(T);
        }
        private async Task TrySetAsync<T>(string key, TimeSpan? expireTime, T returnValue)
        {
            //Try Set
            try
            {
                await SetCacheAsync<T>(key, returnValue, expireTime);
            }
            catch { }
        }
        private void TrySet<T>(string key, TimeSpan? expireTime, T returnValue)
        {
            //Try Set
            try
            {
                SetCache<T>(key, returnValue, expireTime);
            }
            catch { }
        }
        private bool InsertCache<T>(string key, T cacheObject, TimeSpan? expireTime)
        {
            if (expireTime != null)
                return Db.StringSet(key, JsonConvert.SerializeObject(cacheObject), expireTime, When.NotExists);
            else
                return Db.StringSet(key, JsonConvert.SerializeObject(cacheObject), when: When.NotExists);
        }
        private async Task<bool> InsertCacheAsync<T>(string key, T cacheObject, TimeSpan? expireTime = null)
        {
            if (expireTime != null)
                return await Db.StringSetAsync(key, JsonConvert.SerializeObject(cacheObject), expireTime, When.NotExists);
            else
                return await Db.StringSetAsync(key, JsonConvert.SerializeObject(cacheObject), when: When.NotExists);
        }
        private bool SetCache<T>(string key, T cacheObject, TimeSpan? expireTime)
        {
            if (expireTime != null)
                return Db.StringSet(key, JsonConvert.SerializeObject(cacheObject), expireTime);
            else
                return Db.StringSet(key, JsonConvert.SerializeObject(cacheObject));
        }
        private bool SetCache(string key, Type cacheType, object cacheObject, TimeSpan? expireTime)
        {
            if (expireTime != null)
                return Db.StringSet(key, JsonConvert.SerializeObject(cacheObject, type: cacheType, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }), expireTime);
            else
                return Db.StringSet(key, JsonConvert.SerializeObject(cacheObject, type: cacheType, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }));
        }

        private async Task<bool> SetCacheAsync<T>(string key, T cacheObject, TimeSpan? expireTime)
        {
            if (expireTime != null)
                return await Db.StringSetAsync(key, JsonConvert.SerializeObject(cacheObject), expireTime);
            else
                return await Db.StringSetAsync(key, JsonConvert.SerializeObject(cacheObject));
        }

        private bool UpdateCache<T>(string key, T cacheObject, TimeSpan? expireTime)
        {
            if (expireTime != null)
                return Db.StringSet(key, JsonConvert.SerializeObject(cacheObject), expireTime, When.Exists);
            else
                return Db.StringSet(key, JsonConvert.SerializeObject(cacheObject), when: When.Exists);
        }

        private async Task<bool> UpdateCacheAsync<T>(string key, T cacheObject, TimeSpan? expireTime)
        {
            if (expireTime != null)
                return await Db.StringSetAsync(key, JsonConvert.SerializeObject(cacheObject), expireTime, When.Exists);
            else
                return await Db.StringSetAsync(key, JsonConvert.SerializeObject(cacheObject), when: When.Exists);
        }
    }
}
