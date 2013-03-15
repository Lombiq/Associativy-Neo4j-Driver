using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.GraphDiscovery;
using Associativy.Services;
using Neo4jClient;
using Orchard.Logging;
using Orchard.Exceptions;

namespace Associativy.Neo4j.Services
{
    public abstract class Neo4jServiceBase : GraphAwareServiceBase
    {
        protected readonly Uri _rootUri;
        protected readonly INeo4jGraphClientPool _graphClientPool;
        protected IGraphClient _graphClient;

        public ILogger Logger { get; set; }


        protected Neo4jServiceBase(
            IGraphDescriptor graphDescriptor,
            Uri rootUri,
            INeo4jGraphClientPool graphClientPool)
            : base(graphDescriptor)
        {
            _rootUri = rootUri;
            _graphClientPool = graphClientPool;

            Logger = NullLogger.Instance;
        }


        protected virtual void TryInit()
        {
            if (_graphClient != null) return;

            if (_rootUri == null) throw new InvalidOperationException("The root URI should be set before this Neo4j service intance can be used.");

            try
            {
                _graphClient = _graphClientPool.GetClient(_rootUri);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal()) throw;

                var message = "Acquiring a graph client for the graph " + _graphDescriptor.Name + " with the url " + _rootUri + " failed.";
                Logger.Error(ex, message);
                throw new ApplicationException(message, ex);
            }
        }
    }
}