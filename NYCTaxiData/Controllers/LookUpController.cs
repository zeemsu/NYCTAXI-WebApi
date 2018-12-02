using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NYCTaxiData.Models;
using NYCTaxiData.Service;

namespace NYCTaxiData.Controllers
{
    [RoutePrefix("api/LookUp")]
    public class LookUpController : ApiController
    {
        private readonly ILookUpService _lookUpService;
        public LookUpController(ILookUpService lookUpService)
        {
            _lookUpService = lookUpService;
        }
        [HttpGet]
        [Route("GetZones")]
        public IEnumerable<TaxiZone> GetZones()
        {
            return _lookUpService.GetZones();
        }
        [HttpGet]
        [Route("GetLocation")]
        public TaxiZone GetLocation(int locationId)
        {
            return _lookUpService.GetLocation(locationId);
        }

        [HttpPost]
        public void UpdateLocation(int locationId, float lat, float lng)
        {
            _lookUpService.UpdateLocation(locationId, lat, lng);
        }


    }
}
