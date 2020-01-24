using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using cem_updater_core.DAL;
using cem_updater_core.Model;
using Dasync.Collections;
using Microsoft.Extensions.Configuration;
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
        private static System.Timers.Timer _aTimer3 = new System.Timers.Timer(1000) {AutoReset = false};
        private static System.Timers.Timer _aTimer4 = new System.Timers.Timer(1000) {AutoReset = false};

        private static System.Threading.ManualResetEvent _event1 = new ManualResetEvent(true);
        private static System.Threading.ManualResetEvent _event2 = new ManualResetEvent(true);
        private static System.Threading.ManualResetEvent _event3 = new ManualResetEvent(true);
        private static System.Threading.ManualResetEvent _event4 = new ManualResetEvent(true);
        private static WaitHandle[] events = {_event1, _event2, _event3, _event4};

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

            if (!string.IsNullOrEmpty(Configuration["ua"]))
            {
                Caches.httpClient.DefaultRequestHeaders.Add("User-Agent", Configuration["ua"].Trim());
            }

            DAL.Helpers.connectionstring_market_cn = Configuration["cndb"];
            DAL.Helpers.connectionstring_market_tq = Configuration["tqdb"];

            DAL.Helpers.connectionstring_kb_cn = Configuration["cnkbdb"];
            DAL.Helpers.connectionstring_kb_tq = Configuration["tqkbdb"];

            var cts = new CancellationTokenSource();


            bool keepRunning = true;
            isServiceRunning = true;

            _aTimer1.Elapsed += (sender, ar) =>
            {
                _event1.Reset();
                try
                {
                    Log("CN Start");
                    SyncESI(false).Wait();
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
                    SyncESI(true).Wait();
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


            _aTimer3.Elapsed += (sender, ar) =>
            {
                _event3.Reset();
                try
                {
                    Log("KM TQ Start");
                    SyncKMTQ();
                    Log("KM TQ Stop");
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                }

                _event3.Set();
                if (isServiceRunning)
                {
                    _aTimer3.Interval = 1 * 60 * 1000;
                    _aTimer3.Start();

                }
            };

            //            _aTimer3.Elapsed += async (sender, ar) =>
            //            {
            //                _event3.Reset();
            //                try
            //                {
            //                    await GetStreamTQKM(cts.Token);
            //                }
            //                catch (Exception e)
            //                {
            //                    Log(e.ToString());
            //                }
            //                _event3.Set();
            //                if (isServiceRunning)
            //                {
            //                    _aTimer3.Start();
            //
            //                }
            //            };

//            _aTimer1.Start(); //國服開關 暫時關閉
            _aTimer2.Start();
//            _aTimer3.Start();

            new Thread(async () =>
            {
                while (isServiceRunning)
                {
                    await GetZKBKM(cts.Token);
                }
            }).Start();

            Console.CancelKeyPress += delegate
            {
                Log("Shutting Down");
                isServiceRunning = false;
                _aTimer1.Stop();
                _aTimer2.Stop();
                _aTimer3.Stop();
                cts.Cancel();
                WaitHandle.WaitAll(events);
                Log("Exited!");
                keepRunning = false;
            };
            while (keepRunning)
            {
                Thread.Sleep(1000);
            }
        }

        private static void SyncKMTQ()
        {
            throw new NotImplementedException();
        }

        private static async Task GetZKBKM(CancellationToken ctsToken)
        {
            try
            {
                var req = Caches.httpClient.GetStreamAsync(
                    "https://redisq.zkillboard.com/listen.php?queueID=ceve-market.org&ttw=5");


                var crestresult = await
                    System.Text.Json.JsonSerializer.DeserializeAsync<RedisQ>(
                        await req, null, ctsToken);
                if (crestresult != null && crestresult.package != null)
                {
                    Log($"TQ NEW KM: {crestresult.package.killID}/{crestresult.package.zkb?.hash}");
                    await DAL.KillBoard.AddWaiting(
                        new Kb_waiting_api()
                            {killID = crestresult.package.killID, hash = crestresult.package.zkb?.hash},
                        tq: true);
                }




            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Log(ex.ToString());

            }


        }

        private static async Task SyncESI(bool istq = false)
        {
            var regions = await DAL.Market.GetRegions(istq);
            foreach (var region in regions)
            {



                List<ESIMarketOrder> orders;
                try
                {
                    orders = (await GetESIOrders(region, istq)).Distinct().ToList();
                }
                catch (WrongMarketSnapShotException e)
                {
                    Log(e.ToString());
                    continue;
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                    continue;
                }

                var oldorders = await DAL.Market.GetCurrentMarkets(region, istq);
                var oldlist = oldorders.GroupBy(p => p.orderid).ToDictionary(g => g.Key, g => g.First());
                var oldorderids = oldorders.Select(p => p.orderid).ToHashSet();

                List<ESIMarketOrder> newlist = new List<ESIMarketOrder>();
                List<ESIMarketOrder> updatelist = new List<ESIMarketOrder>();
                Dictionary<long, HashSet<int>> updatedtypes = new Dictionary<long, HashSet<int>>();

                foreach (var crest in orders)
                {
                    crest.regionid = region;
                    if (Caches.GetStationRegionDict(istq).ContainsKey(crest.location_id))
                    {
                        if (Caches.GetStationRegionDict(istq)[crest.location_id] != region)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        //continue; //TODO:此空间站是空堡, 暂不收录
                    }

                    if (oldorderids.Contains(crest.order_id))
                    {

                        if (crest != oldlist[crest.order_id])
                        {
                            updatelist.Add(crest);
                            if (!updatedtypes.ContainsKey(region))
                            {
                                updatedtypes.Add(region,
                                    new HashSet<int>());
                            }

                            updatedtypes[region].Add(crest.type_id);
                        }

                        oldorderids.Remove(crest.order_id);

                    }
                    else
                    {
                        newlist.Add(crest);

                        if (!updatedtypes.ContainsKey(region))
                        {
                            updatedtypes.Add(region, new HashSet<int>());
                        }

                        updatedtypes[region].Add(crest.type_id);
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

                Log($"RegionID {region}:new:{newlist.Count},update:{updatelist.Count},del:{deletelist.Count}");
                await DAL.Market.UpdateDatabaseAsync(newlist, updatelist, deletelist, updatedtypes, istq);





            }


        }

        private static async Task<List<ESIMarketOrder>> GetESIOrders(int regionid, bool tq = false)
        {
            string url;
            if (tq)
            {
                url = $"https://esi.evetech.net/v1/markets/{regionid}/orders/?datasource=tranquility&order_type=all";
            }
            else
            {
                url = $"https://esi.evepc.163.com/v1/markets/{regionid}/orders/?datasource=serenity&order_type=all";
            }

            Log(url);
            int pages;

            var headresponse = await Caches.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            if (!headresponse.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + headresponse.StatusCode);
            }

            var lastModified = headresponse.Content.Headers.LastModified;
            if (!headresponse.Headers.TryGetValues("x-pages", out var xPages) ||
                !int.TryParse(xPages.FirstOrDefault(), out pages))
            {
                pages = 1;
            }

            ConcurrentBag<ESIMarketOrder> results = new ConcurrentBag<ESIMarketOrder>();


            await Enumerable.Range(1, pages).ParallelForEachAsync(async pagenum =>
            {

                var result = await GetESIOrders(url + $"&page={pagenum}", lastModified);
                if (result != null)
                {
                    foreach (var v in result)
                    {
                        results.Add(v);
                    }

                }



            }, 10);



            return results.ToList();
        }

        private static async Task<List<ESIMarketOrder>> GetESIOrders(string url, DateTimeOffset? lastmod = null)
        {
            Log(url);

            var message = await Caches.httpClient.GetAsync(url);
            if (message.Content.Headers.LastModified != lastmod)
            {
                throw new WrongMarketSnapShotException(lastmod, message.Content.Headers.LastModified);

            }



            return await JsonSerializer.DeserializeAsync<List<ESIMarketOrder>>(
                await message.Content.ReadAsStreamAsync());



        }




        private static async Task<List<Esi_war_kms>> EsiGetWarsKM(int war, bool tq = false)
        {
            string url;
            if (tq)
            {
                url = "https://esi.evetech.net" + $"/v1/wars/{war}/killmails/";
            }
            else
            {
                throw new NotImplementedException("没有国服");
            }



            Log(url);
            var headresponse = await Caches.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            if (!headresponse.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + headresponse.StatusCode);
            }

            int pages;
            if (!headresponse.Headers.TryGetValues("x-pages", out var xPages) ||
                !int.TryParse(xPages.FirstOrDefault(), out pages))
            {
                pages = 1;
            }

            ConcurrentBag<Esi_war_kms> results = new ConcurrentBag<Esi_war_kms>();

            await Enumerable.Range(1, pages).ParallelForEachAsync(async pagenum =>
            {

                var result = await GetESIKM(url + $"?page={pagenum}");
                if (result != null)
                {
                    foreach (var esiWarKmse in result)
                    {
                        results.Add(esiWarKmse);
                    }

                }




            }, 10);


            return results.ToList();





        }

        private static async Task<List<Esi_war_kms>> GetESIKM(string url)
        {
            Log(url);




            return await JsonSerializer.DeserializeAsync<List<Esi_war_kms>>(
                await Caches.httpClient.GetStreamAsync(url));


        }

        private static async Task UpdateWars(bool tq = false)
        {
            string url;
            if (tq)
            {
                url = "https://esi.evetech.net";
            }
            else
            {
                throw new NotImplementedException("没有国服");
            }

            List<int> warlist;

            warlist = JsonSerializer
                .DeserializeAsync<List<int>>(Caches.httpClient.GetStreamAsync(url + "/v1/wars/").Result).Result;



            var dbwars = await DAL.KillBoard.GetWarStatus(tq);


            await Enumerable.Range(1, warlist.Max()).ParallelForEachAsync(async war =>
            {
                if (dbwars.ContainsKey(war) && dbwars[war][0] == 1)
                {
                    return;
                }

                List<Esi_war_kms> kmlist = await EsiGetWarsKM(war, tq);

                await DAL.KillBoard.AddWaiting(kmlist.Where(p => p.killmail_id > dbwars[war][1]).ToList(), tq);

            }, 10);

            await Enumerable.Range(1, warlist.Max()).ParallelForEachAsync(
                async war =>
                {
                    if (dbwars.ContainsKey(war) && dbwars[war][0] == 1)
                    {
                        return;
                    }

                    List<Esi_war_kms> kmlist = await EsiGetWarsKM(war, tq);

                    await DAL.KillBoard.AddWaiting(kmlist.Where(p => p.killmail_id > dbwars[war][1]).ToList(), tq);

                }, 10);
        }




    }
}
