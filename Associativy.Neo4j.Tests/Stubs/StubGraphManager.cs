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
        private Func<IGraphDescriptor, IGraphServices> _graphServicesFactory;

        public StubGraphManager()
        {
            if (_graphServicesFactory == null)
            {
                var clientPool = new StubClientPool();
                var dummyRootUri = new Uri("http://google.com"); // A real URL is given by StubClientPool
                var connectionManager = new Neo4jConnectionManager(TestGraphDescriptor(), dummyRootUri, clientPool);

                _graphServicesFactory = graphDescriptor => new GraphServices(
                    new Mock<IMind>().Object,
                    connectionManager,
                    new Neo4jPathFinder(TestGraphDescriptor(), dummyRootUri, clientPool, new Mock<IGraphEventMonitor>().Object, new StubCacheManager()),
                    new Mock<INodeManager>().Object,
                    new Mock<IGraphStatisticsService>().Object);
            }
        }

        public IGraphDescriptor FindGraph(IGraphContext graphContext)
        {
            return TestGraphDescriptor();
        }

        public IEnumerable<IGraphDescriptor> FindGraphs(IGraphContext graphContext)
        {
            return new GraphDescriptor[] { TestGraphDescriptor() };
        }

        public IEnumerable<IGraphDescriptor> FindDistinctGraphs(IGraphContext graphContext)
        {
            return new GraphDescriptor[] { TestGraphDescriptor() };
        }

        private GraphDescriptor TestGraphDescriptor()
        {
            return new GraphDescriptor(
                TestGraphHelper.TestGraphContext().Name,
                new Orchard.Localization.LocalizedString("Neo4j Test"),
                TestGraphHelper.TestGraphContext().ContentTypes,
                _graphServicesFactory);
        }
    }
}
