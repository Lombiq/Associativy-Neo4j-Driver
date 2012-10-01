using System.Collections.Generic;
using System.Web.Mvc;
using Associativy.GraphDiscovery;
using Associativy.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Associativy.Neo4j.Services;
using System;
using Neo4jClient;
using System.Linq;

namespace Associativy.Instances.Notions.Controllers
{
    public class SetupController : Controller
    {
        private readonly IAssociativyServices _associativyServices;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;

        private readonly INeo4jGraphClientPool _graphClientRepository;

        public SetupController(
            IAssociativyServices associativyServices,
            IOrchardServices orchardServices,

            INeo4jGraphClientPool graphClientRepository)
        {
            _associativyServices = associativyServices;
            _contentManager = orchardServices.ContentManager;
            _orchardServices = orchardServices;

            _graphClientRepository = graphClientRepository;
        }

        public void Index()
        {
            var graphContext = new GraphContext { GraphName = "Neo4jTest" };
            var pathServices = _associativyServices.GraphManager.FindGraph(graphContext).PathServices;
            var connectionManager = pathServices.ConnectionManager;
            var pathFinder = pathServices.PathFinder;

        }

        public string Hungarian()
        {
            var graphContext = new GraphContext { GraphName = "Neo4jTest" };
            var connectionManager = _associativyServices.GraphManager.FindGraph(graphContext).PathServices.ConnectionManager;

            var nodes = new Dictionary<string, IContent>();

            nodes["jég"] = NewNotion();
            nodes["fagyott"] = NewNotion();
            nodes["fagylalt"] = NewNotion();
            nodes["téli fagylalt"] = NewNotion();
            nodes["tél"] = NewNotion();
            nodes["víz"] = NewNotion();
            nodes["nyár"] = NewNotion();
            nodes["meleg"] = NewNotion();
            nodes["forró"] = NewNotion();
            nodes["tűz"] = NewNotion();
            nodes["tűzhely"] = NewNotion();
            nodes["gőz"] = NewNotion();
            nodes["oxigén"] = NewNotion();
            nodes["levegő"] = NewNotion();
            nodes["folyó"] = NewNotion();
            nodes["nitrogén"] = NewNotion();
            nodes["hidrogén"] = NewNotion();
            nodes["föld"] = NewNotion();
            nodes["út"] = NewNotion();
            nodes["autó"] = NewNotion();
            nodes["téligumi"] = NewNotion();
            nodes["fagyálló"] = NewNotion();
            nodes["sofőr"] = NewNotion();
            nodes["kerék"] = NewNotion();
            nodes["Audi"] = NewNotion();
            nodes["Suzuki"] = NewNotion();
            nodes["Maserati"] = NewNotion();
            nodes["BMW"] = NewNotion();
            nodes["Mercedes"] = NewNotion();
            nodes["benzin"] = NewNotion();
            nodes["Volkswagen"] = NewNotion();
            nodes["Skoda"] = NewNotion();
            nodes["Honda"] = NewNotion();
            nodes["márka"] = NewNotion(); ;
            nodes["német"] = NewNotion();
            nodes["kaiserlich und königlich"] = NewNotion();
            nodes["kuk"] = NewNotion();
            nodes["mozaikszó"] = NewNotion();
            nodes["OSZK"] = NewNotion();
            nodes["Országos Széchenyi Könyvtár"] = NewNotion();
            nodes["gumi"] = NewNotion();
            nodes["rágógumi"] = NewNotion();
            nodes["kaucsuk"] = NewNotion();
            nodes["Japán"] = NewNotion();
            nodes["motorbicikli"] = NewNotion();
            nodes["mission accomplished"] = NewNotion();
            nodes["feladat befejezve"] = NewNotion();
            nodes["Nap"] = NewNotion();
            nodes["sárga"] = NewNotion();
            nodes["lila"] = NewNotion();
            nodes["orgona"] = NewNotion();
            nodes["virág"] = NewNotion();
            nodes["szín"] = NewNotion();
            nodes["őselem"] = NewNotion();
            nodes["Prométheusz"] = NewNotion();

            foreach (var node in nodes)
            {
                node.Value.As<TitlePart>().Title = node.Key;
                _contentManager.Create(node.Value);
            }

            connectionManager.Connect(graphContext, nodes["mission accomplished"], nodes["feladat befejezve"]);
            connectionManager.Connect(graphContext, nodes["Nap"], nodes["sárga"]);
            connectionManager.Connect(graphContext, nodes["lila"], nodes["orgona"]);
            connectionManager.Connect(graphContext, nodes["orgona"], nodes["virág"]);
            connectionManager.Connect(graphContext, nodes["szín"], nodes["lila"]);
            connectionManager.Connect(graphContext, nodes["szín"], nodes["sárga"]);
            connectionManager.Connect(graphContext, nodes["német"], nodes["Volkswagen"]);
            connectionManager.Connect(graphContext, nodes["német"], nodes["BMW"]);
            connectionManager.Connect(graphContext, nodes["német"], nodes["Audi"]);
            connectionManager.Connect(graphContext, nodes["német"], nodes["Mercedes"]);
            connectionManager.Connect(graphContext, nodes["német"], nodes["kaiserlich und königlich"]);
            connectionManager.Connect(graphContext, nodes["kaiserlich und königlich"], nodes["kuk"]);
            connectionManager.Connect(graphContext, nodes["mozaikszó"], nodes["kuk"]);
            connectionManager.Connect(graphContext, nodes["mozaikszó"], nodes["BMW"]);
            connectionManager.Connect(graphContext, nodes["mozaikszó"], nodes["OSZK"]);
            connectionManager.Connect(graphContext, nodes["Országos Széchenyi Könyvtár"], nodes["OSZK"]);
            connectionManager.Connect(graphContext, nodes["gumi"], nodes["téligumi"]);
            connectionManager.Connect(graphContext, nodes["gumi"], nodes["kerék"]);
            connectionManager.Connect(graphContext, nodes["gumi"], nodes["rágógumi"]);
            connectionManager.Connect(graphContext, nodes["gumi"], nodes["kaucsuk"]);
            connectionManager.Connect(graphContext, nodes["Honda"], nodes["Japán"]);
            connectionManager.Connect(graphContext, nodes["Honda"], nodes["motorbicikli"]);
            connectionManager.Connect(graphContext, nodes["Audi"], nodes["márka"]);
            connectionManager.Connect(graphContext, nodes["Suzuki"], nodes["márka"]);
            connectionManager.Connect(graphContext, nodes["Maserati"], nodes["márka"]);
            connectionManager.Connect(graphContext, nodes["BMW"], nodes["márka"]);
            connectionManager.Connect(graphContext, nodes["Mercedes"], nodes["márka"]);
            connectionManager.Connect(graphContext, nodes["Volkswagen"], nodes["márka"]);
            connectionManager.Connect(graphContext, nodes["Skoda"], nodes["márka"]);
            connectionManager.Connect(graphContext, nodes["Honda"], nodes["márka"]);
            connectionManager.Connect(graphContext, nodes["téligumi"], nodes["autó"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["fagyálló"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["sofőr"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["kerék"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["Suzuki"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["Maserati"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["Audi"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["BMW"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["Mercedes"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["benzin"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["Volkswagen"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["Skoda"]);
            connectionManager.Connect(graphContext, nodes["autó"], nodes["Honda"]);
            connectionManager.Connect(graphContext, nodes["oxigén"], nodes["levegő"]);
            connectionManager.Connect(graphContext, nodes["levegő"], nodes["őselem"]);
            connectionManager.Connect(graphContext, nodes["víz"], nodes["őselem"]);
            connectionManager.Connect(graphContext, nodes["folyó"], nodes["víz"]);
            connectionManager.Connect(graphContext, nodes["nitrogén"], nodes["levegő"]);
            connectionManager.Connect(graphContext, nodes["föld"], nodes["őselem"]);
            connectionManager.Connect(graphContext, nodes["út"], nodes["föld"]);
            connectionManager.Connect(graphContext, nodes["út"], nodes["autó"]);
            connectionManager.Connect(graphContext, nodes["nyár"], nodes["meleg"]);
            connectionManager.Connect(graphContext, nodes["nyár"], nodes["fagylalt"]);
            connectionManager.Connect(graphContext, nodes["forró"], nodes["meleg"]);
            connectionManager.Connect(graphContext, nodes["forró"], nodes["gőz"]);
            connectionManager.Connect(graphContext, nodes["forró"], nodes["tűzhely"]);
            connectionManager.Connect(graphContext, nodes["forró"], nodes["tűz"]);
            connectionManager.Connect(graphContext, nodes["tűz"], nodes["tűzhely"]);
            connectionManager.Connect(graphContext, nodes["tűz"], nodes["forró"]);
            connectionManager.Connect(graphContext, nodes["tűz"], nodes["őselem"]);
            connectionManager.Connect(graphContext, nodes["tűz"], nodes["Prométheusz"]);
            connectionManager.Connect(graphContext, nodes["tűzhely"], nodes["gőz"]);
            connectionManager.Connect(graphContext, nodes["jég"], nodes["tél"]);
            connectionManager.Connect(graphContext, nodes["jég"], nodes["fagyott"]);
            connectionManager.Connect(graphContext, nodes["fagyott"], nodes["fagylalt"]);
            connectionManager.Connect(graphContext, nodes["fagylalt"], nodes["jég"]);
            connectionManager.Connect(graphContext, nodes["téli fagylalt"], nodes["fagylalt"]);
            connectionManager.Connect(graphContext, nodes["téli fagylalt"], nodes["meleg"]);
            connectionManager.Connect(graphContext, nodes["tél"], nodes["téli fagylalt"]);
            connectionManager.Connect(graphContext, nodes["tél"], nodes["nyár"]);
            connectionManager.Connect(graphContext, nodes["tél"], nodes["téligumi"]);
            connectionManager.Connect(graphContext, nodes["tél"], nodes["fagyálló"]);
            connectionManager.Connect(graphContext, nodes["víz"], nodes["jég"]);
            connectionManager.Connect(graphContext, nodes["víz"], nodes["hidrogén"]);
            connectionManager.Connect(graphContext, nodes["víz"], nodes["gőz"]);
            connectionManager.Connect(graphContext, nodes["víz"], nodes["oxigén"]);
            connectionManager.Connect(graphContext, nodes["víz"], nodes["folyó"]);


            return "Done";
        }

        private ContentItem NewNotion()
        {
            return _contentManager.New("Notion");
        }
    }
}