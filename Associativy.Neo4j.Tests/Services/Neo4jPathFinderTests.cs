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
using Associativy.Tests.Services;

namespace Associativy.Neo4j.Tests.Services
{
    [TestFixture]
    public class Neo4jPathFinderTests : PathFinderTestsBase
    {
        [SetUp]
        public override void Init()
        {
            base.Init();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(new Uri("http://google.com")).As<Uri>(); // A real URL is given by StubClientPool
            builder.RegisterInstance(new StubClientPool()).As<INeo4jGraphClientPool>();
            builder.RegisterInstance(new StubExternalGraphStatisticsService()).As<IExternalGraphStatisticsService>();
            builder.RegisterInstance(new StubNeo4jGraphInfoService()).As<INeo4jGraphInfoService>();
            builder.RegisterType<Neo4jConnectionManager>().As<IConnectionManager>();
            builder.RegisterType<Neo4jPathFinder>().As<IPathFinder>();
            
            builder.Update(_container);
        }
    }
}
