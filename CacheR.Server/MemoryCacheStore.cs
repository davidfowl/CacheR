using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using CacheR.Model;

namespace CacheR.Server
{
    public class MemoryCacheStore : ICacheStore
    {
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly Task _completedTask = CompletedTask();

        public Task Save(CacheEntry entry)
        {
            var policy = new CacheItemPolicy();

            // TODO: Allow this to be configured
            // Why 8 minutes? Because it feels right.
            policy.AbsoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(8);
            
            _cache.Set(entry.Key, entry.Value, policy);

            return _completedTask;
        }

        public IEnumerable<CacheEntry> GetAll()
        {
            foreach (var entry in _cache)
            {
                yield return new CacheEntry
                {
                    Key = entry.Key,
                    Value = entry.Value
                };
            }
        }

        public Task Delete(string key)
        {
            _cache.Remove(key);

            return _completedTask;
        }

        private static Task CompletedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }
}
