using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Threading.Tasks;
using CacheR.Model;

namespace CacheR.Server
{
    public class MemoryCacheStore : ICacheStore
    {
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly Task _completedTask = CompletedTask();

        public Action<string> OnEntryRemoved { get; set; }

        public Task Save(CacheEntry entry)
        {
            var policy = new CacheItemPolicy();

            // TODO: Allow this to be configured
            // Why 8 minutes? Because it feels right.
            policy.AbsoluteExpiration = DateTimeOffset.Now + TimeSpan.FromMinutes(8);
            policy.RemovedCallback = OnCacheEntryRemoved;
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

        public void OnCacheEntryRemoved(CacheEntryRemovedArguments args)
        {
            Debug.WriteLine("Cache entry for '{0}' has been removed because of {1}.", args.CacheItem.Key, args.RemovedReason);

            if (OnEntryRemoved != null)
            {
                OnEntryRemoved(args.CacheItem.Key);
            }
        }
    }
}
