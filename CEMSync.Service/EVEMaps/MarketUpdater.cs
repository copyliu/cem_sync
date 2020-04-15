using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;
using EVEMarketSite.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;

namespace CEMSync.Service.EVEMaps
{
    public class TQMarketUpdater : MarketUpdater
    {
        public TQMarketUpdater(ESICNService esi, ESITQService tqesi, ILogger<MarketUpdater> logger,
            IServiceProvider service) : base(esi, tqesi, logger, service)
        {
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            return Update(stoppingToken, true);

        }
    }

    public class CNMarketUpdater : MarketUpdater
    {
        public CNMarketUpdater(ESICNService esi, ESITQService tqesi, ILogger<MarketUpdater> logger,
            IServiceProvider service) : base(esi, tqesi, logger, service)
        {
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            return Update(stoppingToken, false);

        }
    }

    public abstract class MarketUpdater : BackgroundService
    {
        protected readonly ESICNService _esi;
        protected readonly ESITQService _tqesi;
        protected readonly ILogger<MarketUpdater> _logger;
        protected readonly IServiceProvider _service;

        public MarketUpdater(ESICNService esi, ESITQService tqesi, ILogger<MarketUpdater> logger,
            IServiceProvider service)
        {
            _esi = esi;
            _tqesi = tqesi;
            _logger = logger;
            _service = service;
        }

        async Task<List<Get_markets_region_id_orders_200_ok>> GetESIOrders(int regionid, bool tq)
        {
            ESIService esi;
            esi = tq ? (ESIService) this._tqesi : this._esi;

            _logger.LogInformation("GET Market ESI " + regionid + " TQ:" + tq + " Page 1");
            var result = await esi._client.Get_markets_region_id_ordersAsync(null, null,
                Order_type.All, 1, regionid,
                null);

            if (!result.Headers.TryGetValue("X-Pages", out var xPages) ||
                !int.TryParse(xPages.FirstOrDefault(), out var pages))
            {
                pages = 1;
            }

            if (pages == 1)
            {
                return result.Result.ToList();
            }

            var ret = new ConcurrentBag<Get_markets_region_id_orders_200_ok>();
            foreach (var getMarketsRegionIdOrders200Ok in result.Result)
            {
                ret.Add(getMarketsRegionIdOrders200Ok);
            }

            result.Headers.TryGetValue("Last-Modified", out var page1lastmodstring);
            var page1lastmod = page1lastmodstring?.FirstOrDefault();
            await Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync(Enumerable.Range(2, pages - 1),
                async pagenum =>
                {
                    _logger.LogInformation("GET Market ESI " + regionid + " TQ:" + tq + " Page " + pagenum);
                    var r = await esi._client.Get_markets_region_id_ordersAsync(null, null,
                        Order_type.All, pagenum, regionid,
                        null);
                    r.Headers.TryGetValue("Last-Modified", out var lastmod);
                    var lastmoddate = lastmod?.FirstOrDefault();
                    if (page1lastmod != lastmoddate)
                    {
                        throw new Exception("WrongMarketSnapShot");
                    }

                    foreach (var getMarketsRegionIdOrders200Ok in r.Result)
                    {
                        ret.Add(getMarketsRegionIdOrders200Ok);
                    }





                }, 30);

            return ret.ToList();
        }

        protected async Task Update(CancellationToken stoppingToken, bool tq = false)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<regions> regions;
                {
                    var db = tq ? (MarketDB) _service.GetService<TQMarketDB>() : _service.GetService<CNMarketDB>();
                    regions = await db.regions.AsNoTracking().OrderBy(p => p.regionid).ToListAsync();
                }
                var delay = Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                var downloadTasks = new Dictionary<long, Task<List<Get_markets_region_id_orders_200_ok>>>();
                Task<List<Get_markets_region_id_orders_200_ok>> tmptask = null;
                foreach (var region in regions)
                {
                    if (tmptask == null)
                    {
                        tmptask = GetESIOrders((int) region.regionid, tq);
                    }
                    else
                    {
                        tmptask = tmptask.ContinueWith(task => GetESIOrders((int) region.regionid, tq)).Unwrap();
                    }

                    downloadTasks[region.regionid] = tmptask;
                }


