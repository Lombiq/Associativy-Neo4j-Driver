﻿using System.Threading.Tasks;
using Associativy.Services;
using Orchard;

namespace Associativy.Neo4j.Services
{
    public interface INeo4jConnectionManager : IConnectionManager, INeo4jService, ITransientDependency
    {
        Task RebuildStatisticsAsync();
    }
}
