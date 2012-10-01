using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Associativy.Services;
using Orchard;
using Piedone.HelpfulLibraries.Utilities;

namespace Associativy.Neo4j.Services
{
    public interface INeo4jConnectionManager : IConnectionManager, INeo4jService
    {
    }
}
