using Associativy.GraphDiscovery;
using Associativy.Instances.Notions;
using Autofac;
using Orchard.ContentManagement;

namespace Associativy.Neo4j.Tests.Helpers
{
    internal class TestGraphHelper
    {
        public static IGraphContext TestGraphContext()
        {
            return new GraphContext { Name = "Neo4jTestGraph", ContentTypes = new string[] { "Notion" } };
        }
    }
}
