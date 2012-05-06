using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CacheR.Model;
using Newtonsoft.Json;
using SignalR.Client;

namespace CacheR.Client
{
    public class Cache
    {
        private readonly Connection _connection;
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public Cache(string server)
        {
            if (!server.EndsWith("/"))
            {
                server += "/";
            }

            _connection = new Connection(server + "cache");
            _connection.Received += OnCacheEntryReceived;
        }

        private void OnCacheEntryReceived(string data)
        {
            var entries = JsonConvert.DeserializeObject<CacheEntry[]>(data);
            foreach (var entry in entries)
            {
                // You can't trick me C#...
                object value = entry.Value;
                _cache.AddOrUpdate(entry.Key, entry.Value, (k, v) => value);
            }
        }

        public object Get(string key)
        {
            object value;
            if (_cache.TryGetValue(key, out value))
            {
                return value;
            }

            return null;
        }

        public Task Add(string key, object value)
        {
            var entry = new[] {
                new CacheEntry
                {
                    Key = key,
                    Value = value
                }
            };

            // Make it available immediately to the local cache
            _cache.AddOrUpdate(key, value, (k, v) => value);

            return _connection.Send(JsonConvert.SerializeObject(entry));
        }

        public void Connect()
        {
            _connection.Start().Wait();
        }
    }
}