                foreach (var region in regions)
                {
                    try
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            continue;
                        }

                        _logger.LogInformation("开始下载订单: Region: " + region.regionid + $" TQ: {tq}");
                        var newordertask = downloadTasks[region.regionid];

                        MarketDB db = tq
                            ? (MarketDB) _service.GetService<TQMarketDB>()
                            : _service.GetService<CNMarketDB>();

                        var dttoday = MarketDB.ChinaTimeZone.AtStartOfDay(DateTime.Today.ToLocalDateTime().Date);
                        var dtnext = dttoday.Plus(Duration.FromDays(1));

                        var order = await db.current_market.Where(p => p.regionid == region.regionid)
                            .ToDictionaryAsync(p => p.orderid, p => p);
                        List<Get_markets_region_id_orders_200_ok> neworder;
                        try
                        {
                            neworder = await newordertask;
                        }
                        catch (Exception e)
                        {
                            _logger.LogInformation("下载订单失败: Region: " + region.regionid + $" TQ: {tq}  " + e.Message);
                            continue;
                        }

                        var allorders = neworder.GroupBy(p => p.Order_id).Select(p => p.First()).ToList();
                        _logger.LogInformation("完成下载订单: Region: " + region.regionid +
                                               $" TQ: {tq}; APIOrder {neworder.Count}/{allorders.Count}");
                        var alltypes = allorders.Select(p => p.Type_id).Distinct().ToList();
                        var todayhistsory = await db.market_markethistorybyday.Where(p =>
                            p.regionid == region.regionid && alltypes.Contains(p.typeid) &&
                            p.date == dttoday.Date).ToDictionaryAsync(p => p.typeid, p => p);


                        var realtimehistory = await db.evetypes.Where(p => alltypes.Contains(p.typeID)).Select(ip => ip
                                .market_realtimehistory.Where(p =>
                                    p.regionid == region.regionid && p.date >= dttoday.ToInstant() &&
                                    p.date < dtnext.ToInstant()).OrderByDescending(o => o.date).First())
                            .Where(p => p != null).ToDictionaryAsync(p => p.typeid, p => p);

                        foreach (var p in allorders)
                        {

                            current_market market;
                            if (!order.TryGetValue(p.Order_id, out market))
                            {
                                market = new current_market();
                                market.orderid = p.Order_id;
                                db.current_market.Add(market);
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
                            if (db.Entry(market).State == EntityState.Added ||
                                db.Entry(market).State == EntityState.Modified)
                            {
                                market.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                            }

                        }



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
                                hisbydate.min = Math.Max(hisbydate.min, price);



                            }

                            if (rt.Value.Any())
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
                        var delteids = order.AsParallel().Select(p => p.Key).Where(p => !allids.Contains(p));

                        _logger.LogInformation("开始保存订单: Region: " + region.regionid + $" TQ: {tq}");
                        await using var trans = await db.Database.BeginTransactionAsync();
                        await db.Database.ExecuteSqlInterpolatedAsync(
                            $"delete from current_market  where orderid = any ({delteids.ToList()}) and regionid = {region.regionid};");
                        try
                        {
                            await db.SaveChangesAsync();
                            await trans.CommitAsync();
                            _logger.LogInformation("完成保存订单: Region: " + region.regionid + $" TQ: {tq}");
                        }
                        finally
                        {

                            db.ChangeTracker.Entries()
                                .Where(e => e.Entity != null).ToList()
                                .ForEach(e => e.State = EntityState.Detached);

                        }



                    }
                    catch (Exception e)
                    {
                        _logger.LogError("出现错误" + e);
                    }

                }




                try
                {
                    await delay;
                }
                catch (Exception e)
                {

                }

            }
        }

    }
}