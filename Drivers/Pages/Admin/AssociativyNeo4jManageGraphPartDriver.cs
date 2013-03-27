using Associativy.Neo4j.Models.Pages.Admin;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;

namespace Associativy.Neo4j.Drivers.Pages.Admin
{
    [OrchardFeature("Associativy.Neo4j.Administration")]
    public class AssociativyNeo4jManageGraphPartDriver : ContentPartDriver<AssociativyNeo4jManageGraphPart>
    {
        protected override DriverResult Display(AssociativyNeo4jManageGraphPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Pages_AssociatvyNeo4jManageGraph",
                () => shapeHelper.DisplayTemplate(
                                TemplateName: "Pages/Admin/Neo4jManageGraph",
                                Model: part,
                                Prefix: Prefix));
        }
    }
}