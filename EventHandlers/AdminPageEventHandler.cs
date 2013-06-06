using Associativy.Administration;
using Associativy.Administration.Models.Pages.Admin;
using Associativy.Neo4j.Models.Pages.Admin;
using Associativy.Neo4j.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Piedone.HelpfulLibraries.Contents.DynamicPages;

namespace Associativy.Neo4j.EventHandlers
{
    [OrchardFeature("Associativy.Neo4j.Administration")]
    public class AdminPageEventHandler : IPageEventHandler
    {
        public void OnPageInitializing(PageContext pageContext)
        {
            if (pageContext.Group != AdministrationPageConfigs.Group) return;

            var page = pageContext.Page;

            if (!(page.ContentItem.As<AssociativyManageGraphPart>().GraphDescriptor.Services.ConnectionManager is INeo4jConnectionManager)) return;
            
            if (page.IsPage("ManageGraph", pageContext.Group))
            {
                page.ContentItem.Weld(new AssociativyNeo4jManageGraphPart());
            }
        }

        public void OnPageInitialized(PageContext pageContext)
        {
        }

        public void OnPageBuilt(PageContext pageContext)
        {
        }

        public void OnAuthorization(PageAutorizationContext authorizationContext)
        {
        }
    }
}