using System.Collections.Generic;
using Associativy.EventHandlers;
using Associativy.GraphDiscovery;
using Associativy.Services;
using Moq;
using Orchard.Tests.Stubs;
using Associativy.Neo4j.Services;
using Associativy.Tests.Stubs;
using System;
using Associativy.Neo4j.Tests.Helpers;

namespace Associativy.Neo4j.Tests.Stubs
{
    public class StubGraphManager : IGraphManager
    {
        private static PathServices _pathServices;

        public StubGraphManager()
        {
            if (_pathServices == null)
            {
                var clientPool = new StubClientPool();
                var connectionManager = new Neo4jConnectionManager(clientPool);
                connectionManager.RootUri = new Uri("http://google.com");

                _pathServices = new PathServices(
                    connectionManager,
                    new Neo4jPathFinder(clientPool, this, new StubGraphEditor(), new Mock<IGraphEventMonitor>().Object, new StubCacheManager()));
            }
        }

        public GraphDescriptor FindGraph(IGraphContext graphContext)
        {
            return TestGraphDescriptor();
        }

        public IEnumerable<GraphDescriptor> FindGraphs(IGraphContext graphContext)
        {
            return new GraphDescriptor[] { TestGraphDescriptor() };
        }

        public IEnumerable<GraphDescriptor> FindDistinctGraphs(IGraphContext graphContext)
        {
            return new GraphDescriptor[] { TestGraphDescriptor() };
        }

        private GraphDescriptor TestGraphDescriptor()
        {
            return new GraphDescriptor(
                TestGraphHelper.TestGraphContext().GraphName,
                new Orchard.Localization.LocalizedString("Neo4j Test"),
                TestGraphHelper.TestGraphContext().ContentTypes,
                () => _pathServices);
        }
    }
}
