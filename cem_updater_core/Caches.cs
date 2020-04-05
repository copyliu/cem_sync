using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cem_updater_core
{
    public class Utils
    {
        public class ForceHttp2Handler : DelegatingHandler
        {
            public ForceHttp2Handler(HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // request.Version = HttpVersion.Version20;
                return base.SendAsync(request, cancellationToken);
            }
        }
    }

    public  class Caches
    {
        private static DateTime _lastCache=new DateTime(2000,1,1);
        private static object _lock=new object();
        private static Dictionary<long, int> _stationSystemDictCn;
        private static Dictionary<long, int> _stationRegionDictCn;

        private static Dictionary<long, int> StationSystemDictCn
        {
            get
            {
                UpdateCaches();
                if (_stationSystemDictCn == null || _stationSystemDictCn.Count == 0)
                {
                    UpdateCaches();
                }
                return _stationSystemDictCn;
            }
        }

        private static Dictionary<long, int> StationRegionDictCn    
        {
            get
            {
                UpdateCaches();
                if (_stationRegionDictCn == null || _stationRegionDictCn.Count == 0)
                {
                    UpdateCaches();
                }
                return _stationRegionDictCn;
            }

        }

        private static void UpdateCaches()
        {
            lock (_lock)
            {
                if (_lastCache < DateTime.Now - TimeSpan.FromHours(1))
                {
                    var v = DAL.Market.GetStations().Result;
                    _stationSystemDictCn = v[0];
                    _stationRegionDictCn = v[1];
                    var w = DAL.Market.GetStations(true).Result;
                    _stationSystemDictTq = w[0];
                    _stationRegionDictTq = w[1];
                    _lastCache = DateTime.Now;
                }
            }
        }


        private static Dictionary<long, int> _stationSystemDictTq;
        private static Dictionary<long, int> _stationRegionDictTq;
        private static HttpClient _httpClient;

        private static Dictionary<long, int> StationSystemDictTq
        {
            get
            {
                UpdateCaches();
                if (_stationSystemDictTq == null || _stationSystemDictTq.Count == 0)
                {
                    UpdateCaches();
                }
                return _stationSystemDictTq;
            }
        }

        private static Dictionary<long, int> StationRegionDictTq
        {
            get
            {
                UpdateCaches();
                if (_stationRegionDictTq == null || _stationRegionDictTq.Count == 0)
                {
                    UpdateCaches();
                }
                return _stationRegionDictTq;
            }

        }


        public static HttpClient httpClient => _httpClient ??= new HttpClient(new Utils.ForceHttp2Handler(new SocketsHttpHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
         
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 50
    

        }));

        public static Dictionary<long, int> GetStationRegionDict(bool tq=false)
        {
            return tq ? StationRegionDictTq : StationRegionDictCn;
        }
        public static Dictionary<long, int> GetStationSystemDict(bool tq = false)
        {
            return tq ? StationSystemDictTq : StationSystemDictCn;
        }
    }
}
