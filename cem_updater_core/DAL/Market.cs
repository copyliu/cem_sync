using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using cem_updater_core.Model;
using Dasync.Collections;
using Npgsql;
using NpgsqlTypes;

namespace cem_updater_core.DAL
{
    public class Market
    {
        public static async Task<List<CurrentMarket>> GetCurrentMarkets(long regionid, bool tq = false)
        {
            var result = new List<CurrentMarket>();
            await using var conn = new NpgsqlConnection(Helpers.GetMarketConnString(tq));
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select * from current_market where regionid=@region;";
            cmd.Parameters.AddWithValue("region", regionid);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new CurrentMarket()
                {
                    id = (long) reader["id"],
                    regionid = (long) reader["regionid"],
                    systemid = Convert.IsDBNull(reader["systemid"]) ? null : (long?) reader["systemid"],
                    stationid = (long) reader["stationid"],
                    typeid = (int) reader["typeid"],
                    bid = (int) reader["bid"],
                    price = (double) reader["price"],
                    orderid = (long) reader["orderid"],
                    minvolume = (int) reader["minvolume"],
                    volremain = (int) reader["volremain"],
                    volenter = (int) reader["volenter"],
                    issued = (DateTime) reader["issued"],
                    range = (int) reader["range"],
                    reportedby = (long) reader["reportedby"],
                    reportedtime = (DateTime) reader["reportedtime"],
                    source = (int) reader["source"],
                    interval = (int) reader["interval"],

                });

            }

