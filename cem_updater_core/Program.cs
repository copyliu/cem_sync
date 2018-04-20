using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using cem_updater_core.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.Logging;

namespace cem_updater_core
{
    class Program
    {
        private static readonly object _log_locker = new object();
        private static bool isServiceRunning = false;
        private static System.Timers.Timer _aTimer1 = new System.Timers.Timer(1000) {AutoReset = false};
        private static System.Timers.Timer _aTimer2 = new System.Timers.Timer(1000) {AutoReset = false};

        private static System.Threading.ManualResetEvent _event1 = new ManualResetEvent(true);
        private static System.Threading.ManualResetEvent _event2 = new ManualResetEvent(true);
        private static WaitHandle[] events = {_event1, _event2};

        public static IConfiguration Configuration { get; set; }

        public static void Log(string log)
        {
            lock (_log_locker)
            {

                if (Environment.UserInteractive)
                {
                    Console.WriteLine(DateTime.Now + " : " + log);
                }

            }


        }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            DAL.Helpers.connectionstring_market_cn = Configuration["cndb"];
            DAL.Helpers.connectionstring_market_tq = Configuration["tqdb"];

            DAL.Helpers.connectionstring_kb_cn = Configuration["cnkbdb"];
            DAL.Helpers.connectionstring_kb_tq = Configuration["tqkbdb"];




            bool keepRunning = true;
            isServiceRunning = true;
            _aTimer1.Start();
            _aTimer2.Start();
            _aTimer1.Elapsed += (sender, ar) =>
            {
                _event1.Reset();
                try
                {
                    Log("CN Start");
                    SyncCN();
                    Log("CN Stop");
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                }

                _event1.Set();
                if (isServiceRunning)
                {
                    _aTimer1.Interval = 5 * 60 * 1000;
                    _aTimer1.Start();

                }
            };
            _aTimer2.Elapsed += (sender, ar) =>
            {
                _event2.Reset();
                try
                {
                    Log("TQ Start");
                    SyncTQ();
                    Log("TQ Stop");
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                }

                _event2.Set();
                if (isServiceRunning)
                {
                    _aTimer2.Interval = 5 * 60 * 1000;
                    _aTimer2.Start();

                }
            };

            Console.CancelKeyPress += delegate
            {
                Log("Shutting Down");
                isServiceRunning = false;
                _aTimer1.Stop();
                _aTimer2.Stop();
                WaitHandle.WaitAll(events);
                Log("Exited!");
                keepRunning = false;
            };
            while (keepRunning)
            {
                Thread.Sleep(1000);
            }
        }

        private static void SyncTQ()
        {
            var regions = DAL.Market.GetRegions(true);
            foreach (var region in regions)
            {
                var oldorders = DAL.Market.GetCurrentMarkets(region, true).AsParallel();
                var oldlist = oldorders.GroupBy(p => p.orderid).ToDictionary(g => g.Key, g => g.First());
                var oldorderids = oldorders.Select(p => p.orderid).ToHashSet();
                List<ESIMarketOrder> orders;
                try
                {
                    orders = GetESIOrders(region, true).Distinct().ToList();
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                    continue;
                }



                List<ESIMarketOrder> newlist = new List<ESIMarketOrder>();
                List<ESIMarketOrder> updatelist = new List<ESIMarketOrder>();
                Dictionary<long, HashSet<int>> updatedtypes = new Dictionary<long, HashSet<int>>();
                
                foreach (var crest in orders)
                {
                    if (!Caches.GetStationRegionDict(true).ContainsKey(crest.location_id))
                    {
                        continue; //TODO:此空间站是空堡, 暂不收录
                    }

                    if (Caches.GetStationRegionDict(true)[crest.location_id] != region)
                    {
                        continue;
                    }
                    if (oldorderids.Contains(crest.order_id))
                    {

                        if (crest != oldlist[crest.order_id])
                        {
                            updatelist.Add(crest);
                            if (!updatedtypes.ContainsKey(Caches.GetStationRegionDict(true)[crest.location_id]))
                            {
                                updatedtypes.Add(Caches.GetStationRegionDict(true)[crest.location_id], new HashSet<int>());
                            }

                            updatedtypes[Caches.GetStationRegionDict(true)[crest.location_id]].Add(crest.type_id);
                        }

                        oldorderids.Remove(crest.order_id);

                    }
                    else
                    {
                        newlist.Add(crest);

                        if (!updatedtypes.ContainsKey(Caches.GetStationRegionDict(true)[crest.location_id]))
                        {
                            updatedtypes.Add(Caches.GetStationRegionDict(true)[crest.location_id], new HashSet<int>());
                        }

                        updatedtypes[Caches.GetStationRegionDict(true)[crest.location_id]].Add(crest.type_id);
                    }

                }

                var deletelist = oldorderids.ToList();
                foreach (var oldorder in oldorders.Where(p => oldorderids.Contains(p.orderid)).GroupBy(p => p.regionid)
                    .Select(p =>
                        new {regionid = p.Key, types = p.Select(o => o.typeid).Distinct().ToList()}).Distinct())
                {
                    if (!updatedtypes.ContainsKey(oldorder.regionid))
                    {
                        updatedtypes.Add(oldorder.regionid, new HashSet<int>());
                    }

                    foreach (var typeid in oldorder.types)
                    {
                        updatedtypes[oldorder.regionid].Add(typeid);
                    }
                }
                Log($"new:{newlist.Count},update:{updatelist.Count},del:{deletelist.Count}");
                DAL.Market.UpdateDatabase(newlist, updatelist, deletelist, updatedtypes, true);





            }
        }

