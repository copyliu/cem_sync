using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace cem_updater_core
{
    public class Helpers
    {
        public static int ConvertRange(string range)
        {
            switch (range)
            {
                case "station": return 0;
                case "solarsystem": return 32767;
                case "region": return 65535;
                default: return 65535;
            }
        }
    }
    public class DAL
    {
        public  static string GetConnString(bool tq = false)
        {
            return tq ? connectionstring_tq : connectionstring_cn;
        }
        public static List<CurrentMarket> GetCurrentMarkets(long regionid, bool tq = false)
        {
            var result = new List<CurrentMarket>();
            using (var conn = new NpgsqlConnection(GetConnString(tq)))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select * from current_market where regionid=@region;";
                    cmd.Parameters.AddWithValue("region", regionid);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new CurrentMarket()
                            {
                                id = (long)reader["id"],
                                regionid = (long)reader["regionid"],
                                systemid = (long)reader["systemid"],
                                stationid = (long)reader["stationid"],
                                typeid = (int)reader["typeid"],
                                bid = (int)reader["bid"],
                                price = (double)reader["price"],
                                orderid = (long)reader["orderid"],
                                minvolume = (int)reader["minvolume"],
                                volremain = (int)reader["volremain"],
                                volenter = (int)reader["volenter"],
                                issued = (DateTime)reader["issued"],
                                range = (int)reader["range"],
                                reportedby = (long)reader["reportedby"],
                                reportedtime = (DateTime)reader["reportedtime"],
                                source = (int)reader["source"],
                                interval = (int)reader["interval"],

                            });

                        }
                    }

                }
            }

            return result;

        }
        public static Dictionary<long, long>[] GetStations(bool tq=false)
        {
            var system=new Dictionary<long, long>();
            var region = new Dictionary<long, long>();
            using (var conn = new NpgsqlConnection(GetConnString(tq)))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select stationid,a.systemid,b.regionid from stations a inner join systems b on a.systemid=b.systemid ;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            system.Add(reader.GetInt64(0),reader.GetInt64(1));
                            region.Add(reader.GetInt64(0),reader.GetInt64(2));
                        }
                    }
                }
            }

            return new []{system,region};

        }

        public static string connectionstring_cn;
        public static string connectionstring_tq;

        public static List<int> GetRegions(bool tq=false)
        {
            List<int> regions = new List<int>();
            using (var conn = new NpgsqlConnection(GetConnString(tq)))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select regionid FROM regions;";
                    using (var reader = cmd.ExecuteReader())

                    {
                        while (reader.Read())
                        {
                            regions.Add(reader.GetInt32(0));
                        }
                    }
                }
            }

            return regions;
        }

        public static void UpdateDatabase(List<ESIMarketOrder> newlist, List<ESIMarketOrder> updatelist,
            List<long> deletelist, Dictionary<long, HashSet<int>> updatedtypelist, bool tq = false)
        {
          
            var cnewlist=newlist.AsParallel().Select(p => new CrestOrder()
            {
                stationID = p.location_id,
                id = p.order_id,
                type = p.type_id,
                duration = p.duration,
                minVolume = p.min_volume,
                volume = p.volume_remain,
                issued = p.issued,
                volumeEntered = p.volume_total,
                price = p.price,
                buy = p.is_buy_order,
                range = p.range
            }).ToList();
            var cupdatelist = updatelist.AsParallel().Select(p => new CrestOrder()
            {
                stationID = p.location_id,
                id = p.order_id,
                type = p.type_id,
                duration = p.duration,
                minVolume = p.min_volume,
                volume = p.volume_remain,
                issued = p.issued,
                volumeEntered = p.volume_total,
                price = p.price,
                buy = p.is_buy_order,
                range = p.range
            }).ToList();
            UpdateDatabase(cnewlist,cupdatelist,deletelist,updatedtypelist,tq);

        }

        public static void UpdateDatabase(List<CrestOrder> newlist, List<CrestOrder> updatelist, List<long> deletelist,Dictionary<long, HashSet<int>> updatedtypelist,bool tq=false)
        {
            Parallel.ForEach(newlist, new ParallelOptions() {MaxDegreeOfParallelism = 10}, (model) =>
            {
                using (var conn = new NpgsqlConnection(GetConnString(tq)))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText =
                        @"insert into current_market  (regionid,systemid,stationid,typeid,bid,price,orderid,minvolume,volremain,volenter,issued,interval,range,reportedby,reportedtime,source) 
                                    VALUES (@regionid,@systemid,@stationid,@typeid,@bid,@price,@orderid,@minvolume,@volremain,@volenter,@issued,@interval,@range,@reportedby,@reportedtime,@source)";
                    cmd.Parameters.Add(new NpgsqlParameter<long>("regionid", Caches.StationRegionDictCn[model.stationID]));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("systemid", Caches.StationSystemDictCn[model.stationID]));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("stationid", model.stationID));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("typeid", model.type));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("bid", model.buy ? 1 : 0));
                    cmd.Parameters.Add(new NpgsqlParameter<double>("price", model.price));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("orderid", model.id));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("minvolume", model.minVolume));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("volremain", model.volume));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("volenter", model.volumeEntered));
                    cmd.Parameters.Add(new NpgsqlParameter<DateTime>("issued", model.issued));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("interval", model.duration));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("range", Helpers.ConvertRange(model.range)));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("reportedby", 0));
                    cmd.Parameters.Add(new NpgsqlParameter<DateTime>("reportedtime", DateTime.Now));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("source", 0));
                    cmd.ExecuteNonQuery();
                }
            });
            Parallel.ForEach(updatelist, new ParallelOptions() {MaxDegreeOfParallelism = 10}, model =>
            {
                using (var conn = new NpgsqlConnection(GetConnString(tq)))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText =
                        @"update current_market set
                                (regionid,systemid,stationid,typeid,bid,price,orderid,minvolume,volremain,volenter,issued,interval,range,reportedby,reportedtime)
                                 =
                                (
                                @regionid,
                                @systemid,
                                @stationid,
                                @typeid,
                                @bid,
                                @price,
                                @orderid,
                                @minvolume,
                                @volremain,
                                @volenter,
                                @issued,
                                @interval,
                                @range,
                                @reportedby,
                                @reportedtime
                                ) where orderid=@orderid
                                ";
                    cmd.Parameters.Add(new NpgsqlParameter<long>("regionid", Caches.StationRegionDictCn[model.stationID]));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("systemid", Caches.StationSystemDictCn[model.stationID]));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("stationid", model.stationID));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("typeid", model.type));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("bid", model.buy ? 1 : 0));
                    cmd.Parameters.Add(new NpgsqlParameter<double>("price", model.price));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("orderid", model.id));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("minvolume", model.minVolume));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("volremain", model.volume));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("volenter", model.volumeEntered));
                    cmd.Parameters.Add(new NpgsqlParameter<DateTime>("issued", model.issued));
                    cmd.Parameters.Add(new NpgsqlParameter<long>("interval", model.duration));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("range", Helpers.ConvertRange(model.range)));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("reportedby", 0));
                    cmd.Parameters.Add(new NpgsqlParameter<DateTime>("reportedtime", DateTime.Now));
                    cmd.Parameters.Add(new NpgsqlParameter<int>("source", 0));
                    cmd.ExecuteNonQuery();
                }

            });
            using (var conn = new NpgsqlConnection(GetConnString(tq)))
            {
                conn.Open();
                var cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandText =@"delete from current_market  where orderid= any (@orderid);";
                cmd.Parameters.AddWithValue("orderid",deletelist);
                cmd.ExecuteNonQuery();
            }

            foreach (var u in updatedtypelist)
            {
                Parallel.ForEach(u.Value, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, typeid =>
                {

                    using (var conn = new NpgsqlConnection(GetConnString(tq)))
                    {
                        conn.Open();
                        var cmd = new NpgsqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = @"select max(price),sum(volremain) from current_market where typeid=@typeid and regionid=@regionid and bid=1;
                                            select min(price),sum(volremain) from current_market where typeid=@typeid and regionid=@regionid and bid=0;";
                        cmd.Parameters.AddWithValue("typeid", typeid);
                        cmd.Parameters.AddWithValue("regionid", u.Key);
                        double sellprice = 0;
                        double buyprice = 0;
                        long sellvol = 0;
                        long buyvol = 0;
                        using (var reader = cmd.ExecuteReader())
                        {
                           
                            while (reader.Read())
                            {
                                buyprice  = reader.IsDBNull(0)? 0:reader.GetDouble(0);
                                buyvol = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                            }

                            reader.NextResult();
                            while (reader.Read())
                            {
                                sellprice = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                                sellvol = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                            }
                        }
                        cmd=new NpgsqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText =
                            "insert into market_realtimehistory (regionid,typeid,date,sell,buy,sellvol,buyvol) values" +
                            " (@regionid,@typeid,@date,@sell,@buy,@sellvol,@buyvol);";
                        cmd.Parameters.AddWithValue("typeid", typeid);
                        cmd.Parameters.AddWithValue("regionid", u.Key);
                        cmd.Parameters.AddWithValue("date", DateTime.Now);
                        cmd.Parameters.AddWithValue("sell", sellprice);
                        cmd.Parameters.AddWithValue("buy", buyprice);
                        cmd.Parameters.AddWithValue("sellvol", sellvol);
                        cmd.Parameters.AddWithValue("buyvol", buyvol);
                        cmd.ExecuteNonQuery();

                        cmd=new NpgsqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText =
                            "select count(*) from market_markethistorybyday where date=@date and regionid=@regionid and typeid=@typeid;";
                        cmd.Parameters.AddWithValue("date", DateTime.Today);
                        cmd.Parameters.AddWithValue("typeid", typeid);
                        cmd.Parameters.AddWithValue("regionid", u.Key);
                        bool hasoldrecord = false;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.GetInt32(0) > 0)
                                {
                                    hasoldrecord = true;
                                    break;
                                }
                            }
                        }
                        cmd=new NpgsqlCommand();
                        cmd.Connection = conn;
                        if (hasoldrecord)
                        {
                            cmd.CommandText =
                                "update market_markethistorybyday set min=LEAST(min,@cur),max=GREATEST(max,@cur),\"end\"=@cur where date=@date and regionid=@regionid and typeid=@typeid;";
                           
                        }
                        else
                        {
                            cmd.CommandText =
                                "INSERT INTO market_markethistorybyday( date, min, max, start, \"end\", volume, regionid, typeid, \"order\") " +
                                "VALUES ( @date, @cur, @cur, @cur, @cur, 0, @regionid, @typeid, 0);";
                           
                        }
                        cmd.Parameters.AddWithValue("cur", sellprice);
                        cmd.Parameters.AddWithValue("date", DateTime.Today);
                        cmd.Parameters.AddWithValue("typeid", typeid);
                        cmd.Parameters.AddWithValue("regionid", u.Key);
                        cmd.ExecuteNonQuery();



                    }

                });
            }

            
        }
    }
}
