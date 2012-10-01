using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.Services;
using Associativy.GraphDiscovery;
using Associativy.EventHandlers;
using Orchard.Caching;

namespace Associativy.Neo4j.Services
{
    public class Neo4jPathFinder : Neo4jService, INeo4jPathFinder
    {
        private readonly INeo4jGraphClientPool _graphClientRepository;
        private readonly IGraphEditor _graphEditor;
        private readonly IGraphEventMonitor _graphEventMonitor;
        private readonly ICacheManager _cacheManager;

        public Neo4jPathFinder(
            INeo4jGraphClientPool graphClientRepository,
            IGraphManager graphManager,
            IGraphEditor graphEditor,
            IGraphEventMonitor graphEventMonitor,
            ICacheManager cacheManager)
        {
            _graphClientRepository = graphClientRepository;
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