using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Associativy.Neo4j.Models;
using Orchard;

namespace Associativy.Neo4j.Services
{
    public interface INeo4jGraphInfoService : IDependency
    {
        IGraphInfo GetGraphInfo(string graphName);
    }
}
