
namespace Associativy.Neo4j.Models
{
    public interface IGraphInfo
    {
        int BiggestNodeId { get; set; }
        int BiggestNodeNeighbourCount { get; set; }
    }
}
