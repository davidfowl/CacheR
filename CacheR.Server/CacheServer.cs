using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CacheR.Model;
using Newtonsoft.Json;
using SignalR;
using SignalR.Hosting;
using SelfHostServer = SignalR.Hosting.Self.Server;

namespace CacheR.Server
{
    public class CacheServer
    {
        private readonly SelfHostServer _server;

        public CacheServer(string url)
            : this(new MemoryCacheStore(), url)
        {
        }

        public CacheServer(ICacheStore store, string url)
        {
            Store = store;
            _server = new SelfHostServer(url);
            _server.DependencyResolver.Register(typeof(CacheConnection), () => new CacheConnection(this));
            _server.MapConnection<CacheConnection>("/cache");
        }

        public ICacheStore Store
        {
            get;
            private set;
        }

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
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

        private class CacheConnection : PersistentConnection
        {
            private readonly CacheServer _cache;

            public CacheConnection(CacheServer cache)
            {
                _cache = cache;
            }

            protected override Task OnConnectedAsync(IRequest request, string connectionId)
            {
                // REVIEW: Should we send all of the data to the client on reconnect?
                // Let the client request a subset of the data or none at all.
                return Connection.Send(connectionId, _cache.Store.GetAll());
            }

            protected override Task OnReceivedAsync(string connectionId, string data)
            {
                // Store the data and tell all clients to update
                return _cache.Save(data).ContinueWith(task => Connection.Broadcast(data)).Unwrap();
            }
        }
    }
}
