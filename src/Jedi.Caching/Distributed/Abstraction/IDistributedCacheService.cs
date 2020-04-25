using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jedi.Caching.Distributed
{
    public interface IDistributedCacheService
    {
        new bool Delete(string key);

        Task<bool> DeleteAsync(string key);

        new bool KeyExist(string key);

        void ClearAllCaches();

        Task ClearAllCachesAsync();

        List<string> GetAllKeys();

        Tuple<int, List<string>> Keys(int pageIndex = 0, int pageSize = 0, string pattern = "");

        Tuple<int, List<string>> Keys(string pattern = "", int takeCount = 0, int skipCount = 0);

        long RemoveKeysByPattern(string keyPattern);

        Task<long> RemoveKeysByPatternAsync(string keyPattern);

        new T Get<T>(string key);

        object Get(string key, Type cacheType);

        T GetOrFetch<T>(string key, Func<T> fetchFunction , TimeSpan? expireTime = null);

        Task<T> GetOrFetchAsync<T>(string key, Func<Task<T>> fetchFunction, TimeSpan? expireTime = null);

        Task<T> GetAsync<T>(string key);

        bool Set<T>(string key, T cacheObject, TimeSpan? expireTime = null);

        bool Set(string key, Type cacheType, object cacheObject, TimeSpan? expireTime = null);

        Task<bool> SetAsync<T>(string key, T cacheObject, TimeSpan? expireTime = null);

        bool Insert<T>(string key, T cacheObject, TimeSpan? expireTime = null);

        Task<bool> InsertAsync<T>(string key, T cacheObject, TimeSpan? expireTime = null);

        bool Update<T>(string key, T cacheObject,TimeSpan? expireTime = null);

        Task<bool> UpdateAsync<T>(string key, T cacheObject, TimeSpan? expireTime = null);

        Dictionary<string, string> GetInfo();

        Task<Dictionary<string, string>> GetInfoAsync();

        T HashGet<T>(string key, string fieldName);

        Task<T> HashGetAsync<T>(string key, string fieldName);

        void HashSet<T>(string key, T o);

        Task HashSetAsync<T>(string key, T o);

        Dictionary<string, string> HashGetAll(string key);

        Task<Dictionary<string, string>> HashGetAllAsync(string key);

        T HashGetAll<T>(string key);

        Task<T> HashGetAllAsync<T>(string key);
    }
}
