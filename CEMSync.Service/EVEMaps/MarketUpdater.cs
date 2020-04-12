using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public class MarketUpdater : BackgroundService
    {
        private readonly ESICNService _esi;
        private readonly ESITQService _tqesi;
        private readonly ILogger<MarketUpdater> _logger;
        private readonly IServiceProvider _service;

        public MarketUpdater(ESICNService esi, ESITQService tqesi, ILogger<MarketUpdater> logger, IServiceProvider service)
        {
            _esi = esi;
            _tqesi = tqesi;
            _logger = logger;
            _service = service;
        }

        async Task<List<Get_markets_region_id_orders_200_ok>> GetESIOrders(int regionid,bool tq)
        {
            ESIService esi;
            esi = tq ?  (ESIService) this._tqesi : this._esi;


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
            await Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync(Enumerable.Range(2, pages),
                async pagenum =>
                {

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

        protected  async Task Update(CancellationToken stoppingToken,bool tq=false)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<regions> regions;
                {
                     var db = _service.GetService<CNMarketDB>();

                    regions = await db.regions.AsNoTracking().ToListAsync();
                }
                var delay = Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                try
                {
                    foreach (var region in regions)
                    {
                        var newordertask =  GetESIOrders((int)region.regionid,tq);
                        
                        MarketDB db;
                        
                        db = tq ? (MarketDB) _service.GetService<TQMarketDB>() : _service.GetService<CNMarketDB>();
                        var zone = DateTimeZoneProviders.Tzdb["Asia/Shanghai"];
                        var dttoday = zone.AtStartOfDay(DateTime.Today.ToLocalDateTime().Date);
                        var dtnext = dttoday.Plus(Duration.FromDays(1));

                        var order = await db.current_market.Where(p => p.regionid == region.regionid)
                            .ToDictionaryAsync(p => p.orderid, p => p);

                        var neworder = await newordertask;
                        foreach (var p in  neworder)
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

                        }

                        var alltypes = neworder.Select(p => p.Type_id).Distinct().ToList();
                        var todayhistsory = await db.market_markethistorybyday.Where(p =>
                            p.regionid == region.regionid && alltypes.Contains(p.typeid) &&
                            p.date == dttoday.Date).ToDictionaryAsync(p => p.typeid, p => p);


                        var realtimehistory = await db.evetypes.Where(p => alltypes.Contains(p.typeID) ).Select(ip => ip
                                .market_realtimehistory.Where(p =>
                                    p.regionid == region.regionid && p.date >= dttoday.ToInstant() &&
                                    p.date < dtnext.ToInstant()).OrderByDescending(o => o.date).First())
                            .Where(p => p != null).ToDictionaryAsync(p => p.typeid, p => p);

                        foreach (var keyValuePair in order.Where(p =>
                            !neworder.Select(o => o.Order_id).Contains(p.Key)))
                        {
                            db.current_market.Remove(keyValuePair.Value);
                        }

                        foreach (var rt in neworder.GroupBy(p => p.Type_id))
                        {
                            if (rt.Any(p => p.Is_buy_order == false))
                            {
                                market_markethistorybyday hisbydate;
                                var price = rt.Where(p => !p.Is_buy_order).Min(o => o.Price);
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

                            if (rt.Any())
                            {
                                market_realtimehistory realtime;
                                realtime = new market_realtimehistory
                                {
                                    regionid = region.regionid,
                                    typeid = rt.Key,
                                    date = Instant.FromDateTimeOffset(DateTimeOffset.Now),
                                    buy = rt.Any(p => p.Is_buy_order)
                                        ? rt.Where(p => p.Is_buy_order).Max(p => p.Price)
                                        : 0,
                                    sell = rt.Any(p => !p.Is_buy_order)
                                        ? rt.Where(p => !p.Is_buy_order).Min(p => p.Price)
                                        : 0,
                                    buyvol = rt.Any(p => p.Is_buy_order)
                                        ? rt.Where(p => p.Is_buy_order).Sum(p => (long)p.Volume_remain)
                                        : 0,
                                    sellvol = rt.Any(p => !p.Is_buy_order)
                                        ? rt.Where(p => !p.Is_buy_order).Sum(p => (long)p.Volume_remain)
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

                        await db.SaveChangesAsync();

                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "出现错误" + e);
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
  
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cn = Update(stoppingToken, false);
            var tq= Update(stoppingToken, true);
            // await tq;
            await Task.WhenAll(cn, tq);
        }
    }
}