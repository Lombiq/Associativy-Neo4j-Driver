using System;
using Associativy.EventHandlers;
using Associativy.GraphDiscovery;
using Associativy.Models.Services;
using Associativy.Queryable;
using Associativy.Services;
using Orchard.Caching;

namespace Associativy.Neo4j.Services
{
    // TODO
    // Is it really necessary, or usage through StandardPathFinder is good enough?
    public class Neo4jPathFinder : Neo4jServiceBase, INeo4jPathFinder
    {
        private readonly IGraphEventMonitor _graphEventMonitor;
        private readonly ICacheManager _cacheManager;


        public Neo4jPathFinder(
            IGraphDescriptor graphDescriptor,
            Uri rootUri,
            INeo4jGraphClientPool graphClientPool,
            IGraphEventMonitor graphEventMonitor,
            ICacheManager cacheManager)
            : base(graphDescriptor, rootUri, graphClientPool)
        {
            _graphEventMonitor = graphEventMonitor;
            _cacheManager = cacheManager;
        }


        public IPathResult FindPaths(int startNodeId, int targetNodeId, IPathFinderSettings settings)
        {
            if (settings == null) settings = PathFinderSettings.Default;

            throw new NotImplementedException();
        }

        public IQueryableGraph<int> GetPartialGraph(int centralNodeId, IPathFinderSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}