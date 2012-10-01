using System;
using Piedone.HelpfulLibraries.Utilities;

namespace Associativy.Neo4j.Services
{
    public class Neo4jService : FreezableBase, INeo4jService
    {
        protected Uri _rootUri;
        public Uri RootUri
        {
            get { return _rootUri; }
            set
            {
                ThrowIfFrozen();
                _rootUri = value;
                Freeze();
            }
        }

    }
}