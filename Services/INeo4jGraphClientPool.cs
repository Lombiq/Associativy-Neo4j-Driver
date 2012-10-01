using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;
using Neo4jClient;
using System.Security.Policy;

namespace Associativy.Neo4j.Services
{
    public interface INeo4jGraphClientPool : ISingletonDependency
    {
        IGraphClient GetClient(Uri rootUri);
    }
}
