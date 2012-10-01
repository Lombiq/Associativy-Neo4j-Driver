using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.GraphDiscovery;
using Orchard.Localization;
using Associativy.Services;
using Associativy.Neo4j.Services;
using Orchard.Environment;

namespace Associativy.Neo4j
{
    public class Neo4jTestGraphProvider : IGraphProvider
    {
        private readonly Func<IPathServices> _pathServicesFactory;

        public Localizer T { get; set; }

        public Neo4jTestGraphProvider(
            Work<INeo4jConnectionManager> connectionManagerWork,
            Work<INeo4jPathFinder> pathFinderWork)
        {
            _pathServicesFactory = () =>
            {
                var connectionManager = connectionManagerWork.Value;
                connectionManager.RootUri = new Uri("http://localhost:7474/db/data/");
                return new PathServices(connectionManager, pathFinderWork.Value);
            };

            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext describeContext)
        {
            describeContext.DescribeGraph(
                "Neo4jTest",
                T("Neo4j Test"),
                new[] { "Notion" },
                _pathServicesFactory);
        }
    }
}