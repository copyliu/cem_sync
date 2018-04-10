using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        private static string connectionstring_cn;
        private static string connectionstring_tq;

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

            connectionstring_cn = Configuration["cndb"];
            connectionstring_tq = Configuration["tqdb"];





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
            List<int> regions = new List<int>();
            using (var conn = new NpgsqlConnection(connectionstring_cn))
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

                List<CurrentMarket> oldlist = new List<CurrentMarket>();
                foreach (var region in regions)
                {
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from current_market where regionid=@region;";
                        cmd.Parameters.AddWithValue("region", region);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                oldlist.Add(new CurrentMarket()
                                {
                                    id = (long) reader["id"],
                                    regionid = (long) reader["regionid"],
                                    systemid = (long) reader["systemid"],
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
                        }

                    }

                    var grouped_oldlist = oldlist.AsParallel().GroupBy(p => p.typeid)
                        .ToDictionary(markets => markets.Key, markets => markets.ToList());
                    string url = $"https://api-serenity.eve-online.com.cn/market/{region}/orders/all/";
                    MyWebClient client =new MyWebClient();
                    Log(url);
                    var res=client.DownloadString(url);
                    List<Crest> orders =new List<Crest>();
                    var tmp = JObject.Parse(res);

                    List<Crest> re = JsonConvert.DeserializeObject<List<Crest>>(tmp["items"].ToString());
                    orders.AddRange(re);
                    while (tmp.ContainsKey("next"))
                    {
                        Log(tmp["next"]["href"].ToString());
                        res =client.DownloadString(tmp["next"]["href"].ToString());
                        tmp = JObject.Parse(res);
                        re = JsonConvert.DeserializeObject<List<Crest>>(tmp["items"].ToString());
                        orders.AddRange(re);
                    }

                    Parallel.ForEach(orders.GroupBy(p => p.type),new ParallelOptions(){MaxDegreeOfParallelism = 10}, c =>
                    {
                        List<CurrentMarket> newlist=new List<CurrentMarket>();
                        List<CurrentMarket> updatelist=new List<CurrentMarket>();
                        List<long> deletelist=new List<long>();
                        if (grouped_oldlist.ContainsKey(c.Key))
                        {

                        }
                        else
                        {
                            foreach (var crest in c)
                            {
                                newlist.Add(new CurrentMarket()
                                {id=0,
                                    regionid = region,
                                    systemid = crest.,
                                    stationid = ,
                                    typeid = ,
                                    bid = ,
                                    price = ,
                                    orderid = ,
                                    minvolume = ,
                                    volremain = ,
                                    volenter = ,
                                    issued = ,
                                    range = ,
                                    reportedby = ,
                                    reportedtime = ,
                                    source = ,
                                    interval = ,
                                });
                            }
                        }
                    });
                   



                }
            }
        }
    }
}
