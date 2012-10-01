using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Neo4jClient;
using System.Collections.Concurrent;

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