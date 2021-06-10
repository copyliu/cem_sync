using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;
using CEMSync.Helpers;
using EVEMarketSite.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;

namespace CEMSync.Service.EVEMaps
{
    public class TQMarketUpdater : MarketUpdater
    {
        public TQMarketUpdater(IHttpClientFactory httpClientFactory, ILogger<MarketUpdater> logger,
            IServiceProvider service) : base(httpClientFactory, logger, service)
        {
            // var http = new HttpClient(new SocketsHttpHandler() {AutomaticDecompression = DecompressionMethods.All})
            // {
            //     Timeout = TimeSpan.FromSeconds(30),
            //     BaseAddress = new Uri("https://esi.evetech.net/latest/"),
            //
            // };
            // http.DefaultRequestHeaders.Add("User-Agent",
            //     "CEVE-MARKET slack-copyliu CEMSync-Service");
            // this.client = new ESIClient(http); 
            var http = httpClientFactory.CreateClient("TQ");
            this.client = service.GetService<ITypedHttpClientFactory<ESIClient>>().CreateClient(http);
            this.client.IsTq = true;
            //service.GetService<ITypedHttpClientFactory<ESIClient>>().CreateClient(http);
            this.IsTQ = true;
        }

 


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                var delay = Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stoppingToken);

                    await Update(linkedCts.Token);
                }
                catch (Exception e)
                {
                    _logger.LogError("出现错误" + e);
                }
                
                try
                {
                    await delay;
                }
                catch
                {
                    // ignored
                }
            }
          

        }
    }

    public class CNMarketUpdater : MarketUpdater
    {
        public CNMarketUpdater(IHttpClientFactory httpClientFactory, ILogger<MarketUpdater> logger,
            IServiceProvider service) : base(httpClientFactory, logger, service)
        {
           var http = httpClientFactory.CreateClient("CN");
           this.client = service.GetService<ITypedHttpClientFactory<ESIClient>>().CreateClient(http);
           this.IsTQ = false;
           this.client.IsTq = false;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                var delay = Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));

                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stoppingToken);

                    await Update(linkedCts.Token);
                }
                catch (Exception e)
                {
                    _logger.LogError("出现错误" + e);
                }
                try
                {
                    await delay;
                }
                catch 
                {
                    // ignored
                }
            }

        }
    }

    public abstract class MarketUpdater : BackgroundService
    {
        protected readonly ILogger<MarketUpdater> _logger;
        protected readonly IServiceProvider _service;
        protected ESIClient client;
        protected bool IsTQ=false;
        public MarketUpdater(IHttpClientFactory httpClientFactory, ILogger<MarketUpdater> logger,
            IServiceProvider service)
        {
            _logger = logger;
            _service = service;
        }

        async Task UpdateCitidals(CancellationToken stoppingToken)
        {

            using var sp = _service.CreateScope();
            await using MarketDB db1 = IsTQ ? sp.ServiceProvider.GetService<TQMarketDB>() : sp.ServiceProvider.GetService<CNMarketDB>();
            
        
            long[] citidals;
            try
            {
                citidals = await this.client.GetAllCitidalIds(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, $"更新建筑物失败 {IsTQ} " + e);
                return;
            }
          
            var tasks = citidals.Select(p => new {p,task= this.client.GetCitidal(p)}).ToList();
            var oldstations = await db1.stations.Where(p => p.stationid > int.MaxValue).ToListAsync();
            try
            {
                await Task.WhenAll(tasks.Select(p => p.task));
            }
            catch (Exception e)
            {
                _logger.LogInformation(e,$"更新建筑物失败 {IsTQ} "+e );
              return;
            }
         
            foreach (var task in tasks.Where(p=>p.task.Result!=null))
            {
                var citidalinfo = task.task.Result;
                var model = oldstations.FirstOrDefault(p => p.stationid == task.p);
                if (model == null)
                {
                    model=new stations();
                    model.stationid = task.p;
                    db1.stations.Add(model);

                }

                var name = citidalinfo.Name.Split(" - ")[0];
                model.systemid = citidalinfo.Solar_system_id;
                model.corpid = citidalinfo.Owner_id;
                // var mod = await db1.evetypes.FirstOrDefaultAsync(p => p.typeID == citidalinfo.Type_id);
                // var typename = mod?.typeName ?? "玩家建筑物";
                model.stationname = citidalinfo.Name;
                model.typeid = citidalinfo.Type_id;


            }
            db1.RemoveRange(db1.stations.Where(p=>p.stationid>int.MaxValue).Where(p=>!citidals.Contains(p.stationid)));
            await db1.SaveChangesAsync(stoppingToken);
        }

        async Task<List<Get_markets_structures_structure_id_200_ok>> GetStructOrders(long structid,
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("GET Market ESI " + structid + " TQ:" + IsTQ);

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stoppingToken);

            var pageheaders = await client.GetMarketstructureOrdersHeaders(structid, linkedCts.Token);
            if (pageheaders.StatusCode == HttpStatusCode.Forbidden ||
                pageheaders.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new List<Get_markets_structures_structure_id_200_ok>();
            }
            if (!pageheaders.IsSuccessStatusCode)
            {
                throw new Exception($"structid {structid} order Error");
            }
            var lastModified = pageheaders.Content.Headers.LastModified;
            int pages;
            if (!pageheaders.Headers.TryGetValues("x-pages", out var xPages) ||
                !int.TryParse(xPages.FirstOrDefault(), out pages))
            {
                pages = 1;
            }

            // ConcurrentBag<List<Get_markets_region_id_orders_200_ok>> res=new ConcurrentBag<List<Get_markets_region_id_orders_200_ok>>();


            var tasks = Enumerable.Range(1, pages).ToList().Select(p =>

                client.Get_markets_structure_ordersAsync(structid, p, linkedCts.Token, lastModified)
            ).ToList();


            await Task.WhenAll(tasks);
            return tasks.SelectMany(p => p.Result).Where(p => p.Volume_remain > 0).ToList();

        }


        async Task<List<Get_markets_region_id_orders_200_ok>> GetESIOrders(int regionid,  CancellationToken stoppingToken)
        {
         
            _logger.LogInformation("GET Market ESI " + regionid + " TQ:" + IsTQ);

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stoppingToken);

            var pageheaders = await client.GetMarketOrdersHeaders(regionid, linkedCts.Token);

            var lastModified = pageheaders.Content.Headers.LastModified;
            int pages;
            if (!pageheaders.Headers.TryGetValues("x-pages", out var xPages) ||
                !int.TryParse(xPages.FirstOrDefault(), out pages))
            {
                pages = 1;
            }


            


            var tasks=Enumerable.Range(1, pages).ToList().Select(p =>
            
                client.Get_markets_region_id_ordersAsync(regionid, p, linkedCts.Token, lastModified)
            ).ToList();

           
           await Task.WhenAll(tasks);
           return tasks.SelectMany(p=>p.Result).Where(p => p.Volume_remain > 0).ToList();
 
        }

        protected async Task Update(CancellationToken stoppingToken)
        {
            await UpdateCitidals(stoppingToken);

            using var sp = _service.CreateScope();
            await using MarketDB db1 = IsTQ ? sp.ServiceProvider.GetService<TQMarketDB>() : sp.ServiceProvider.GetService<CNMarketDB>();
 
            var regions =  db1.regions.AsNoTracking().OrderBy(p => p.regionid).AsAsyncEnumerable();
            var stations = await db1.stations.Where(p => p.stationid > int.MaxValue).ToDictionaryAsync(p => p.stationid, p => p.systemid, cancellationToken: stoppingToken);

            await regions.AsyncParallelForEach(p => UpdateRegion(p, stations, stoppingToken), 3);
            
           
            // foreach (var region in regions)
            // {
            //     await UpdateRegion(region, stations, stoppingToken);
            //
            //
            // }

        }

        private async Task UpdateRegion(regions region, Dictionary<long, int> stations,
            CancellationToken stoppingToken)
        {
            try
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return ;
                }

                _logger.LogInformation("开始下载订单: Region: " + region.regionid + $" TQ: {IsTQ}");


                var dttoday = MarketDB.ChinaTimeZone.AtStartOfDay(DateTime.Today.ToLocalDateTime().Date);
                var dtnext = dttoday.Plus(Duration.FromDays(1));
                List<Get_markets_region_id_orders_200_ok> neworder;

                try
                {
                    neworder = await GetESIOrders((int)region.regionid, stoppingToken); 
                }
                catch (Exception e)
                {
                    _logger.LogInformation("下载订单失败: Region: " + region.regionid + $" TQ: {IsTQ}  " + e.Message);
                    return ;
                }

                using var sp = _service.CreateScope();
                await using MarketDB db = IsTQ ? sp.ServiceProvider.GetService<TQMarketDB>() : sp.ServiceProvider.GetService<CNMarketDB>();

            
              
                var currentOrders = await db.current_market.Where(p => p.regionid == region.regionid).ToListAsync(cancellationToken: stoppingToken);


                _logger.LogInformation("完成下载订单: Region: " + region.regionid +
                                       $" TQ: {IsTQ}; APIOrder {neworder.Count()}");

                var citidal = currentOrders.Where(p => p.stationid > int.MaxValue).Select(p => p.stationid)
                    .Distinct().ToList();
                citidal.AddRange(neworder.Where(p => p.Location_id > int.MaxValue).Select(p => p.Location_id).Distinct());
                citidal = citidal.Distinct().Where(stations.ContainsKey).ToList();

                var citidaltasks = citidal.Select(p => GetStructOrders(p, stoppingToken)).ToList();
                await Task.WhenAll(citidaltasks);

                var citidalorders = citidaltasks.AsParallel().SelectMany(p => p.Result).AsParallel()
                    .Where(p => stations.ContainsKey(p.Location_id))
                    .Select(citidaltask =>
                    {
                        var orderinfo = new Get_markets_region_id_orders_200_ok()
                        {
                            Location_id = citidaltask.Location_id,
                            Duration = citidaltask.Duration,
                            Is_buy_order = citidaltask.Is_buy_order,
                            Issued = citidaltask.Issued,
                            Min_volume = citidaltask.Min_volume,
                            Order_id = citidaltask.Order_id,
                            Price = citidaltask.Price,
                            Type_id = citidaltask.Type_id,
                            Volume_remain = citidaltask.Volume_remain,
                            Volume_total = citidaltask.Volume_total
                        };
                        orderinfo.System_id = (int) stations[citidaltask.Location_id];
                        orderinfo.Range = citidaltask.Range switch
                        {
                            Get_markets_structures_structure_id_200_okRange.Region =>
                                Get_markets_region_id_orders_200_okRange.Region,
                            Get_markets_structures_structure_id_200_okRange.Solarsystem =>
                                Get_markets_region_id_orders_200_okRange.Solarsystem,
                            Get_markets_structures_structure_id_200_okRange.Station =>
                                Get_markets_region_id_orders_200_okRange.Station,
                            Get_markets_structures_structure_id_200_okRange._1 =>
                                Get_markets_region_id_orders_200_okRange._1,
                            Get_markets_structures_structure_id_200_okRange._2 =>
                                Get_markets_region_id_orders_200_okRange._2,
                            Get_markets_structures_structure_id_200_okRange._3 =>
                                Get_markets_region_id_orders_200_okRange._3,
                            Get_markets_structures_structure_id_200_okRange._4 =>
                                Get_markets_region_id_orders_200_okRange._4,
                            Get_markets_structures_structure_id_200_okRange._5 =>
                                Get_markets_region_id_orders_200_okRange._5,
                            Get_markets_structures_structure_id_200_okRange._10 =>
                                Get_markets_region_id_orders_200_okRange._10,
                            Get_markets_structures_structure_id_200_okRange._20 =>
                                Get_markets_region_id_orders_200_okRange._20,
                            Get_markets_structures_structure_id_200_okRange._30 =>
                                Get_markets_region_id_orders_200_okRange._30,
                            Get_markets_structures_structure_id_200_okRange._40 =>
                                Get_markets_region_id_orders_200_okRange._40,
                            _ => orderinfo.Range
                        };
                        return orderinfo;
                    });

                neworder.AddRange(citidalorders);

                var allorders = neworder.AsParallel().GroupBy(p => p.Order_id).Select(p => p.First()).ToList();

                _logger.LogInformation("完成下载玩家建筑物订单: Region: " + region.regionid +
                                       $" TQ: {IsTQ}; ");


                var alltypes = allorders.Select(p => p.Type_id).Distinct().ToList();

                // ConcurrentDictionary<long, current_market> orders =
                //     new ConcurrentDictionary<long, current_market>(currentOrders);

                var insertorder = new ConcurrentBag<current_market>();


                var oldorders = allorders.AsParallel().GroupJoin(currentOrders.AsParallel(), ok => ok.Order_id,
                    market => market.orderid, (p, market) => new
                    {
                        p, m = market.FirstOrDefault()
                    });

                oldorders.ForAll(obj =>
                {
                    current_market market;
                    var p = obj.p;
                    if (obj.m == null)
                    {
                        market = new current_market
                        {
                            orderid = p.Order_id, reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now)
                        };
                        insertorder.Add(market);
                    }
                    else
                    {
                        market = obj.m;
                    }

                    market.stationid = p.Location_id;
                    market.typeid = p.Type_id;
                    market.interval = p.Duration;
                    market.minvolume = p.Min_volume;
                    market.volremain = p.Volume_remain;
                    market.issued = Instant.FromDateTimeOffset(p.Issued);
                    market.volenter = p.Volume_total;
                    market.price = p.Price;
                    market.bid = p.Is_buy_order ? 1 : 0;
                    market.range = p.Range.ConvertRange();
                    market.systemid = p.System_id;
                    market.regionid = region.regionid;
                });


                // Parallel.ForEach(allorders, p =>
                // foreach (var p in allorders)


                var todayhistsory = await db.market_markethistorybyday.Where(p =>
                    p.regionid == region.regionid && alltypes.Contains(p.typeid) &&
                    p.date == dttoday.Date).ToDictionaryAsync(p => p.typeid, p => p, cancellationToken: stoppingToken);


                var realtimehistory =
                        !IsTQ
                            ? await db.evetypes.Where(p => alltypes.Contains(p.typeID)).Select(ip => ip
                                    .market_realtimehistory.Where(p =>
                                        p.regionid == region.regionid && p.date >= dttoday.ToInstant() &&
                                        p.date < dtnext.ToInstant()).OrderByDescending(o => o.date).First())
                                .Where(p => p != null).ToDictionaryAsync(p => p.typeid, p => p, cancellationToken: stoppingToken)
                            : null
                    ;

                foreach (var rt in allorders.AsParallel().GroupBy(p => p.Type_id,
                    (i, oks) => new {Key = i, Value = oks.ToList().AsParallel()}))
                {
                    var hassellorder = rt.Value.Any(p => p.Is_buy_order == false);
                    var hasbuyorder = rt.Value.Any(p => p.Is_buy_order == true);
                    if (hassellorder)
                    {
                        var price = rt.Value.Where(p => !p.Is_buy_order).Min(o => o.Price);
                        if (!todayhistsory.TryGetValue(rt.Key, out var hisbydate))
                        {
                            hisbydate = new market_markethistorybyday();
                            hisbydate.date = LocalDate.FromDateTime(DateTime.Today);
                            hisbydate.regionid = region.regionid;
                            hisbydate.typeid = rt.Key;
                            hisbydate.start = price;
                            db.market_markethistorybyday.Add(hisbydate);
                        }

                        hisbydate.end = price;
                        hisbydate.max = Math.Max(hisbydate.max, price);
                        hisbydate.min = hisbydate.min > 0 ? Math.Min(hisbydate.min, price) : price;
                    }

                    if (!IsTQ && rt.Value.Any())
                    {
                        market_realtimehistory realtime;
                        realtime = new market_realtimehistory
                        {
                            regionid = region.regionid,
                            typeid = rt.Key,
                            date = Instant.FromDateTimeOffset(DateTimeOffset.Now),
                            buy = hasbuyorder
                                ? rt.Value.Where(p => p.Is_buy_order).Max(p => p.Price)
                                : 0,
                            sell = hassellorder
                                ? rt.Value.Where(p => !p.Is_buy_order).Min(p => p.Price)
                                : 0,
                            buyvol = hasbuyorder
                                ? rt.Value.Where(p => p.Is_buy_order).Sum(p => (long) p.Volume_remain)
                                : 0,
                            sellvol = hassellorder
                                ? rt.Value.Where(p => !p.Is_buy_order).Sum(p => (long) p.Volume_remain)
                                : 0,
                        };

                        if (realtimehistory.TryGetValue(rt.Key, out var oldrealtime))
                        {
                            if (Math.Abs(oldrealtime.buy - realtime.buy) < 0.001 &&
                                Math.Abs(oldrealtime.sell - realtime.sell) < 0.001 &&
                                Math.Abs(oldrealtime.buyvol - realtime.buyvol) < 1 &&
                                Math.Abs(oldrealtime.sellvol - realtime.sellvol) < 1
                            )

                                realtime = null;
                        }


                        if (realtime != null)
                        {
                            db.market_realtimehistory.Add(realtime);
                        }
                    }
                }

                var allids = allorders.AsParallel().Select(o => o.Order_id).Distinct().ToList();
                var delteids = currentOrders.Select(p => p.orderid).Where(p => !allids.Contains(p));

                _logger.LogInformation("开始保存订单: Region: " + region.regionid + $" TQ: {IsTQ}");

                await using var trans = await db.Database.BeginTransactionAsync(CancellationToken.None);

                try
                {
                    db.current_market.AddRange(insertorder);
                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $"delete from current_market  where orderid = any ({delteids.ToList()}) and regionid = {region.regionid};", CancellationToken.None);

                    await db.SaveChangesAsync(CancellationToken.None);
                    await trans.CommitAsync(CancellationToken.None);
                    _logger.LogInformation("完成保存订单: Region: " + region.regionid + $" TQ: {IsTQ}");
                }
                finally
                {
                  
                }
            }
            catch (Exception e)
            {
                _logger.LogError("出现错误" + e);
            }

        }
    }
}