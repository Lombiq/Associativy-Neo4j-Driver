using System;
using System.Collections.Generic;
using System.Linq;
using Associativy.EventHandlers;
using Associativy.GraphDiscovery;
using Associativy.Models;
using Associativy.Models.Nodes;
using Associativy.Services;
using Neo4jClient;
using Neo4jClient.Gremlin;

namespace Associativy.Neo4j.Services
{
    public class Neo4jConnectionManager : GraphServiceBase, INeo4jConnectionManager
    {
        private readonly Uri _rootUri;
        private readonly INeo4jGraphClientPool _graphClientPool;
        private readonly IGraphEventHandler _graphEventHandler;
        private IGraphClient _graphClient;
        private const string NodeIdIndexName = "NodeIds";


        public Neo4jConnectionManager(
            IGraphDescriptor graphDescriptor,
            Uri rootUri,
            INeo4jGraphClientPool graphClientPool,
            IGraphEventHandler graphEventHandler)
            : base(graphDescriptor)
        {
            _rootUri = rootUri;
            _graphClientPool = graphClientPool;
            _graphEventHandler = graphEventHandler;
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

            _graphClient.CreateRelationship(AddOrGetNodeReference(node1Id), new AssociativyNodeRelationship(AddOrGetNodeReference(node2Id)));
            _graphEventHandler.ConnectionAdded(_graphDescriptor, node1Id, node2Id);
        }

        public void DeleteFromNode(int nodeId)
        {
            TryInit();

            var nodeReference = GetNodeReference(nodeId);
            if (nodeReference == null) return;

            _graphClient.Delete(nodeReference, DeleteMode.NodeAndRelationships);
            _graphEventHandler.ConnectionsDeletedFromNode(_graphDescriptor, nodeId);
        }

        public void Disconnect(int node1Id, int node2Id)
        {
            TryInit();

            if (!AreNeighbours(node1Id, node2Id)) return;

            _graphClient.DeleteRelationship(
                GetNodeReference(node1Id)
                .Both<AssociativyNode>(AssociativyNodeRelationship.TypeKey, node => node.Id == node2Id).Single()
                .BackE(AssociativyNodeRelationship.TypeKey).Single().Reference);

            _graphEventHandler.ConnectionDeleted(_graphDescriptor, node1Id, node2Id);
        }

        public IEnumerable<INodeToNodeConnector> GetAll(int skip, int count)
        {
            TryInit();

            var connections = _graphClient.Cypher
                                    .StartWithNodeIndexLookup("node1", NodeIdIndexName, "id:*")
                                    .Match("(node1)-[:" + AssociativyNodeRelationship.TypeKey + "]->(node2)")
                                    .Return((node1, node2) =>
                                        new AssociativyNodeConnection
                                        {
                                            Node1 = node1.As<AssociativyNode>(),
                                            Node2 = node2.As<AssociativyNode>()
                                        })
                                    .Skip(skip)
                                    .Limit(count).Results;

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

            return nodeReference.BothE(AssociativyNodeRelationship.TypeKey).GremlinCount();
        }


        private void TryInit()
        {
            if (_graphClient != null) return;

            if (_rootUri == null) throw new InvalidOperationException("The RootUri property should be set before the connection manager intance can be used.");

            _graphClient = _graphClientPool.GetClient(_rootUri);
            if (!_graphClient.CheckIndexExists(NodeIdIndexName, IndexFor.Node))
            {
                _graphClient.CreateIndex(NodeIdIndexName, new IndexConfiguration { Provider = IndexProvider.lucene, Type = IndexType.exact }, IndexFor.Node);
            }
        }

        private NodeReference<AssociativyNode> AddOrGetNodeReference(int nodeId)
        {
            var existingNode = GetNodeReference(nodeId);
            if (existingNode != null) return existingNode;

            var node = new AssociativyNode(nodeId);
            var indexEntry = new IndexEntry(NodeIdIndexName)
                {
                    { "id", nodeId }
                };
            return _graphClient.Create<AssociativyNode>(node, null, new IndexEntry[] { indexEntry });
        }

        private NodeReference<AssociativyNode> GetNodeReference(int nodeId)
        {
            var existingNode = _graphClient.QueryIndex<AssociativyNode>(NodeIdIndexName, IndexFor.Node, "id:" + nodeId).SingleOrDefault();
            if (existingNode != null) return existingNode.Reference;
            return null;
        }
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
}