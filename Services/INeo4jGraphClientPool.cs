using System;
using Neo4jClient;
using Orchard;

namespace Associativy.Neo4j.Services
{
    public interface INeo4jGraphClientPool : ISingletonDependency
    {
        IGraphClient GetClient(Uri rootUri);
    }
}
