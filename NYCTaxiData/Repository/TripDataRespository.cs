using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NYCTaxiData.Models;

namespace NYCTaxiData.Repository
{
    public class TripDataRespository : ITripDataRepository
    {
        private readonly IRepository _repository;
      
        public TripDataRespository(IRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<TripData> FindAll()
        {
            string query =
                  @"SELECT CONCAT(z.borough,'/',z.zone,'/',z.servicezone) as location,CONCAT(z2.borough,'/',z2.zone,'/',z2.servicezone) as Dist,t.trips,t.tips
                from( 
                      select locationId,dolocationId,SUM(tripcount) as trips,sum(tippercentage) as tips,taxitype
                       from public.nyctaxitripdata_destinations_bytrips where locationType=@locationType
                       Group by locationId,dolocationId,taxitype) t
                        inner join taxizones z on z.locationId=t.locationId
                  inner join taxizones z2 on z2.locationId=t.dolocationId
                  order by trips  desc";
            return _repository.SqlQuery<TripData>(query);

        }

        public dynamic GetDashBoardData(Search search)
        {
            string query =
                @"SELECT CONCAT(z.borough,'/',z.zone,'/',z.servicezone) as location,t.*,z.Lat,z.Lag           
            from( 
                      select locationId,SUM(tripcount) as trips,ROUND(CAST(AVG(tippercentage) as numeric),2) as tips,ROUND(CAST(AVG(triptotal) as numeric),2) as triptotal,taxitype";
            query += search.month > 0 ? ",month" : " ";
            query += @" from public.nyctaxitripdata_destinations_bytrips where locationType=@locationType and taxitype=@taxitype and tripcount>100";
            query += search.month > 0 ? " and month=" + search.month : " ";
            query += @" Group by locationId,taxitype";
            query += search.month > 0 ? ",month" : "";
            query += @") t
                        inner join taxizones z on z.locationId=t.locationId                
                  order by ";
            query += search.countBy != null ? search.countBy : "trips";
            query += @" desc
                  FETCH FIRST 10 ROW ONLY
                  ";
            var parameters = new Dictionary<string, object>
                                 {
                                     { "@locationType", search.locationType },
                                     { "@taxitype", search.taxiType }
                                 };
            return _repository.SqlQuery<dynamic>(query, parameters).ToList();

        }

        public dynamic GetLocationDetail(Search search)
        {
            string query =
               @"SELECT CONCAT(z.borough,'/',z.zone,'/',z.servicezone) as location,t.*,z.Lat,z.Lag           
            from( 
                      select dolocationId as locationId,SUM(tripcount) as trips,ROUND(CAST(AVG(tippercentage) as numeric),2) as tips,ROUND(CAST(AVG(triptotal) as numeric),2) as triptotal,taxitype";
            query += search.month > 0 ? ",month" : " ";
            query += @" from public.nyctaxitripdata_destinations_bytrips where locationId=@locationId and locationId!=dolocationId and locationType=@locationType and taxitype=@taxitype";
            query += search.month > 0 ? " and month=" + search.month : " ";
            query += @" Group by dolocationId,taxitype";
            query += search.month > 0 ? ",month" : "";
            query += @") t
                        inner join taxizones z on z.locationId=t.locationId                
                  order by ";
            query += search.countBy != null ? search.countBy : "trips";
            query += @" desc
                  FETCH FIRST 3 ROW ONLY
                  ";
            var parameters = new Dictionary<string, object>
                                 {
                                     { "@locationId", search.locationId },
                                     { "@locationType", search.locationType },
                                     { "@taxitype", search.taxiType }
                                 };
            return _repository.SqlQuery<dynamic>(query, parameters).ToList();

        }
        public TripData FindById(int id)
        {
            //using (IDbConnection dbConnection = Connection)
            //{
            //    dbConnection.Open();
            //    return dbConnection.Query<TripData>("SELECT * FROM nyctaxitripdata2 WHERE id = @Id", new { Id = id }).FirstOrDefault();
            //}
            var query = @"SELECT * FROM nyctaxitripdata2 WHERE id = @Id";
            var parameters = new Dictionary<string, object>
                                 {
                                     { "@Id", id }
                                 };
            return _repository.SqlQuery<TripData>(query, parameters).FirstOrDefault();
        }
        public IEnumerable<TripData> FindBySearch(Search search)
        {

            var query = @"SELECT *  FROM nyctaxitripdata_dataset1 where  ";
            string locationIds = "1";
            if (search.locationId > 0)
                locationIds = search.locationId.ToString();
            query += " locationId in( @locationIds)";


            var parameters = new Dictionary<string, object>
            {
                {"@locationIds", locationIds}
            };
            //     using (IDbConnection dbConnection = Connection)
            //     {
            //         dbConnection.Open();
            //         return dbConnection.Query<TripData>(query).ToList();
            //     }
            return _repository.SqlQuery<TripData>(query, parameters);
        }

        public dynamic GetBarChartData(JObject obj)
        {
            var query= @"select SUM(tripcount) as trips,ROUND(CAST(AVG(tippercentage) as numeric), 2) as tips,ROUND(CAST(AVG(triptotal) as numeric),2) as triptotal,taxitype,month           
            from public.nyctaxitripdata_destinations_bytrips  
            where locationType='PU'";
            if (obj != null)
            {
                
                foreach (JProperty property in obj.Properties())
                {
                    if (property.Name == "zones")
                    {
                        string zones = "";
                        foreach (var jToken in property.Value)
                        {
                            zones += zones=="" ? jToken.Value<string>(): "," + jToken.Value<string>();
                        }
                        if(zones!="")
                        query += " and locationId in(" + zones + ")";
                    }
                    if (property.Name == "months")
                    {
                        string months = "";
                        foreach (var jToken in property.Value)
                        {
                            months += months == "" ? jToken.Value<string>() : "," + jToken.Value<string>();
                        }
                        if(months!="")
                        query += " and month in(" + months + ")";
                    }
                }
            }

            query +=" Group by taxitype,month";
            return _repository.SqlQuery<dynamic>(query).ToList();

        }



    }

}