            return result;

        }

        public static async Task<Dictionary<long, int>[]> GetStations(bool tq = false)
        {
            var system = new Dictionary<long, int>();
            var region = new Dictionary<long, int>();
            await using var conn = new NpgsqlConnection(Helpers.GetMarketConnString(tq));
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText =
                "select stationid,a.systemid,b.regionid from stations a inner join systems b on a.systemid=b.systemid ;";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                system.Add(reader.GetInt64(0), (int) reader.GetInt64(1));
                region.Add(reader.GetInt64(0), (int) reader.GetInt64(2));
            }

            return new[] {system, region};

        }

        public static async Task<List<int>> GetRegions(bool tq = false)
        {
            List<int> regions = new List<int>();
            await using var conn = new NpgsqlConnection(Helpers.GetMarketConnString(tq));

            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select regionid FROM regions order by regionid;";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                regions.Add(reader.GetInt32(0));
            }


            return regions;
        }

        public static async Task UpdateDatabaseAsync(List<ESIMarketOrder> newlist, List<ESIMarketOrder> updatelist,
            List<long> deletelist, Dictionary<long, HashSet<int>> updatedtypelist, int region, bool tq = false)
        {

            var cnewlist = newlist.AsParallel().Select(p => new CrestOrder()
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
                range = p.range,
                systemid = p.system_id,
                regionid = p.regionid
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
                range = p.range,
                systemid = p.system_id,
                regionid = p.regionid
            }).ToList();
            await UpdateDatabase(cnewlist, cupdatelist, deletelist, updatedtypelist, region, tq);

        }

        public static async Task UpdateDatabase(List<CrestOrder> newlist, List<CrestOrder> updatelist,
            List<long> deletelist, Dictionary<long, HashSet<int>> updatedtypelist, int region, bool tq = false)
        {

            await using var conn = new NpgsqlConnection(Helpers.GetMarketConnString(tq));
            await conn.OpenAsync();
            var trans = await conn.BeginTransactionAsync();
            {

                var cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandText =
                    @"insert into current_market  (regionid,systemid,stationid,typeid,bid,price,orderid,minvolume,volremain,volenter,issued,interval,range,reportedby,reportedtime,source) 
                                    VALUES (@regionid,@systemid,@stationid,@typeid,@bid,@price,@orderid,@minvolume,@volremain,@volenter,@issued,@interval,@range,@reportedby,@reportedtime,@source)";


                var regionid_p = cmd.Parameters.Add("regionid", NpgsqlDbType.Bigint);
                var systemid_p = cmd.Parameters.Add("systemid", NpgsqlDbType.Bigint);
                var stationid_p = cmd.Parameters.Add("stationid", NpgsqlDbType.Bigint);
                var typeid_p = cmd.Parameters.Add("typeid", NpgsqlDbType.Bigint);
                var bid_p = cmd.Parameters.Add("bid", NpgsqlDbType.Bigint);
                var price_p = cmd.Parameters.Add("price", NpgsqlDbType.Double);
                var orderid_p = cmd.Parameters.Add("orderid", NpgsqlDbType.Bigint);
                var minvolume_p = cmd.Parameters.Add("minvolume", NpgsqlDbType.Integer);
                var volremain_p = cmd.Parameters.Add("volremain", NpgsqlDbType.Integer);
                var volenter_p = cmd.Parameters.Add("volenter", NpgsqlDbType.Integer);
                var interval_p = cmd.Parameters.Add("interval", NpgsqlDbType.Bigint);
                var range_p = cmd.Parameters.Add("range", NpgsqlDbType.Integer);
                var reportedby_p = cmd.Parameters.Add("reportedby", NpgsqlDbType.Integer);
                var source_p = cmd.Parameters.Add("source", NpgsqlDbType.Integer);
                var issued_p = cmd.Parameters.Add("issued", NpgsqlDbType.TimestampTz);
                var reportedtime_p = cmd.Parameters.Add("reportedtime", NpgsqlDbType.TimestampTz);
                await cmd.PrepareAsync();
                var now = DateTime.Now;
                foreach (var model in newlist)
                {
                    regionid_p.Value = model.regionid;
                    systemid_p.Value = (object) model.systemid ?? DBNull.Value;
                    stationid_p.Value = model.stationID;
                    typeid_p.Value = model.type;
                    bid_p.Value = model.buy ? 1 : 0;
                    price_p.Value = model.price;
                    orderid_p.Value = model.id;
                    minvolume_p.Value = model.minVolume;
                    volremain_p.Value = model.volume;
                    volenter_p.Value = model.volumeEntered;
                    interval_p.Value = model.duration;
                    range_p.Value = Helpers.ConvertRange(model.range);
                    reportedby_p.Value = 0;
                    source_p.Value = 0;
                    reportedtime_p.Value = now;
                    issued_p.Value = model.issued;

                    await cmd.ExecuteNonQueryAsync();

                }



            }

            {


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

                var regionid_p = cmd.Parameters.Add("regionid", NpgsqlDbType.Bigint);
                var systemid_p = cmd.Parameters.Add("systemid", NpgsqlDbType.Bigint);
                var stationid_p = cmd.Parameters.Add("stationid", NpgsqlDbType.Bigint);
                var typeid_p = cmd.Parameters.Add("typeid", NpgsqlDbType.Bigint);
                var bid_p = cmd.Parameters.Add("bid", NpgsqlDbType.Bigint);
                var price_p = cmd.Parameters.Add("price", NpgsqlDbType.Double);
                var orderid_p = cmd.Parameters.Add("orderid", NpgsqlDbType.Bigint);
                var minvolume_p = cmd.Parameters.Add("minvolume", NpgsqlDbType.Integer);
                var volremain_p = cmd.Parameters.Add("volremain", NpgsqlDbType.Integer);
                var volenter_p = cmd.Parameters.Add("volenter", NpgsqlDbType.Integer);
                var interval_p = cmd.Parameters.Add("interval", NpgsqlDbType.Bigint);
                var range_p = cmd.Parameters.Add("range", NpgsqlDbType.Integer);
                var reportedby_p = cmd.Parameters.Add("reportedby", NpgsqlDbType.Integer);
                var source_p = cmd.Parameters.Add("source", NpgsqlDbType.Integer);
                var issued_p = cmd.Parameters.Add("issued", NpgsqlDbType.TimestampTz);
                var reportedtime_p = cmd.Parameters.Add("reportedtime", NpgsqlDbType.TimestampTz);
                await cmd.PrepareAsync();
                var now = DateTime.Now;
                foreach (var model in newlist)
                {
                    regionid_p.Value = model.regionid;
                    systemid_p.Value = (object) model.systemid ?? DBNull.Value;
                    stationid_p.Value = model.stationID;
                    typeid_p.Value = model.type;
                    bid_p.Value = model.buy ? 1 : 0;
                    price_p.Value = model.price;
                    orderid_p.Value = model.id;
                    minvolume_p.Value = model.minVolume;
                    volremain_p.Value = model.volume;
                    volenter_p.Value = model.volumeEntered;
                    interval_p.Value = model.duration;
                    range_p.Value = Helpers.ConvertRange(model.range);
                    reportedby_p.Value = 0;
                    source_p.Value = 0;
                    reportedtime_p.Value = now;
                    issued_p.Value = model.issued;

                    await cmd.ExecuteNonQueryAsync();

                }



            }


            {

                var cmd = new NpgsqlCommand();
                cmd.Connection = conn;
                cmd.CommandText =
                    @"delete from current_market  where orderid= any (@orderid) and regionid = @regionid;";
                cmd.Parameters.AddWithValue("orderid", deletelist);
                cmd.Parameters.Add(new NpgsqlParameter<long>("regionid", region));
                await cmd.ExecuteNonQueryAsync();
            }

            foreach (var u in updatedtypelist)
            {
                {
                    var cmd = new NpgsqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText =
                        @"select typeid,max(price),sum(volremain) from current_market where typeid=any (@typeid) and regionid=@regionid and bid=1 group by typeid;
                                            select typeid,min(price),sum(volremain) from current_market where typeid=any (@typeid) and regionid=@regionid and bid=0 group by typeid;";
                    cmd.Parameters.AddWithValue("typeid", u.Value.ToList());
                    cmd.Parameters.AddWithValue("regionid", u.Key);
                    var buyprice = u.Value.ToDictionary(p => p, p => (double) 0);
                    var buyvol = u.Value.ToDictionary(p => p, p => (long) 0);
                    var sellprice = u.Value.ToDictionary(p => p, p => (double) 0);
                    var sellvol = u.Value.ToDictionary(p => p, p => (long) 0);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {

                        while (await reader.ReadAsync())
                        {
                            buyprice[reader.GetInt32(0)] = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);

                            buyvol[reader.GetInt32(0)] = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                        }

                        await reader.NextResultAsync();
                        while (await reader.ReadAsync())
                        {
                            sellprice[reader.GetInt32(0)] = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);

                            buyvol[reader.GetInt32(0)] = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                        }
                    }

                    {
                        cmd = new NpgsqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText =
                            "insert into market_realtimehistory (regionid,typeid,date,sell,buy,sellvol,buyvol) values" +
                            " (@regionid,@typeid,@date,@sell,@buy,@sellvol,@buyvol);";
                        var typeid_p = cmd.Parameters.Add("typeid", NpgsqlDbType.Integer);
                        var regionid_p = cmd.Parameters.Add("regionid", NpgsqlDbType.Bigint);
                        var date_p = cmd.Parameters.Add("date", NpgsqlDbType.TimestampTz);
                        var sell_p = cmd.Parameters.Add("sell", NpgsqlDbType.Double);
                        var buy_p = cmd.Parameters.Add("buy", NpgsqlDbType.Double);
                        var sellvol_p = cmd.Parameters.Add("sellvol", NpgsqlDbType.Bigint);
                        var buyvol_p = cmd.Parameters.Add("buyvol", NpgsqlDbType.Bigint);
                        await cmd.PrepareAsync();
                        foreach (var typeid in u.Value)
                        {
                            typeid_p.Value = typeid;
                            regionid_p.Value = u.Key;
                            date_p.Value = DateTime.Now;
                            sell_p.Value = sellprice[typeid];
                            buy_p.Value = buyprice[typeid];
                            sellvol_p.Value = sellvol[typeid];
                            buyvol_p.Value = buyvol[typeid];
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    cmd = new NpgsqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText =
                        "select count(*),typeid from market_markethistorybyday where date=@date and regionid=@regionid and typeid=any (@typeid) group by typeid ;";
                    cmd.Parameters.AddWithValue("date", DateTime.Today);
                    cmd.Parameters.AddWithValue("typeid", u.Value.ToList());
                    cmd.Parameters.AddWithValue("regionid", u.Key);
                    var hasrec = u.Value.ToDictionary(p => p, p => false);
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (reader.GetInt32(0) > 0)
                            {
                                hasrec[reader.GetInt32(1)] = true;

                            }
                        }
                    }

                    {
                        var cmdupdate = new NpgsqlCommand();
                        cmdupdate.Connection = conn;
                        cmdupdate.CommandText =
                            "update market_markethistorybyday set min=LEAST(min,@cur),max=GREATEST(max,@cur),\"end\"=@cur where date=@date and regionid=@regionid and typeid=@typeid;";
                        var cmdinsert = new NpgsqlCommand();
                        cmdinsert.Connection = conn;
                        cmdinsert.CommandText =
                            "INSERT INTO market_markethistorybyday( date, min, max, start, \"end\", volume, regionid, typeid, \"order\") " +
                            "VALUES ( @date, @cur, @cur, @cur, @cur, 0, @regionid, @typeid, 0);";

                        var cur_i = cmdinsert.Parameters.Add("cur", NpgsqlDbType.Double);
                        var cur_p = cmdupdate.Parameters.Add("cur", NpgsqlDbType.Double);
                        var date_i = cmdinsert.Parameters.Add("date", NpgsqlDbType.TimestampTz);
                        var date_p = cmdupdate.Parameters.Add("date", NpgsqlDbType.TimestampTz);
                        var typeid_i = cmdinsert.Parameters.Add("typeid", NpgsqlDbType.Integer);
                        var typeid_p = cmdupdate.Parameters.Add("typeid", NpgsqlDbType.Integer);
                        var regionid_i = cmdinsert.Parameters.Add("regionid", NpgsqlDbType.Bigint);
                        var regionid_p = cmdupdate.Parameters.Add("regionid", NpgsqlDbType.Bigint);

                        await cmdinsert.PrepareAsync();
                        await cmdupdate.PrepareAsync();

                        foreach (var typeid in u.Value)
                        {
                            if (hasrec[typeid])
                            {
                                cur_p.Value = sellprice[typeid];
                                date_p.Value = DateTime.Today;
                                typeid_p.Value = typeid;
                                regionid_p.Value = u.Key;
                                await cmdupdate.ExecuteNonQueryAsync();
                            }
                            else
                            {
                                cur_i.Value = sellprice[typeid];
                                date_i.Value = DateTime.Today;
                                typeid_i.Value = typeid;
                                regionid_i.Value = u.Key;
                                await cmdinsert.ExecuteNonQueryAsync();
                            }
                        }

                    }

                }


            }

            await trans.CommitAsync();

        }
    }
}