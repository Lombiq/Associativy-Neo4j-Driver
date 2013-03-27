using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Associativy.EventHandlers;
using Associativy.GraphDiscovery;
using Associativy.Models;
using Associativy.Models.Nodes;
using Associativy.Models.Services;
using Associativy.Neo4j.Models.Neo4j;
using Associativy.Services;
using Neo4jClient;
using Neo4jClient.Gremlin;
using Orchard.Logging;

namespace Associativy.Neo4j.Services
{
    public class Neo4jConnectionManager : Neo4jServiceBase, INeo4jConnectionManager
    {
        private readonly IExternalGraphStatisticsService _statisticsService;
        private readonly INeo4jGraphInfoService _infoService;
        private readonly IGraphCacheService _cacheService;
        private readonly IGraphEventHandler _graphEventHandler;

        private const string CacheKeyPrefix = "Associativy.Neo4j.Neo4jConnectionManager.";


        public Neo4jConnectionManager(
            IGraphDescriptor graphDescriptor,
            Uri rootUri,
            INeo4jGraphClientPool graphClientPool,
            Func<IGraphDescriptor, IExternalGraphStatisticsService> statisticsService,
            INeo4jGraphInfoService infoService,
            IGraphCacheService cacheService,
            IGraphEventHandler graphEventHandler)
            : base(graphDescriptor, rootUri, graphClientPool)
        {
            _statisticsService = statisticsService(_graphDescriptor);
            _infoService = infoService;
            _cacheService = cacheService;
            _graphEventHandler = graphEventHandler;

            Logger = NullLogger.Instance;
        }


        public bool AreNeighbours(int node1Id, int node2Id)
        {
            if (node1Id == node2Id) return true;

            return _cacheService.GetMonitored(_graphDescriptor, MakeCacheKey("AreNeighbours." + node1Id + "/" + node2Id), () =>
                {
                    TryInit();

                    var node1Reference = GetNodeReference(node1Id);
                    if (node1Reference == null) return false;
                    return node1Reference.Both<AssociativyNode>(WellKnownConstants.RelationshipTypeKey, node => node.Id == node2Id).GremlinCount() != 0;
                });
        }

        public void Connect(int node1Id, int node2Id)
        {
            TryInit();

            if (AreNeighbours(node1Id, node2Id)) return;

            Func<int, NodeReference<AssociativyNode>> addOrGetNodeReference =
                (nodeId) =>
                {
                    var existingNode = GetNodeReference(nodeId);
                    if (existingNode != null) return existingNode;

                    var node = new AssociativyNode(nodeId);
                    var indexEntry = new IndexEntry(WellKnownConstants.NodeIdIndexName)
                        {
                            { "id", nodeId }
                        };

                    _statisticsService.AdjustNodeCount(1);
                    return _graphClient.Create<AssociativyNode>(node, null, new IndexEntry[] { indexEntry });
                };

            var node1 = addOrGetNodeReference(node1Id);
            var node2 = addOrGetNodeReference(node2Id);
            _graphClient.CreateRelationship(node1, new AssociativyNodeRelationship(node2));

            var info = _infoService.GetGraphInfo(_graphDescriptor.Name);
            var node1NeighbourCount = CountEdges(node1);
            if (node1NeighbourCount > info.BiggestNodeNeighbourCount) SetBiggestNode(node1Id, node1NeighbourCount);
            else
            {
                var node2NeighbourCount = CountEdges(node2);
                if (node2NeighbourCount > info.BiggestNodeNeighbourCount) SetBiggestNode(node2Id, node2NeighbourCount);
            }

            _statisticsService.AdjustConnectionCount(1);

            _graphEventHandler.ConnectionAdded(_graphDescriptor, node1Id, node2Id);
        }

