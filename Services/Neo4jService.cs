using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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