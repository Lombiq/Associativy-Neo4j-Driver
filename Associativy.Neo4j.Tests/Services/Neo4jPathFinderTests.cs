using System.Collections.Generic;
using System.Linq;
using Associativy.EventHandlers;
using Associativy.GraphDiscovery;
using Associativy.Services;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.ContentManagement.Records;
using Orchard.Core.Settings.Metadata;
using Orchard.Data;
using Orchard.FileSystems.AppData;
using Orchard.Tests;
using Orchard.Tests.ContentManagement;
using Orchard.Tests.UI.Navigation;
using Orchard.Tests.Utility;
using QuickGraph;
using Associativy.Tests.Helpers;
using Associativy.Tests;
using Associativy.Neo4j.Tests.Stubs;
using Associativy.Neo4j.Services;
using Associativy.Models.Services;
using System;
using Associativy.Tests.Stubs;
using Associativy.Queryable;

namespace Associativy.Neo4j.Tests.Services
{
    [TestFixture]
    public class Neo4jPathFinderTests : ContentManagerEnabledTestBase
    {
        private IGraphDescriptor _graphDescriptor;
        private IContentManager _contentManager;

        [SetUp]
        public override void Init()
        {
            base.Init();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(new Uri("http://google.com")).As<Uri>(); // A real URL is given by StubClientPool
            builder.RegisterInstance(new StubClientPool()).As<INeo4jGraphClientPool>();
            builder.RegisterInstance(new StubExternalGraphStatisticsService()).As<IExternalGraphStatisticsService>();
            builder.RegisterInstance(new StubNeo4jGraphInfoService()).As<INeo4jGraphInfoService>();
            builder.RegisterInstance(new StubGraphEditor()).As<IGraphEditor>();
            builder.RegisterInstance(new StubQueryableGraphFactory()).As<IQueryableGraphFactory>();
            builder.RegisterType<StubGraphManager>().As<IGraphManager>();
            builder.RegisterType<PathFinderAuxiliaries>().As<IPathFinderAuxiliaries>();
            builder.RegisterType<Neo4jConnectionManager>().As<IConnectionManager>();
            builder.RegisterType<Neo4jPathFinder>().As<IPathFinder>();
            
            StubGraphManager.Setup(builder);

            builder.Update(_container);

            _graphDescriptor = _container.Resolve<IGraphManager>().FindGraph(null);
            _contentManager = _container.Resolve<IContentManager>();
        }

        [TestFixtureTearDown]
        public void Clean()
        {
        }

        [Test]
        public void SinglePathsAreFound()
        {
            var nodes = TestGraphHelper.BuildTestGraph(_contentManager, _graphDescriptor).Nodes;

            var result = CalcPathResult(nodes["medicine"], nodes["colour"]);
            var succeededGraph = result.SucceededGraph.ToGraph();
            var succeededPaths = result.SucceededPaths;

            var rightPath = new IContent[] { nodes["medicine"], nodes["cyanide"], nodes["cyan"], nodes["colour"] };

            Assert.That(succeededGraph.VertexCount, Is.EqualTo(4));
            Assert.That(succeededGraph.EdgeCount, Is.EqualTo(3));

            Assert.That(PathVerifier.PathExistsInGraph(succeededGraph, rightPath), Is.True);

            Assert.That(succeededPaths.Count(), Is.EqualTo(1));
            Assert.That(PathVerifier.VerifyPath(succeededPaths.First(), rightPath), Is.True);
        }

        [Test]
        public void SinglePathsAreFound2()
        {
            var nodes = TestGraphHelper.BuildTestGraph(_contentManager, _graphDescriptor).Nodes;

            var result = CalcPathResult(nodes["American"], nodes["writer"]);
            var succeededGraph = result.SucceededGraph.ToGraph();
            var succeededPaths = result.SucceededPaths;

            var rightPath = new IContent[] { nodes["American"], nodes["Ernest Hemingway"], nodes["writer"] };

            Assert.That(succeededGraph.VertexCount, Is.EqualTo(3));
            Assert.That(succeededGraph.EdgeCount, Is.EqualTo(2));

            Assert.That(PathVerifier.PathExistsInGraph(succeededGraph, rightPath), Is.True);

            Assert.That(succeededPaths.Count(), Is.EqualTo(1));
            Assert.That(PathVerifier.VerifyPath(succeededPaths.First(), rightPath), Is.True);
        }

        [Test]
        public void DualPathsAreFound()
        {
            var nodes = TestGraphHelper.BuildTestGraph(_contentManager, _graphDescriptor).Nodes;

            var result = CalcPathResult(nodes["yellow"], nodes["light year"]);
            var succeededGraph = result.SucceededGraph.ToGraph();
            var succeededPaths = result.SucceededPaths.ToList();

            var rightPath1 = new IContent[] { nodes["yellow"], nodes["sun"], nodes["light"], nodes["light year"] };
            var rightPath2 = new IContent[] { nodes["yellow"], nodes["colour"], nodes["light"], nodes["light year"] };

            Assert.That(succeededGraph.VertexCount, Is.EqualTo(5));
            Assert.That(succeededGraph.EdgeCount, Is.EqualTo(5));

            Assert.That(PathVerifier.PathExistsInGraph(succeededGraph, rightPath1), Is.True);
            Assert.That(PathVerifier.PathExistsInGraph(succeededGraph, rightPath2), Is.True);

            Assert.That(succeededPaths.Count, Is.EqualTo(2));

            // The order of found paths is not fixed.
            Assert.That(PathVerifier.VerifyPath(succeededPaths[0], rightPath1) || PathVerifier.VerifyPath(succeededPaths[0], rightPath2), Is.True);
            Assert.That(PathVerifier.VerifyPath(succeededPaths[1], rightPath1) || PathVerifier.VerifyPath(succeededPaths[1], rightPath2), Is.True);
        }

        [Test]
        public void TooLongPathsAreNotFound()
        {
            var nodes = TestGraphHelper.BuildTestGraph(_contentManager, _graphDescriptor).Nodes;

            var result = CalcPathResult(nodes["blue"], nodes["medicine"]);
            var succeededGraph = result.SucceededGraph.ToGraph();
            var succeededPaths = result.SucceededPaths;

            Assert.That(succeededGraph.VertexCount, Is.EqualTo(0));
            Assert.That(succeededGraph.EdgeCount, Is.EqualTo(0));

            Assert.That(succeededPaths.Count(), Is.EqualTo(0));
        }

        [Test]
        public void NotConnectedPathsAreNotFound()
        {
            var nodes = TestGraphHelper.BuildTestGraph(_contentManager, _graphDescriptor).Nodes;

            var result = CalcPathResult(nodes["writer"], nodes["plant"]);
            var succeededGraph = result.SucceededGraph.ToGraph();
            var succeededPaths = result.SucceededPaths;

            Assert.That(succeededGraph.VertexCount, Is.EqualTo(0));
            Assert.That(succeededGraph.EdgeCount, Is.EqualTo(0));

            Assert.That(succeededPaths.Count(), Is.EqualTo(0));
        }

        public IPathResult CalcPathResult(IContent node1, IContent node2)
        {
            return _graphDescriptor.Services.PathFinder.FindPaths(node1.Id, node2.Id, PathFinderSettings.Default);
        }
    }
}
