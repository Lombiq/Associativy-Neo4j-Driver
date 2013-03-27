using System.Threading.Tasks;
using System.Web.Mvc;
using Associativy.GraphDiscovery;
using Associativy.Neo4j.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Associativy.Neo4j.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly IAuthorizer _authorizer;
        private readonly IGraphManager _graphManager;
        private readonly INotifier _notifier;

        public Localizer T { get; set; }


        public AdminController(IAuthorizer authorizer, IGraphManager graphManager, INotifier notifier)
        {
            _authorizer = authorizer;
            _graphManager = graphManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }


        [HttpPost]
        public void RebuildStatistics(string graphName)
        {
            if (!_authorizer.Authorize(Associativy.Administration.Permissions.ManageAssociativyGraphs, T("You're not allowed to manage Associativy settings.")))
                return;

            var graph = _graphManager.FindGraph(graphName);
            if (graph == null) return;

            var connectionManager = graph.Services.ConnectionManager;
            if (!(connectionManager is INeo4jConnectionManager)) return;

            Task.WaitAll(((INeo4jConnectionManager)connectionManager).RebuildStatisticsAsync());

            // This throws an exception due to the WorkContext being null. This will be solved with Orchard 1.7.
            //Task.WaitAll(((INeo4jConnectionManager)connectionManager).RebuildStatisticsAsync().ContinueWith(task =>
            //    {
            //        _notifier.Information(T("Statistics rebuilt."));
            //    }));
        }
    }
}