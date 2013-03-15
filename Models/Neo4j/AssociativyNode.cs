using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Associativy.Neo4j.Models.Neo4j
{
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
}