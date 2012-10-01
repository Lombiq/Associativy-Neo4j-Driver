using System;
using Orchard;
using Piedone.HelpfulLibraries.Utilities;

namespace Associativy.Neo4j.Services
{
    public interface INeo4jService : IFreezable, ITransientDependency
    {
        Uri RootUri { get; set; }
    }
}
