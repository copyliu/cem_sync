using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

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

            DAL.connectionstring_cn = Configuration["cndb"];
            DAL.connectionstring_tq = Configuration["tqdb"];





            bool keepRunning = true;
            _aTimer1.Start();
//            _aTimer2.Start();
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
        }

        private static void SyncCN()
        {
            var regions = DAL.GetRegions();
            var stations = DAL.GetStations();
            using (var conn = new NpgsqlConnection(DAL.connectionstring_cn))
            {
                conn.Open();


                foreach (var region in regions)
                {
                    var oldorders = DAL.GetCurrentMarkets(region);
                    var oldlist = oldorders.AsParallel().GroupBy(p=>p.orderid).ToDictionary(g=>g.Key,g=>g.First());
                    var oldorderids = oldorders.Select(p => p.orderid).ToHashSet();
                  
                    string url = $"https://api-serenity.eve-online.com.cn/market/{region}/orders/all/";
                    MyWebClient client = new MyWebClient();
                    Log(url);
                    var res = client.DownloadString(url);
                    List<CrestOrder> orders = new List<CrestOrder>();
                    var tmp = JObject.Parse(res);

                    List<CrestOrder> re = JsonConvert.DeserializeObject<List<CrestOrder>>(tmp["items"].ToString());
                    orders.AddRange(re);
                    while (tmp.ContainsKey("next"))
                    {
                        Log(tmp["next"]["href"].ToString());
                        res = client.DownloadString(tmp["next"]["href"].ToString());
                        tmp = JObject.Parse(res);
                        re = JsonConvert.DeserializeObject<List<CrestOrder>>(tmp["items"].ToString());
                        orders.AddRange(re);
                    }
                    List<CrestOrder> newlist = new List<CrestOrder>();
                    List<CrestOrder> updatelist = new List<CrestOrder>();
                    
                    
                    foreach (var crest in orders)
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
                        if (oldorderids.Contains(crest.id))
                        {
                            //TODO 判斷下訂單狀態是否變動過

                            updatelist.Add(crest);
                            oldorderids.Remove(crest.id);

                        }
                        else
                        {
                            newlist.Add(crest);
                        }
                        
                    }

                    var deletelist = oldorderids.ToList();
                    DAL.UpdateDatabase(newlist, updatelist, deletelist);
                           
                            
                           
                      




                }
            }
        }
    }
}
