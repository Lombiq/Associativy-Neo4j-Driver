using System;
using System.Collections.Generic;
using System.Linq;
using Associativy.EventHandlers;
using Associativy.GraphDiscovery;
using Associativy.Models;
using Associativy.Models.Nodes;
using Associativy.Models.Services;
using Associativy.Services;
using Neo4jClient;
using Neo4jClient.Gremlin;
using Orchard.Exceptions;
using Orchard.Logging;

namespace Associativy.Neo4j.Services
{
    public class Neo4jConnectionManager : GraphAwareServiceBase, INeo4jConnectionManager
    {
        private readonly Uri _rootUri;
        private readonly INeo4jGraphClientPool _graphClientPool;
        private readonly IExternalGraphStatisticsService _statisticsService;
        private readonly INeo4jGraphInfoService _infoService;
        private readonly IGraphEventHandler _graphEventHandler;
        private IGraphClient _graphClient;
        private const string NodeIdIndexName = "NodeIds";

        public ILogger Logger { get; set; }


        public Neo4jConnectionManager(
            IGraphDescriptor graphDescriptor,
            Uri rootUri,
            INeo4jGraphClientPool graphClientPool,
            Func<IGraphDescriptor, IExternalGraphStatisticsService> statisticsService,
            INeo4jGraphInfoService infoService,
            IGraphEventHandler graphEventHandler)
            : base(graphDescriptor)
        {
            _rootUri = rootUri;
            _graphClientPool = graphClientPool;
            _statisticsService = statisticsService(_graphDescriptor);
            _infoService = infoService;
            _graphEventHandler = graphEventHandler;

            Logger = NullLogger.Instance;
        }


        public bool AreNeighbours(int node1Id, int node2Id)
        {
            if (node1Id == node2Id) return true;

            TryInit();

            var node1Reference = GetNodeReference(node1Id);
            if (node1Reference == null) return false;
            return node1Reference.Both<AssociativyNode>(AssociativyNodeRelationship.TypeKey, node => node.Id == node2Id).GremlinCount() != 0;
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
                    var indexEntry = new IndexEntry(NodeIdIndexName)
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
                .Both<AssociativyNode>(AssociativyNodeRelationship.TypeKey, node => node.Id == node2Id).Single()
                .BackE(AssociativyNodeRelationship.TypeKey).Single().Reference);

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
            TryInit();

            var connections = _graphClient.Cypher
                                    .Start("node1", "node(*)")
                                    .Match("(node1)-[:" + AssociativyNodeRelationship.TypeKey + "]->(node2)")
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
        }

        public IEnumerable<int> GetNeighbourIds(int nodeId, int skip, int count)
        {
            TryInit();

            var nodeReference = GetNodeReference(nodeId);
            if (nodeReference == null) return Enumerable.Empty<int>();

            return nodeReference.Both<AssociativyNode>(AssociativyNodeRelationship.TypeKey).GremlinSkip(skip).GremlinTake(count).Select(node => node.Data.Id);
        }

        public int GetNeighbourCount(int nodeId)
        {
            TryInit();

            var nodeReference = GetNodeReference(nodeId);
            if (nodeReference == null) return 0;

            return CountEdges(nodeReference);
        }

        public IGraphInfo GetGraphInfo()
        {
            return _statisticsService.GetGraphInfo();
        }


        protected void TryInit()
        {
            if (_graphClient != null) return;

            if (_rootUri == null) throw new InvalidOperationException("The RootUri property should be set before the connection manager intance can be used.");

            try
            {
                _graphClient = _graphClientPool.GetClient(_rootUri);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal()) throw;

                var message = "Acquiring a graph client for the graph " + _graphDescriptor.Name + " with the url " + _rootUri + " failed.";
                Logger.Error(ex, message);
                throw new ApplicationException(message, ex);
            }

            if (!_graphClient.CheckIndexExists(NodeIdIndexName, IndexFor.Node))
            {
                _graphClient.CreateIndex(NodeIdIndexName, new IndexConfiguration { Provider = IndexProvider.lucene, Type = IndexType.exact }, IndexFor.Node);
            }
        }

        protected NodeReference<AssociativyNode> GetNodeReference(int nodeId)
        {
            var existingNode = _graphClient.QueryIndex<AssociativyNode>(NodeIdIndexName, IndexFor.Node, "id:" + nodeId).SingleOrDefault();
            if (existingNode != null) return existingNode.Reference;
            return null;
        }

        protected void FindBiggestNode()
        {
            var biggestNode = _graphClient.Cypher
                                    .Start("Node", "node(*)")
                                    .Match("(Node)-[c:" + AssociativyNodeRelationship.TypeKey + "]-()")
                                    .Return<BiggestNode>("count(c) AS NeighbourCount, Node", Neo4jClient.Cypher.CypherResultMode.Projection)
                                    .OrderByDescending("NeighbourCount")
                                    .Limit(1)
                                    .Results
                                    .SingleOrDefault();

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


        protected static int CountEdges(NodeReference<AssociativyNode> nodeReference)
        {
            return nodeReference.BothE(AssociativyNodeRelationship.TypeKey).GremlinCount();
        }


        public class AssociativyNode
        {
            public int Id { get; set; }

            // For deserialization
            public AssociativyNode()
            {
            }

            public AssociativyNode(int id)
            {
                Id = id;
            }
        }

        public class AssociativyNodeRelationship : Relationship, IRelationshipAllowingSourceNode<AssociativyNode>, IRelationshipAllowingTargetNode<AssociativyNode>
        {
            public AssociativyNodeRelationship(NodeReference targetNode)
                : base(targetNode)
            {
            }

            public const string TypeKey = "ASSOCIATIVY_CONNECTION";
            public override string RelationshipTypeKey
            {
                get { return TypeKey; }
            }
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