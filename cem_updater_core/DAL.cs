using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace cem_updater_core
{
    public class Helpers
    {
        public static int ConvertRange( string range)
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
        private  static string GetConnString(bool tq = false)
        {
            return tq ? connectionstring_tq : connectionstring_cn;
        }
        public static List<CurrentMarket> GetCurrentMarkets(long regionid, bool tq = false)
        {
            var result = new List<CurrentMarket>();
            using (var conn = new NpgsqlConnection(GetConnString(tq)))
            {
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
        public static Dictionary<long, long> GetStations(bool tq=false)
        {
            var result=new Dictionary<long, long>();
            using (var conn = new NpgsqlConnection(GetConnString(tq)))
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select stationid,systemid from stations ;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        { result.Add(reader.GetInt64(0),reader.GetInt64(1));}
                    }
                }
            }

            return result;

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

        public static void UpdateDatabase(List<CrestOrder> newlist, List<CrestOrder> updatelist, List<long> deletelist,bool tq=false)
        {
//                        new CurrentMarket()
//                        {
//                            id = 0,
//                            regionid = region,
//                            systemid = stations[crest.stationID],
//                            stationid = crest.stationID,
//                            typeid = crest.type,
//                            bid = crest.buy ? 1 : 0,
//                            price = crest.price,
//                            orderid = crest.id,
//                            minvolume = crest.minVolume,
//                            volremain = crest.volume,
//                            volenter = crest.volumeEntered,
//                            issued = crest.issued,
//                            range = Helpers.ConvertRange(crest.range),
//                            reportedby = 1,
//                            reportedtime = DateTime.Now,
//                            source = 0,
//                            interval = crest.duration,
//                        }
            using (var conn = new NpgsqlConnection(GetConnString(tq)))
            {
                
            }

        }
    }
}
