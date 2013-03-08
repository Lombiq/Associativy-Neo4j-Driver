using System;
using Associativy.EventHandlers;
using Associativy.GraphDiscovery;
using Associativy.Models.Services;
using Associativy.Services;
using Orchard.Caching;

namespace Associativy.Neo4j.Services
{
    // TODO
    // Is it really necessary, or usage through StandardPathFinder is good enough?
    public class Neo4jPathFinder : GraphServiceBase, INeo4jPathFinder
    {
        private readonly INeo4jGraphClientPool _graphClientPool;
        private readonly IGraphEventMonitor _graphEventMonitor;
        private readonly ICacheManager _cacheManager;


        public Neo4jPathFinder(
            IGraphDescriptor graphDescriptor,
            Uri rootUri,
            INeo4jGraphClientPool graphClientPool,
            IGraphEventMonitor graphEventMonitor,
            ICacheManager cacheManager)
            : base(graphDescriptor)
        {
            _graphClientPool = graphClientPool;
            _graphEventMonitor = graphEventMonitor;
            _cacheManager = cacheManager;
        }


        public IPathResult FindPaths(int startNodeId, int targetNodeId, IPathFinderSettings settings)
        {
            if (settings == null) settings = PathFinderSettings.Default;

            if (settings.UseCache)
            {
                return _cacheManager.Get("Associativy.Paths." + _graphDescriptor.Name + startNodeId.ToString() + targetNodeId.ToString() + settings.MaxDistance, ctx =>
                {
                    _graphEventMonitor.MonitorChanged(_graphDescriptor, ctx);
                    return FindPaths(startNodeId, targetNodeId, settings);
                });
            }

            throw new NotImplementedException();
        }
    }
}