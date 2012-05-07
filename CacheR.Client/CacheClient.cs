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
            var command = JsonConvert.DeserializeObject<CacheCommand>(data);

            switch (command.Type)
            {
                case CacheCommandType.Add:
                    foreach (var entry in command.Entries)
                    {
                        // You can't trick me C#...
                        object value = entry.Value;
                        _cache.AddOrUpdate(entry.Key, entry.Value, (k, v) => value);
                    }
                    break;
                case CacheCommandType.Remove:
                    foreach (var entry in command.Entries)
                    {
                        object value;
                        _cache.TryRemove(entry.Key, out value);
                    }
                    break;
                default:
                    break;
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

            var command = new CacheCommand
            {
                Type = CacheCommandType.Add,
                Entries = entry
            };

            // Make it available immediately to the local cache
            _cache.AddOrUpdate(key, value, (k, v) => value);

            return SendCommand(command);
        }

        public Task Delete(string key)
        {
            var command = new CacheCommand
            {
                Type = CacheCommandType.Remove,
                Entries = new[] {  
                    new CacheEntry {
                        Key = key
                    }
                }
            };

            // Execute local delete
            object value;
            _cache.TryRemove(key, out value);

            return SendCommand(command);
        }

        public void Connect()
        {
            _connection.Start().Wait();
        }

        private Task SendCommand(CacheCommand command)
        {
            return _connection.Send(JsonConvert.SerializeObject(command));
        }
    }
}
