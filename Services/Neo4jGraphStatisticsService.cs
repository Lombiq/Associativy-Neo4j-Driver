using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.GraphDiscovery;
using Associativy.Models.Services;
using Associativy.Services;

namespace Associativy.Neo4j.Services
{
    public class Neo4jGraphStatisticsService : GraphServiceBase, INeo4jGraphStatisticsService
    {
        public Neo4jGraphStatisticsService(IGraphDescriptor graphDescriptor)
            : base(graphDescriptor)
        {
        }


        public IGraphInfo GetGraphInfo()
        {
            throw new NotImplementedException();
        }
    }
}