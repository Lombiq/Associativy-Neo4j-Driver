using System;
using System.Collections.Concurrent;
using Neo4jClient;

namespace Associativy.Neo4j.Services
{
    public class Neo4jGraphClientPool : INeo4jGraphClientPool
    {
        private readonly ConcurrentDictionary<string, IGraphClient> _clients = new ConcurrentDictionary<string, IGraphClient>();

        public IGraphClient GetClient(Uri rootUri)
        {
            return _clients.GetOrAdd(
                        rootUri.ToString(),
                        (key) =>
                        {
                            var client = new GraphClient(rootUri);
                            client.Connect();
                            return client;
                        });
        }
    }
}