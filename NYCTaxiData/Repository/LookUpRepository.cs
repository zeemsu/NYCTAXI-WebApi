using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Routing;
using Dapper;
using NYCTaxiData.Models;

namespace NYCTaxiData.Repository
{
    public class LookUpRepository : ILookUpRepository
    {
        private readonly IRepository _repository;
        public LookUpRepository(IRepository repository)
        {
            _repository = repository;
        }
        
        public IEnumerable<TaxiZone> GetZones()
        {
            return _repository.SqlQuery<TaxiZone>("select * from TaxiZones");

        }

        
        public TaxiZone GetLocation(int locationId)
        {
            return _repository.SqlQuery<TaxiZone>("select * from TaxiZones where locationId=" + locationId).FirstOrDefault();

        }
        
        public void UpdateLocation(int locationId, float lat, float lng)
        {
            var query = "Update taxizones set Lat=@lat,Lag=(@lng) where locationId=@locationId";
            var parameters = new Dictionary<string, object>
                                 {
                                     { "@locationId",locationId },
                                     { "@lat", lat },
                                     { "@lng", lng }
                                 };
            _repository.Execute(query,parameters);
        }

    }
}