        public void DeleteFromNode(int nodeId)
        {
            TryInit();

            var nodeReference = GetNodeReference(nodeId);
            if (nodeReference == null) return;

            _statisticsService.AdjustConnectionCount(-CountEdges(nodeReference));
            _statisticsService.AdjustNodeCount(-1);
            _graphClient.Delete(nodeReference, DeleteMode.NodeAndRelationships);

            var info = _infoService.GetGraphInfo(_graphDescriptor.Name);
            if (info.BiggestNodeId == nodeId) FindBiggestNode();

            _graphEventHandler.ConnectionsDeletedFromNode(_graphDescriptor, nodeId);
        }

        public void Disconnect(int node1Id, int node2Id)
        {
            TryInit();

            if (!AreNeighbours(node1Id, node2Id)) return;

            var nodeReference = GetNodeReference(node1Id);

            if (nodeReference == null) return;

            _graphClient.DeleteRelationship(
                nodeReference
                .Both<AssociativyNode>(WellKnownConstants.RelationshipTypeKey, node => node.Id == node2Id).Single()
                .BackE(WellKnownConstants.RelationshipTypeKey).Single().Reference);

            if (CountEdges(nodeReference) == 0)
            {
                _graphClient.Delete(nodeReference, DeleteMode.NodeAndRelationships);
                _statisticsService.AdjustNodeCount(-1);
            }

            var info = _infoService.GetGraphInfo(_graphDescriptor.Name);
            if (info.BiggestNodeId == node1Id || info.BiggestNodeId == node2Id) FindBiggestNode();

            _statisticsService.AdjustConnectionCount(-1);

            _graphEventHandler.ConnectionDeleted(_graphDescriptor, node1Id, node2Id);
        }

        public IEnumerable<INodeToNodeConnector> GetAll(int skip, int count)
        {
            return _cacheService.GetMonitored(_graphDescriptor, MakeCacheKey("GetAll." + skip + "/" + count), () =>
                {
                    TryInit();

                    var connections = _graphClient.Cypher
                                            .Start("node1", "node(*)")
                                            .Match("(node1)-[:" + WellKnownConstants.RelationshipTypeKey + "]->(node2)")
                                            .Return((node1, node2) =>
                                                new AssociativyNodeConnection
                                                {
                                                    Node1 = node1.As<AssociativyNode>(),
                                                    Node2 = node2.As<AssociativyNode>()
                                                })
                                            .Skip(skip)
                                            .Limit(count)
                                            .Results;

                    var connectors = new List<INodeToNodeConnector>();
                    foreach (var connection in connections)
                    {
                        connectors.Add(new NodeConnector { Node1Id = connection.Node1.Id, Node2Id = connection.Node2.Id });
                    }

                    return connectors;
                });
        }

        public IEnumerable<int> GetNeighbourIds(int nodeId, int skip, int count)
        {
            return _cacheService.GetMonitored(_graphDescriptor, MakeCacheKey("GetNeighbourIds." + nodeId + "/" + skip + "/" + count), () =>
                {
                    TryInit();

                    var nodeReference = GetNodeReference(nodeId);
                    if (nodeReference == null) return Enumerable.Empty<int>();

                    return nodeReference.Both<AssociativyNode>(WellKnownConstants.RelationshipTypeKey).GremlinSkip(skip).GremlinTake(count).Select(node => node.Data.Id);
                });
        }

        public int GetNeighbourCount(int nodeId)
        {
            return _cacheService.GetMonitored(_graphDescriptor, MakeCacheKey("GetNeighbourCount." + nodeId), () =>
                {
                    TryInit();

                    var nodeReference = GetNodeReference(nodeId);
                    if (nodeReference == null) return 0;

                    return CountEdges(nodeReference);
                });
        }

        public IGraphInfo GetGraphInfo()
        {
            return _statisticsService.GetGraphInfo();
        }

