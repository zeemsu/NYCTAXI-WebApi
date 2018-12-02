using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NYCTaxiData.Models;

namespace NYCTaxiData.Repository
{
    public interface ILookUpRepository
    {
        IEnumerable<TaxiZone> GetZones();
        TaxiZone GetLocation(int locationId);
        void UpdateLocation(int locationId, float lat, float lng);
    }
}
