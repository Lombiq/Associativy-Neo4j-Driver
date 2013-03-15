using System;
using System.Collections.Generic;
using Associativy.EventHandlers;
using Associativy.GraphDiscovery;
using Associativy.Models.Services;
using Associativy.Neo4j.Models.Neo4j;
using Associativy.Queryable;
using Associativy.Services;
using Neo4jClient.Cypher;
using Orchard.Caching;
using System.Linq;
using QuickGraph;

namespace Associativy.Neo4j.Services
{
    public class Neo4jPathFinder : Neo4jServiceBase, INeo4jPathFinder
    {
        private readonly IPathFinderAuxiliaries _pathFinderAuxiliaries;
        private readonly IGraphEventMonitor _graphEventMonitor;

        private const string CachePrefix = "Associativy.Neo4j.Neo4jPathFinder.";


        public Neo4jPathFinder(
            IGraphDescriptor graphDescriptor,
            Uri rootUri,
            INeo4jGraphClientPool graphClientPool,
            IPathFinderAuxiliaries pathFinderAuxiliaries,
            IGraphEventMonitor graphEventMonitor)
            : base(graphDescriptor, rootUri, graphClientPool)
        {
            _graphEventMonitor = graphEventMonitor;
            _pathFinderAuxiliaries = pathFinderAuxiliaries;
        }


        public IPathResult FindPaths(int startNodeId, int targetNodeId, IPathFinderSettings settings)
        {
            if (settings == null) settings = PathFinderSettings.Default;

            TryInit();

            var paths = _graphClient.Cypher
                            .Start(
                                new CypherStartBitWithNodeIndexLookupWithSingleParameter("n", WellKnownConstants.NodeIdIndexName, "id:" + startNodeId),
                                new CypherStartBitWithNodeIndexLookupWithSingleParameter("t", WellKnownConstants.NodeIdIndexName, "id:" + targetNodeId)
                            )
                            .Match("path = (n)-[:" + WellKnownConstants.RelationshipTypeKey + "*1.." + settings.MaxDistance + "]-(t)")
                            .Return<Path>("EXTRACT(n in nodes(path) : n) AS Nodes", CypherResultMode.Projection) // Taken from: http://craigbrettdevden.blogspot.co.uk/2013/03/retrieving-paths-in-neo4jclient.html
                            .Results;

            var pathSteps = PathsToPathSteps(paths);
            return new Associativy.Services.PathFinderAuxiliaries.PathResult
            {
                SucceededPaths = pathSteps,
                SucceededGraph = PathToGraph(pathSteps, "PathToGraph:" + startNodeId + "/" + targetNodeId, settings)
            };
        }

        public IQueryableGraph<int> GetPartialGraph(int centralNodeId, IPathFinderSettings settings)
        {
            if (settings == null) settings = PathFinderSettings.Default;

            TryInit();

            return _pathFinderAuxiliaries.QueryableFactory.Create<int>((parameters) =>
            {
                var graph = _pathFinderAuxiliaries.CacheService.GetMonitored(_graphDescriptor, MakeCacheKey("GetPartialGraph.BaseGraph." + centralNodeId, settings), () =>
                {
                    var paths = _graphClient.Cypher
                                    .StartWithNodeIndexLookup("n", WellKnownConstants.NodeIdIndexName, "id:" + centralNodeId)
                                    .Match("path = (n)-[:" + WellKnownConstants.RelationshipTypeKey + "*1.." + settings.MaxDistance + "]-()")
                                    .Return<Path>("EXTRACT(n in nodes(path) : n) AS Nodes", CypherResultMode.Projection) // Taken from: http://craigbrettdevden.blogspot.co.uk/2013/03/retrieving-paths-in-neo4jclient.html
                                    .Results;

                    return _pathFinderAuxiliaries.PathToGraph(PathsToPathSteps(paths));
                }, settings.UseCache);


                return LastStepsWithPaging(parameters, graph, "GetPartialGraph." + centralNodeId + ".PathToGraph.", settings);
            });
        }


        private IQueryableGraph<int> PathToGraph(IEnumerable<IList<int>> succeededPaths, string baseCacheKey, IPathFinderSettings settings)
        {
            return _pathFinderAuxiliaries.QueryableFactory.Create<int>((parameters) =>
            {
                var graph = _pathFinderAuxiliaries.CacheService.GetMonitored(_graphDescriptor, MakeCacheKey(baseCacheKey + "BaseGraph.", settings), () =>
                {
                    return _pathFinderAuxiliaries.PathToGraph(succeededPaths);
                }, settings.UseCache);


                return LastStepsWithPaging(parameters, graph, baseCacheKey, settings);
            });
        }

        private dynamic LastStepsWithPaging(IExecutionParams parameters, IUndirectedGraph<int, IUndirectedEdge<int>> graph, string cacheName, IPathFinderSettings settings)
        {
            return QueryableGraphHelper.LastStepsWithPaging(new Params
            {
                CacheService = _pathFinderAuxiliaries.CacheService,
                GraphEditor = _pathFinderAuxiliaries.GraphEditor,
                GraphDescriptor = _graphDescriptor,
                ExecutionParameters = parameters,
                Graph = graph,
                BaseCacheKey = MakeCacheKey(cacheName, settings),
                UseCache = settings.UseCache
            });
        }

        private string MakeCacheKey(string name, IPathFinderSettings settings)
        {
            return CachePrefix + _graphDescriptor.Name + "." + name + ".PathFinderSettings:" + settings.MaxDistance;
        }


        private static IEnumerable<IList<int>> PathsToPathSteps(IEnumerable<Path> paths)
        {
            return paths.Select(path => path.Nodes.Select(node => node.Id).ToList());
        }


        public class Path
        {
            public List<AssociativyNode> Nodes { get; set; }
        }
    }
}