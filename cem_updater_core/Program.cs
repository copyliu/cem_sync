using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
//            NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            DAL.connectionstring_cn = Configuration["cndb"];
            DAL.connectionstring_tq = Configuration["tqdb"];





            bool keepRunning = true;
//            _aTimer1.Start();
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
//            throw new NotImplementedException();
            var regions = DAL.GetRegions(true);
            foreach (var region in regions)
            {
                var oldorders = DAL.GetCurrentMarkets(region,true).AsParallel();
                var oldlist = oldorders.GroupBy(p => p.orderid).ToDictionary(g => g.Key, g => g.First());
                var oldorderids = oldorders.Select(p => p.orderid).ToHashSet();
                var orders = GetESIOrders(region, true);


                List<ESIMarketOrder> newlist = new List<ESIMarketOrder>();
                List<ESIMarketOrder> updatelist = new List<ESIMarketOrder>();
                Dictionary<long, HashSet<int>> updatedtypes = new Dictionary<long, HashSet<int>>();
                foreach (var crest in orders)
                {
                    if (!Caches.StationRegionDictCn.ContainsKey(crest.location_id))
                    {
                        continue;//TODO:此空间站是空堡, 暂不收录
                    }
                    if (oldorderids.Contains(crest.order_id))
                    {

                        if (crest != oldlist[crest.order_id])
                        {
                            updatelist.Add(crest);
                            if (!updatedtypes.ContainsKey(Caches.StationRegionDictCn[crest.location_id]))
                            {
                                updatedtypes.Add(Caches.StationRegionDictCn[crest.location_id], new HashSet<int>());
                            }
                            updatedtypes[Caches.StationRegionDictCn[crest.location_id]].Add(crest.type_id);
                        }

                        oldorderids.Remove(crest.order_id);

                    }
                    else
                    {
                        newlist.Add(crest);

                        if (!updatedtypes.ContainsKey(Caches.StationRegionDictCn[crest.location_id]))
                        {
                            updatedtypes.Add(Caches.StationRegionDictCn[crest.location_id], new HashSet<int>());
                        }
                        updatedtypes[Caches.StationRegionDictCn[crest.location_id]].Add(crest.type_id);
                    }

                }

                var deletelist = oldorderids.ToList();
                foreach (var oldorder in oldorders.Where(p => oldorderids.Contains(p.orderid)).GroupBy(p => p.regionid).Select(p =>
                       new { regionid = p.Key, types = p.Select(o => o.typeid).Distinct().ToList() }).Distinct())
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

                DAL.UpdateDatabase(newlist, updatelist, deletelist, updatedtypes,true);





            }
        }

        private static List<ESIMarketOrder> GetESIOrders(int regionid,bool tq=false)
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
            List<ESIMarketOrder> results=new List<ESIMarketOrder>();
            MyWebClient client = new MyWebClient();
            using (var s = client.OpenRead(url))
            {
                if (!int.TryParse(client.ResponseHeaders["x-pages"], out pages))
                {
                    pages = 1;
                }
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
                            var result=GetESIOrders(url + $"&page={pagenum}");
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
            MyWebClient client = new MyWebClient();
            List<ESIMarketOrder> crestresult;
            using (var s = client.OpenRead(url))
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
            var regions = DAL.GetRegions();
            
        

                foreach (var region in regions)
                {
                    var oldorders = DAL.GetCurrentMarkets(region).AsParallel();
                    var oldlist = oldorders.GroupBy(p=>p.orderid).ToDictionary(g=>g.Key,g=>g.First());
                    var oldorderids = oldorders.Select(p => p.orderid).ToHashSet();
                  
                    string url = $"https://api-serenity.eve-online.com.cn/market/{region}/orders/all/";
                   
                    CrestMarketResult crestresult = null;
                    List<CrestOrder> orders = new List<CrestOrder>();
                    crestresult = GetCrestMarketResult(url);


                    if (crestresult.items != null)
                    {
                        orders.AddRange(crestresult.items);
                    }


                    while (crestresult.next!=null)
                    {
                        crestresult = GetCrestMarketResult(crestresult.next.href);

                        if (crestresult.items != null)
                        {
                            orders.AddRange(crestresult.items);
                        }

                    }
                    List<CrestOrder> newlist = new List<CrestOrder>();
                    List<CrestOrder> updatelist = new List<CrestOrder>();
                    Dictionary<long,HashSet<int>> updatedtypes =new Dictionary<long, HashSet<int>>();
                    foreach (var crest in orders)
                    {
                        if (!Caches.StationRegionDictCn.ContainsKey(crest.stationID))
                        {
                            continue;//TODO:此空间站是空堡, 暂不收录
                        }
                    if (oldorderids.Contains(crest.id))
                        {
                            
                            if (crest != oldlist[crest.id])
                            {
                                updatelist.Add(crest);
                                if (!updatedtypes.ContainsKey(Caches.StationRegionDictCn[crest.stationID]))
                                {
                                    updatedtypes.Add(Caches.StationRegionDictCn[crest.stationID],new HashSet<int>());
                                }
                                updatedtypes[Caches.StationRegionDictCn[crest.stationID]].Add(crest.type);
                            }
                            
                            oldorderids.Remove(crest.id);

                        }
                        else
                        {
                            newlist.Add(crest);
                           
                            if (!updatedtypes.ContainsKey(Caches.StationRegionDictCn[crest.stationID]))
                            {
                                updatedtypes.Add(Caches.StationRegionDictCn[crest.stationID], new HashSet<int>());
                            }
                            updatedtypes[Caches.StationRegionDictCn[crest.stationID]].Add(crest.type);
                        }
                        
                    }

                    var deletelist = oldorderids.ToList();
                    foreach (var oldorder in oldorders.Where(p=> oldorderids.Contains(p.orderid)).GroupBy(p=>p.regionid).Select(p=>
                        new {regionid=p.Key,types=p.Select(o=>o.typeid).Distinct().ToList()}).Distinct())
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
                    
                    DAL.UpdateDatabase(newlist, updatelist, deletelist,updatedtypes);
                           
                            
                           
                      




                }
            
        }

        private static CrestMarketResult GetCrestMarketResult(string url)
        {
            Log(url);
            MyWebClient client=new MyWebClient();
            CrestMarketResult crestresult;
            using (var s = client.OpenRead(url))
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

        
    }
}
