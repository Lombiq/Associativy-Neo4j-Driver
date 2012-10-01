using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Associativy.Neo4j.Services;
using Neo4jClient;

namespace Associativy.Neo4j.Tests.Stubs
{
    public class StubClientPool : INeo4jGraphClientPool
    {
        private readonly IGraphClient _client;

        public StubClientPool()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data/"));
            client.Connect();
            _client = client;
        }

        public IGraphClient GetClient(Uri rootUri)
        {
            return _client;
        }
    }
}
