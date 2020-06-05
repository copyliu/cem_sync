using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;
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


           
           
            // ConcurrentBag<List<Get_markets_region_id_orders_200_ok>> res=new ConcurrentBag<List<Get_markets_region_id_orders_200_ok>>();



            var tasks=Enumerable.Range(1, pages).ToList().Select(p =>
            
                client.Get_markets_region_id_ordersAsync(regionid, p, linkedCts.Token, lastModified)
            ).ToList();

           
           await Task.WhenAll(tasks);
           return tasks.SelectMany(p=>p.Result).Where(p => p.Volume_remain > 0).ToList();

            //
            //
            // await   Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync(Enumerable.Range(1, pages), async pagenum =>
            //  {
            //
            //      try
            //      {
            //          _logger.LogInformation("GET Market ESI " + regionid + " TQ:" + IsTQ + " Page " + pagenum);
            //          var r = await client.Get_markets_region_id_ordersAsync(regionid, pagenum, linkedCts.Token, lastModified).ConfigureAwait(false);
            //          res.Add(r.ToList());
            //
            //      }
            //      catch (Exception e)
            //      {
            //            cts.Cancel();
            //            throw new Exception("Error " +e.Message);
            //      }
            //  },10, cts.Token);
            //


            // var page1 = result.Result.ToList();
            // page1.AddRange(loadtasks.SelectMany(p=>p.Result));
            // return res.SelectMany(p => p).Where(p => p.Volume_remain > 0).ToList();
            // return page1.Where(p=>p.Volume_remain>0).ToList();
        }

        protected async Task Update(CancellationToken stoppingToken)
        {
            MarketDB db1 = IsTQ ? (MarketDB)_service.GetService<TQMarketDB>() : _service.GetService<CNMarketDB>();
            var regions = await db1.regions.AsNoTracking().OrderBy(p => p.regionid).ToListAsync();


            var downloadTasks = new Dictionary<long, Task<List<Get_markets_region_id_orders_200_ok>>>();
            
                long? previd = null;
                foreach (var region in regions)
                {
                    if (!previd.HasValue)
                    {
                        downloadTasks[region.regionid] = GetESIOrders((int)region.regionid,  stoppingToken);
                    }
                    else
                    {
                        downloadTasks[region.regionid] = downloadTasks[previd.Value]
                            .ContinueWith(t => GetESIOrders((int)region.regionid,  stoppingToken), stoppingToken)
                            .Unwrap();
                    }

                    previd = region.regionid;

                }
            
           


            foreach (var region in regions)
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }

                    _logger.LogInformation("开始下载订单: Region: " + region.regionid + $" TQ: {IsTQ}");


                    var dttoday = MarketDB.ChinaTimeZone.AtStartOfDay(DateTime.Today.ToLocalDateTime().Date);
                    var dtnext = dttoday.Plus(Duration.FromDays(1));
                    List<Get_markets_region_id_orders_200_ok> neworder;

                    try
                    {
                       
                            neworder = await downloadTasks[region.regionid];
                       
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation("下载订单失败: Region: " + region.regionid + $" TQ: {IsTQ}  " + e.Message);
                        continue;
                    }
                
                   
                   
                    var db = IsTQ ? (MarketDB)_service.GetService<TQMarketDB>() : _service.GetService<CNMarketDB>();
                 
                    var order = await EntityFrameworkQueryableExtensions.ToDictionaryAsync<current_market, long, current_market>(db.current_market.Where(p => p.regionid == region.regionid), p => p.orderid, p => p);



                    var allorders = neworder.GroupBy(p => p.Order_id).Select(p => p.First()).ToList();
                    _logger.LogInformation("完成下载订单: Region: " + region.regionid +
                                           $" TQ: {IsTQ}; APIOrder {neworder.Count}/{allorders.Count}");
                    var alltypes = allorders.Select(p => p.Type_id).Distinct().ToList();

                    ConcurrentDictionary<long, current_market> orders =
                        new ConcurrentDictionary<long, current_market>(order);

                    ConcurrentBag<current_market> insertorder = new ConcurrentBag<current_market>();

                    // Parallel.ForEach(allorders, p =>
                    foreach (var p in allorders)

                    {
                        current_market market;
                        var hasold = orders.TryGetValue(p.Order_id, out market);
                        if (!hasold)
                        {
                            market = new current_market();
                            market.orderid = p.Order_id;
                            insertorder.Add(market);
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
                        if (hasold &&
                            db.Entry(market).State == EntityState.Modified)
                        {
                            market.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                        }
                        else if (!hasold)
                        {
                            market.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                        }
                    }

                    var todayhistsory = await EntityFrameworkQueryableExtensions.ToDictionaryAsync<market_markethistorybyday, int, market_markethistorybyday>(db.market_markethistorybyday.Where(p =>
                        p.regionid == region.regionid && alltypes.Contains(p.typeid) &&
                        p.date == dttoday.Date), p => p.typeid, p => p);


                    var realtimehistory =

                            !IsTQ
                                ? await EntityFrameworkQueryableExtensions.ToDictionaryAsync<market_realtimehistory, int, market_realtimehistory>(db.evetypes.Where(p => alltypes.Contains(p.typeID)).Select(ip => ip
                                            .market_realtimehistory.Where(p =>
                                                p.regionid == region.regionid && p.date >= dttoday.ToInstant() &&
                                                p.date < dtnext.ToInstant()).OrderByDescending(o => o.date).First())
                                        .Where(p => p != null), p => p.typeid, p => p)

                                : null
                        ;

                    foreach (var rt in allorders.AsParallel().GroupBy(p => p.Type_id,
                        (i, oks) => new {Key = i, Value = oks.ToList().AsParallel()}))
                    {
                        var hassellorder = rt.Value.Any(p => p.Is_buy_order == false);
                        var hasbuyorder = rt.Value.Any(p => p.Is_buy_order == true);
                        if (hassellorder)
                        {
                            market_markethistorybyday hisbydate;
                            var price = rt.Value.Where(p => !p.Is_buy_order).Min(o => o.Price);
                            if (!todayhistsory.TryGetValue(rt.Key, out hisbydate))
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
                            if (hisbydate.min > 0)
                            {
                                hisbydate.min = Math.Min(hisbydate.min, price);
                            }
                            else
                            {
                                hisbydate.min = price;
                            }
                            



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
                    var delteids = orders.Select(p => p.Key).Where(p => !allids.Contains(p));

                    _logger.LogInformation("开始保存订单: Region: " + region.regionid + $" TQ: {IsTQ}");

                    await using var trans = await db.Database.BeginTransactionAsync();

                    try
                    {
                        db.current_market.AddRange(insertorder);
                        await db.Database.ExecuteSqlInterpolatedAsync(
                            $"delete from current_market  where orderid = any ({delteids.ToList()}) and regionid = {region.regionid};");

                        await db.SaveChangesAsync();
                        await trans.CommitAsync();
                        _logger.LogInformation("完成保存订单: Region: " + region.regionid + $" TQ: {IsTQ}");
                    }
                    finally
                    {
                        foreach (var entityEntry in db.ChangeTracker.Entries().ToArray())
                        {
                            entityEntry.State = EntityState.Detached;
                        }
                    }



                }
                catch (Exception e)
                {
                    _logger.LogError("出现错误" + e);
                }

            }



        }

    }
}