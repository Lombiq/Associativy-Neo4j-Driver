using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Associativy.Neo4j.Models;
using Associativy.Neo4j.Services;

namespace Associativy.Neo4j.Tests.Stubs
{
    public class StubNeo4jGraphInfoService : INeo4jGraphInfoService
    {
        private readonly GraphInfo _graphInfo = new GraphInfo();

        public IGraphInfo GetGraphInfo(string graphName)
        {
            return _graphInfo;
        }


        private class GraphInfo : IGraphInfo
        {
            public int BiggestNodeId { get; set; }
            public int BiggestNodeNeighbourCount { get; set; }
        }
    }
}