        public Task RebuildStatisticsAsync()
        {
            TryInit();

            var nodeCountTask = _graphClient.Cypher
                                .Start("n", "node(*)")
                                .Return<int>("count(*)")
                                .ResultsAsync;

            var connectionCountTask = _graphClient.Cypher
                                        .Start("n", "node(*)")
                                        .Match("(n)-[:ASSOCIATIVY_CONNECTION]->()")
                                        .Return<int>("count(*)")
                                        .ResultsAsync;

            // Rewrite this to use new .NET 4.5 constructs after upgrade

            return Task.Factory.ContinueWhenAll(
                new[] { nodeCountTask, connectionCountTask },
                tasks =>
                {
                    var nodeCount = tasks[0].Result.FirstOrDefault() - 1; // Removing default root node
                    var connectionCount = tasks[1].Result.FirstOrDefault();

                    var biggestNodeTask = GetBiggestNodeAsync();
                    biggestNodeTask.ContinueWith(task =>
                        {
                            var biggestNode = task.Result;

                            if (biggestNode == null)
                            {
                                biggestNode = new BiggestNode { Node = new AssociativyNode(0), NeighbourCount = 0 };
                            }

                            var info = _infoService.GetGraphInfo(_graphDescriptor.Name);
                            info.BiggestNodeId = biggestNode.Node.Id;
                            info.BiggestNodeNeighbourCount = biggestNode.NeighbourCount;

                            _statisticsService.SetCentralNodeId(info.BiggestNodeId);
                            _statisticsService.SetConnectionCount(connectionCount);
                            _statisticsService.SetNodeCount(nodeCount);
                        });
                });
        }


        protected override void TryInit()
        {
            if (_graphClient != null) return;

            base.TryInit();

            if (!_graphClient.CheckIndexExists(WellKnownConstants.NodeIdIndexName, IndexFor.Node))
            {
                _graphClient.CreateIndex(WellKnownConstants.NodeIdIndexName, new IndexConfiguration { Provider = IndexProvider.lucene, Type = IndexType.exact }, IndexFor.Node);
            }
        }

        protected NodeReference<AssociativyNode> GetNodeReference(int nodeId)
        {
            var existingNode = _graphClient.QueryIndex<AssociativyNode>(WellKnownConstants.NodeIdIndexName, IndexFor.Node, "id:" + nodeId).SingleOrDefault();
            if (existingNode != null) return existingNode.Reference;
            return null;
        }

        protected Task<BiggestNode> GetBiggestNodeAsync()
        {
            var biggestNodeTask = _graphClient.Cypher
                                        .Start("Node", "node(*)")
                                        .Match("(Node)-[c:" + WellKnownConstants.RelationshipTypeKey + "]-()")
                                        .Return<BiggestNode>("count(c) AS NeighbourCount, Node", Neo4jClient.Cypher.CypherResultMode.Projection)
                                        .OrderByDescending("NeighbourCount")
                                        .Limit(1)
                                        .ResultsAsync;

            return biggestNodeTask.ContinueWith(task =>
                {
                    return task.Result.FirstOrDefault();
                });
        }

        protected void FindBiggestNode()
        {
            var biggestNode = GetBiggestNodeAsync().Result;

            if (biggestNode != null) SetBiggestNode(biggestNode.Node.Id, biggestNode.NeighbourCount);
            else SetBiggestNode(0, 0);
        }

        protected void SetBiggestNode(int id, int neighbourCount)
        {
            var info = _infoService.GetGraphInfo(_graphDescriptor.Name);
            info.BiggestNodeId = id;
            info.BiggestNodeNeighbourCount = neighbourCount;
            _statisticsService.SetCentralNodeId(id);
        }

        protected string MakeCacheKey(string name)
        {
            return CacheKeyPrefix + _graphDescriptor.Name + "." + name;
        }


        protected static int CountEdges(NodeReference<AssociativyNode> nodeReference)
        {
            return nodeReference.BothE(WellKnownConstants.RelationshipTypeKey).GremlinCount();
        }


        public class AssociativyNodeConnection
        {
            public AssociativyNode Node1 { get; set; }
            public AssociativyNode Node2 { get; set; }
        }

        public class BiggestNode
        {
            public int NeighbourCount { get; set; }
            public AssociativyNode Node { get; set; }
        }
    }
}