        private static List<ESIMarketOrder> GetESIOrders(int regionid, bool tq = false)
        {
            string url;
            if (tq)
            {
                url = $"https://esi.tech.ccp.is/v1/markets/{regionid}/orders/?datasource=tranquility&order_type=all";
            }
            else
            {
                throw new NotImplementedException("沒有國服");
            }

            Log(url);
            int pages;
            List<ESIMarketOrder> results = new List<ESIMarketOrder>();
            var httpResponse = Caches.httpClient.GetAsync(url).Result;
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException(httpResponse.StatusCode.ToString());
            }


            if (!httpResponse.Headers.TryGetValues("x-pages", out var xPages) ||
                !int.TryParse(xPages.FirstOrDefault(), out pages))
            {
                pages = 1;
            }
          

            using (var s = httpResponse.Content.ReadAsStreamAsync().Result)
            {
               

                using (StreamReader sr = new StreamReader(s))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        var crestresult = serializer.Deserialize<List<ESIMarketOrder>>(reader);
                        if (crestresult != null)
                        {
                            results.AddRange(crestresult);
                        }
                    }
                }

                if (pages > 1)
                {
                    Parallel.ForEach(Enumerable.Range(2, pages - 2 + 1),
                        new ParallelOptions() {MaxDegreeOfParallelism = 10},
                        pagenum =>
                        {
                            var result = GetESIOrders(url + $"&page={pagenum}");
                            if (result != null)
                            {
                                lock (results)
                                {
                                    results.AddRange(result);
                                }
                            }
                        });
                }

            }

            return results;
        }

        private static List<ESIMarketOrder> GetESIOrders(string url)
        {
            Log(url);
            
            List<ESIMarketOrder> crestresult;
            using (var s = Caches.httpClient.GetStreamAsync(url).Result)
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        crestresult = serializer.Deserialize<List<ESIMarketOrder>>(reader);
                    }
                }
            }

            return crestresult;
        }

        private static void SyncCN()
        {
            var regions = DAL.Market.GetRegions();



            foreach (var region in regions)
            {
                var oldorders = DAL.Market.GetCurrentMarkets(region).AsParallel();
                var oldlist = oldorders.GroupBy(p => p.orderid).ToDictionary(g => g.Key, g => g.First());
                var oldorderids = oldorders.Select(p => p.orderid).ToHashSet();

                string url = $"https://api-serenity.eve-online.com.cn/market/{region}/orders/all/";

                CrestMarketResult crestresult = null;
                List<CrestOrder> orders = new List<CrestOrder>();
                try
                {
                    crestresult = GetCrestMarketResult(url);


                    if (crestresult.items != null)
                    {
                        orders.AddRange(crestresult.items);
                    }


                    while (crestresult.next != null)
                    {
                        crestresult = GetCrestMarketResult(crestresult.next.href);

                        if (crestresult.items != null)
                        {
                            orders.AddRange(crestresult.items);
                        }

                    }
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                    continue;
                }

               
                List<CrestOrder> newlist = new List<CrestOrder>();
                List<CrestOrder> updatelist = new List<CrestOrder>();
                Dictionary<long, HashSet<int>> updatedtypes = new Dictionary<long, HashSet<int>>();
                orders = orders.Distinct().ToList();
                foreach (var crest in orders)
                {
                    if (!Caches.GetStationRegionDict(false).ContainsKey(crest.stationID))
                    {
                        continue; //TODO:此空间站是空堡, 暂不收录
                    }
                    if (Caches.GetStationRegionDict(false)[crest.stationID] != region)
                    {
                        continue;
                    }
                    crest.issued = DateTime.SpecifyKind(crest.issued, DateTimeKind.Utc);
                    if (Caches.GetStationRegionDict(false)[crest.stationID] != region)
                    {
                        continue;
                    }
                    if (oldorderids.Contains(crest.id))
                    {

                        if (crest != oldlist[crest.id])
                        {
                            updatelist.Add(crest);
                            if (!updatedtypes.ContainsKey(Caches.GetStationRegionDict(false)[crest.stationID]))
                            {
                                updatedtypes.Add(Caches.GetStationRegionDict(false)[crest.stationID], new HashSet<int>());
                            }

                            updatedtypes[Caches.GetStationRegionDict(false)[crest.stationID]].Add(crest.type);
                        }

                        oldorderids.Remove(crest.id);

                    }
                    else
                    {
                        newlist.Add(crest);

                        if (!updatedtypes.ContainsKey(Caches.GetStationRegionDict(false)[crest.stationID]))
                        {
                            updatedtypes.Add(Caches.GetStationRegionDict(false)[crest.stationID], new HashSet<int>());
                        }

                        updatedtypes[Caches.GetStationRegionDict(false)[crest.stationID]].Add(crest.type);
                    }

                }

                var deletelist = oldorderids.ToList();
                foreach (var oldorder in oldorders.Where(p => oldorderids.Contains(p.orderid)).GroupBy(p => p.regionid)
                    .Select(p =>
                        new {regionid = p.Key, types = p.Select(o => o.typeid).Distinct().ToList()}).Distinct())
                {
                    if (!updatedtypes.ContainsKey(oldorder.regionid))
                    {
                        updatedtypes.Add(oldorder.regionid, new HashSet<int>());
                    }

                    foreach (var typeid in oldorder.types)
                    {
                        updatedtypes[oldorder.regionid].Add(typeid);
                    }
                }
                Log($"new:{newlist.Count},update:{updatelist.Count},del:{deletelist.Count}");
                DAL.Market.UpdateDatabase(newlist, updatelist, deletelist, updatedtypes);








            }

        }

        private static CrestMarketResult GetCrestMarketResult(string url)
        {
            Log(url);
            
            CrestMarketResult crestresult;
            
            using (var s = Caches.httpClient.GetStreamAsync(url).Result)
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        crestresult = serializer.Deserialize<CrestMarketResult>(reader);
                    }
                }
            }
            
            return crestresult;
        }


        private static List<Esi_war_kms> EsiGetWarsKM(int war, bool tq = false)
        {
            string url;
            if (tq)
            {
                url = "https://esi.tech.ccp.is"+ $"/v1/wars/{war}/killmails/";
            }
            else
            {
                throw new NotImplementedException("没有国服");
            }



            Log(url);
            int pages;
            List<Esi_war_kms> results = new List<Esi_war_kms>();
            var httpResponse = Caches.httpClient.GetAsync(url).Result;

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException(httpResponse.StatusCode.ToString());
            }


            if (!httpResponse.Headers.TryGetValues("x-pages", out var xPages) ||
                !int.TryParse(xPages.FirstOrDefault(), out pages))
            {
                pages = 1;
            }


            using (var s = httpResponse.Content.ReadAsStreamAsync().Result)
            {


                using (StreamReader sr = new StreamReader(s))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        var kmresult = serializer.Deserialize<List<Esi_war_kms>>(reader);
                        if (kmresult != null)
                        {
                            results.AddRange(kmresult);
                        }
                    }
                }

                if (pages > 1)
                {
                    Parallel.ForEach(Enumerable.Range(2, pages - 2 + 1),
                        new ParallelOptions() { MaxDegreeOfParallelism = 10 },
                        pagenum =>
                        {
                            var result = GetESIKM(url + $"?page={pagenum}");
                            if (result != null)
                            {
                                lock (results)
                                {
                                    results.AddRange(result);
                                }
                            }
                        });
                }

            }

            return results;



        }
        private static List<Esi_war_kms> GetESIKM(string url)
        {
            Log(url);

            List<Esi_war_kms> crestresult;
            using (var s = Caches.httpClient.GetStreamAsync(url).Result)
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        crestresult = serializer.Deserialize<List<Esi_war_kms>>(reader);
                    }
                }
            }

            return crestresult;
        }
        private static void UpdateWars(bool tq = false)
        {
            string url;
            if (tq)
            {
                url = "https://esi.tech.ccp.is";
            }
            else
            {
                throw new NotImplementedException("没有国服");
            }

            List<int> warlist;
            using (var s = Caches.httpClient.GetStreamAsync(url+ "/v1/wars/").Result)
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        warlist = serializer.Deserialize<List<int>>(reader);
                    }
                }
            }
         
            var dbwars = DAL.KillBoard.GetWarStatus(tq);

            Parallel.ForEach(Enumerable.Range(1, warlist.Max()), new ParallelOptions() {MaxDegreeOfParallelism = 10}, war =>
            {
                if (dbwars.ContainsKey(war) && dbwars[war][0] == 1)
                {
                    return;
                }

                List<Esi_war_kms> kmlist = EsiGetWarsKM(war,tq);

                DAL.KillBoard.AddWaiting(kmlist.Where(p => p.killmail_id > dbwars[war][1]).ToList(), tq);

            });
        }




    }
}
