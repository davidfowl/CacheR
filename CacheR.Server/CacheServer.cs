using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheR.Model;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;

namespace CacheR.Server
{
    public class CacheServer
    {
        private IDisposable _server;
        private IPersistentConnectionContext _context;
        private readonly string _url;

        public CacheServer(string url)
            : this(new MemoryCacheStore(), url)
        {
        }

        public CacheServer(ICacheStore store, string url)
        {
            Store = store;
            Store.OnEntryRemoved = OnEntryRemoved;
            _url = url;
        }

        public ICacheStore Store
        {
            get;
            private set;
        }

        public void Start()
        {
            if (_server == null)
            {
                _server = WebApp.Start(_url, app =>
                {
                    var config = new ConnectionConfiguration
                    {
                        Resolver = new DefaultDependencyResolver()
                    };

                    config.Resolver.Register(typeof(CacheConnection), () => new CacheConnection(this));

                    _context = config.Resolver.Resolve<IConnectionManager>().GetConnectionContext<CacheConnection>();

                    app.MapSignalR<CacheConnection>("/cache", config);
                });
            }
        }

        public void Stop()
        {
            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }
        }

        private Task Save(string rawCommand)
        {
            Debug.WriteLine("Processing command: " + rawCommand);

            // REVIEW: Should save retry on failure?
            var command = JsonConvert.DeserializeObject<CacheCommand>(rawCommand);

            switch (command.Type)
            {
                case CacheCommandType.Add:
                    return Store.Save(command.Entries[0]);
                case CacheCommandType.Remove:
                    return Store.Delete(command.Entries[0].Key);
                default:
                    throw new NotSupportedException();
            }
        }

        public void OnEntryRemoved(string key)
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

            // When an entry is removed let all subscribers know
            _context.Connection.Broadcast(command);
        }

        private class CacheConnection : PersistentConnection
        {
            private readonly CacheServer _cache;

            public CacheConnection(CacheServer cache)
            {
                _cache = cache;
            }

            protected override Task OnConnected(IRequest request, string connectionId)
            {
                var command = new CacheCommand
                {
                    Type = CacheCommandType.Add,
                    Entries = _cache.Store.GetAll().ToArray()
                };

                // REVIEW: Should we send all of the data to the client on reconnect?
                // Let the client request a subset of the data or none at all.
                return Connection.Send(connectionId, command);
            }

            protected override async Task OnReceived(IRequest request, string connectionId, string data)
            {
                // Store the data and tell all clients to update
                await _cache.Save(data);
                await Connection.Broadcast(data);
            }
        }
    }
}
