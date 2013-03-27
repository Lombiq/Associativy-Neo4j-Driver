using System.ComponentModel.DataAnnotations;

namespace Associativy.Neo4j.Models
{
    public class GraphInfoRecord : IGraphInfo
    {
        public virtual int Id { get; set; }
        [StringLength(1024)]
        public virtual string GraphName { get; set; }
        public virtual int BiggestNodeId { get; set; }
        public virtual int BiggestNodeNeighbourCount { get; set; }
    }
}