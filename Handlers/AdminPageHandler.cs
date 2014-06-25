using Associativy.Administration;
using Associativy.Administration.Models.Pages.Admin;
using Associativy.Neo4j.Models.Pages.Admin;
using Associativy.Neo4j.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Piedone.HelpfulLibraries.Contents.DynamicPages;

namespace Associativy.Neo4j.Handlers
{
    [OrchardFeature("Associativy.Neo4j.Administration")]
    public class AdminPageHandler : ContentHandler
    {
        protected override void Initializing(InitializingContentContext context)
        {
            var pageContext = context.PageContext();

            if (pageContext.Group != AdministrationPageConfigs.Group) return;

            if (!(pageContext.Page.As<AssociativyManageGraphPart>().GraphDescriptor.Services.ConnectionManager is INeo4jConnectionManager)) return;

            if (pageContext.Page.IsPage("ManageGraph", pageContext.Group))
            {
                pageContext.Page.Weld(new AssociativyNeo4jManageGraphPart());
            }
        }
    }
}