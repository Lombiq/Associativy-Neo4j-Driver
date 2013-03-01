using System;
using System.Collections.Generic;
using System.Linq;
using Associativy.GraphDiscovery;
using Associativy.Models;
using Associativy.Models.Nodes;
using Neo4jClient;
using Neo4jClient.Gremlin;

namespace Associativy.Neo4j.Services
{
    public class Neo4jConnectionManager : Neo4jService, INeo4jConnectionManager
    {
        private readonly INeo4jGraphClientPool _graphClientPool;
        private IGraphClient _graphClient;
        private const string _nodeIdIndexName = "NodeIds";


        public Neo4jConnectionManager(INeo4jGraphClientPool graphClientPool)
        {
            _graphClientPool = graphClientPool;
        }


        public bool AreNeighbours(IGraphContext graphContext, int node1Id, int node2Id)
        {
            if (node1Id == node2Id) return true;

            TryInit();

            var node1Reference = GetNodeReference(node1Id);
            if (node1Reference == null) return false;
            return node1Reference.Both<AssociativyNode>(AssociativyNodeConnection.TypeKey, node => node.Id == node2Id).GremlinCount() != 0;
        }

        public void Connect(IGraphContext graphContext, int node1Id, int node2Id)
        {
            TryInit();

            if (AreNeighbours(graphContext, node1Id, node2Id)) return;

            _graphClient.CreateRelationship(AddOrGetNodeReference(node1Id), new AssociativyNodeConnection(AddOrGetNodeReference(node2Id)));
        }

        public void DeleteFromNode(IGraphContext graphContext, int nodeId)
        {
            TryInit();

            var nodeReference = GetNodeReference(nodeId);
            if (nodeReference == null) return;

            _graphClient.Delete(nodeReference, DeleteMode.NodeAndRelationships);
        }

        public void Disconnect(IGraphContext graphContext, int node1Id, int node2Id)
        {
            TryInit();

            if (!AreNeighbours(graphContext, node1Id, node2Id)) return;

            _graphClient.DeleteRelationship(
                GetNodeReference(node1Id)
                .Both<AssociativyNode>(AssociativyNodeConnection.TypeKey, node => node.Id == node2Id).Single()
                .BackE(AssociativyNodeConnection.TypeKey).Single().Reference);
        }

        public IEnumerable<INodeToNodeConnector> GetAll(IGraphContext graphContext)
        {
            TryInit();

            var connections = new Dictionary<int, HashSet<int>>();

            foreach (var node in _graphClient.QueryIndex<AssociativyNode>(_nodeIdIndexName, IndexFor.Node, "id:*"))
            {
                var nodeId = node.Data.Id;
                if (!connections.ContainsKey(nodeId)) connections[nodeId] = new HashSet<int>();

                var nodeConnections = connections[nodeId];
                foreach (var otherNodeId in node.Out<AssociativyNode>(AssociativyNodeConnection.TypeKey).Select(n => n.Data.Id))
                {
                    nodeConnections.Add(otherNodeId);
                }
            }

            var connectors = new List<INodeToNodeConnector>(connections.Count * 2);
            foreach (var connection in connections)
            {
                foreach (var innerConnection in connection.Value)
                {
                    connectors.Add(new NodeConnector { Node1Id = connection.Key, Node2Id = innerConnection });
                }
            }

            return connectors;
        }

        public IEnumerable<int> GetNeighbourIds(IGraphContext graphContext, int nodeId)
        {
            TryInit();

            var nodeReference = GetNodeReference(nodeId);
            if (nodeReference == null) return Enumerable.Empty<int>();

            return nodeReference.Both<AssociativyNode>(AssociativyNodeConnection.TypeKey).Select(node => node.Data.Id);
        }

        public int GetNeighbourCount(IGraphContext graphContext, int nodeId)
        {
            TryInit();

            var nodeReference = GetNodeReference(nodeId);
            if (nodeReference == null) return 0;

            return nodeReference.BothE(AssociativyNodeConnection.TypeKey).GremlinCount();
        }


        private void TryInit()
        {
            if (_graphClient != null) return;

            if (_rootUri == null) throw new InvalidOperationException("The RootUri property should be set before the connection manager intance can be used.");

            _graphClient = _graphClientPool.GetClient(_rootUri);
            if (!_graphClient.CheckIndexExists(_nodeIdIndexName, IndexFor.Node))
            {
                _graphClient.CreateIndex(_nodeIdIndexName, new IndexConfiguration { Provider = IndexProvider.lucene, Type = IndexType.exact }, IndexFor.Node);
            }
        }

        private NodeReference<AssociativyNode> AddOrGetNodeReference(int nodeId)
        {
            var existingNode = GetNodeReference(nodeId);
            if (existingNode != null) return existingNode;

            var node = new AssociativyNode(nodeId);
            var indexEntry = new IndexEntry(_nodeIdIndexName)
                {
                    { "id", nodeId }
                };
            return _graphClient.Create<AssociativyNode>(node, null, new IndexEntry[] { indexEntry });
        }

        private NodeReference<AssociativyNode> GetNodeReference(int nodeId)
        {
            var existingNode = _graphClient.QueryIndex<AssociativyNode>(_nodeIdIndexName, IndexFor.Node, "id:" + nodeId).SingleOrDefault();
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

    public class AssociativyNodeConnection : Relationship, IRelationshipAllowingSourceNode<AssociativyNode>, IRelationshipAllowingTargetNode<AssociativyNode>
    {
        public AssociativyNodeConnection(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public const string TypeKey = "CONNECTED";
        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }

}