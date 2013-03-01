using System;
using Associativy.EventHandlers;
using Associativy.GraphDiscovery;
using Associativy.Services;
using Orchard.Caching;

namespace Associativy.Neo4j.Services
{
    // TODO
    // Is it really necessary, or usage through StandardPathFinder is good enough?
    public class Neo4jPathFinder : Neo4jService, INeo4jPathFinder
    {
        private readonly INeo4jGraphClientPool _graphClientPool;
        private readonly IGraphEditor _graphEditor;
        private readonly IGraphEventMonitor _graphEventMonitor;
        private readonly ICacheManager _cacheManager;


        public Neo4jPathFinder(
            INeo4jGraphClientPool graphClientPool,
            IGraphManager graphManager,
            IGraphEditor graphEditor,
            IGraphEventMonitor graphEventMonitor,
            ICacheManager cacheManager)
        {
            _graphClientPool = graphClientPool;
            _graphEditor = graphEditor;
            _graphEventMonitor = graphEventMonitor;
            _cacheManager = cacheManager;
        }


        public PathResult FindPaths(IGraphContext graphContext, int startNodeId, int targetNodeId, int maxDistance = 3, bool useCache = false)
        {
            if (useCache)
            {
                return _cacheManager.Get("Associativy.Paths." + graphContext.GraphName + startNodeId.ToString() + targetNodeId.ToString() + maxDistance, ctx =>
                {
                    _graphEventMonitor.MonitorChanged(graphContext, ctx);
                    return FindPaths(graphContext, startNodeId, targetNodeId, maxDistance, false);
                });
            }

            throw new NotImplementedException();
        }
    }
}