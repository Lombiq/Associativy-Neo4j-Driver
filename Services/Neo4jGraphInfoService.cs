using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Associativy.Neo4j.Models;
using Orchard.Caching.Services;
using Orchard.Data;

namespace Associativy.Neo4j.Services
{
    public class Neo4jGraphInfoService : INeo4jGraphInfoService
    {
        private readonly ICacheService _cacheService;
        private readonly IRepository<GraphInfoRecord> _repository;

        private const string CacheKey = "Associativy.Neo4j.Neo4jGraphInfoService.InfoRecordId.";


        public Neo4jGraphInfoService(
            ICacheService cacheService, 
            IRepository<GraphInfoRecord> repository)
        {
            _cacheService = cacheService;
            _repository = repository;
        }


        public IGraphInfo GetGraphInfo(string graphName)
        {
            var id = _cacheService.Get(CacheKey + graphName, () =>
                {
                    var record = _repository.Table.Where(r => r.GraphName == graphName).SingleOrDefault();

                    if (record == null)
                    {
                        record = new GraphInfoRecord { GraphName = graphName };
                        _repository.Create(record);
                    }

                    return record.Id;
                });

            return _repository.Get(id);
        }
    }
}