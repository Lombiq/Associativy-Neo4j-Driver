using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Associativy.Services;
using Orchard;

namespace Associativy.Neo4j.Services
{
    public interface INeo4jGraphStatisticsService : IGraphStatisticsService, ITransientDependency
    {
    }
}
