using Neo4jClient;

namespace Associativy.Neo4j.Models.Neo4j
{
    public class AssociativyNodeRelationship : Relationship, IRelationshipAllowingSourceNode<AssociativyNode>, IRelationshipAllowingTargetNode<AssociativyNode>
    {
        public AssociativyNodeRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }

        public override string RelationshipTypeKey
        {
            get { return WellKnownConstants.RelationshipTypeKey; }
        }
    }
}