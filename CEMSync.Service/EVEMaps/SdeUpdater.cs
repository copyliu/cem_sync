using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;
using CEMSync.Model.KillBoard;
using EVEMarketSite.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CEMSync.Service.EVEMaps
{
    public class SdeUpdater : BackgroundService
    {
        private readonly CNMarketDB _cndb;
        private readonly TQMarketDB _tqdb;
        private readonly ESICNService _esi;
        private readonly ESITQService _tqesi;
        private readonly ILogger<SdeUpdater> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        private static int MAX_THREAD = 30;
        public SdeUpdater(CNMarketDB cndb,TQMarketDB tqdb, ESICNService esi, ESITQService tqesi, ILogger<SdeUpdater> logger, IHostApplicationLifetime lifetime)
        {
            _cndb = cndb;
            _tqdb = tqdb;
            _esi = esi;
            _tqesi = tqesi;
            _logger = logger;
            _lifetime = lifetime;
        }


        public async Task<List<int>> GetAllTypeAsync(bool tq = false)
        {
            ESIService esi = tq ? (ESIService)_tqesi : _esi;
            ConcurrentBag<int> result = new ConcurrentBag<int>();

            var page1 = await esi._client.Get_universe_typesAsync(null,null,null);
            if (!page1.Headers.TryGetValue("X-Pages", out var xPages) ||
                !int.TryParse(xPages.FirstOrDefault(), out var pages))
            {
                pages = 1;
            }

            if (pages == 1)
            {
                return page1.Result.ToList();
            }
            foreach (var i in page1.Result)
            {
                result.Add(i);
            }

            await Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync( Enumerable.Range(2, pages - 1),async page =>
            {
                var thispage = await esi._client.Get_universe_typesAsync(null, null, page);

              
                foreach (var i in thispage.Result)
                {
                    result.Add(i);
                }
            }, MAX_THREAD);
            return result.Distinct().ToList();
        }

        public async Task UpdateSde(bool tq=false)
        {
            MarketDB db = tq ? (MarketDB) _tqdb : _cndb;
            ESIService esi = tq ? (ESIService) _tqesi : _esi;
            var oldgroups = await EntityFrameworkQueryableExtensions.ToListAsync(db.marketgroup);

            var allmarketgroupstask =  esi._client.Get_markets_groupsAsync(null, null);
     
            var alltypetask =  GetAllTypeAsync();

          
             var newmodels = new ConcurrentBag<EVEMarketSite.Model.marketgroup>();
            await Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync((await allmarketgroupstask).Result.Distinct(),async i =>
            {
                while (true)
                {
                    try
                    {
                        var groupinfo_en = esi._client.Get_markets_groups_market_group_idAsync(AcceptLanguage.EnUs, null,null,Language.EnUs, i);
                        var groupinfo_cn = esi._client.Get_markets_groups_market_group_idAsync(AcceptLanguage.Zh, null, null, Language.Zh, i);

                        await Task.WhenAll(groupinfo_cn, groupinfo_en);
                        _logger.LogDebug($"GetItemMarketGroupInfoV1Async {i}");
                        var oldmodel = oldgroups.FirstOrDefault(p => p.marketGroupID == i);
                        if (oldmodel == null)
                        {
                            oldmodel = new marketgroup();
                            oldmodel.marketGroupID = i;
                            newmodels.Add(oldmodel);
                        }

                        oldmodel.marketGroupName_en = groupinfo_en.Result.Result.Name;
                        oldmodel.marketGroupName = groupinfo_cn.Result.Result.Name;
                        oldmodel.description = groupinfo_cn.Result.Result.Description;
                        oldmodel.description_en = groupinfo_en.Result.Result.Description;
                        oldmodel.parentGroupID = groupinfo_en.Result.Result.Parent_group_id;
                        return;
                    }
                    catch (Exception e)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        _logger.LogWarning(e + "");

                    }
                }
            }, MAX_THREAD);
            db.marketgroup.AddRange(newmodels);
            _logger.LogDebug($"Save GetMarketItemGroupInfo");
            await db.SaveChangesAsync();
            _logger.LogDebug($"Save GetMarketItemGroupInfo OK");

            var oldtypes = await EntityFrameworkQueryableExtensions.ToListAsync(db.evetypes);
            var newtypes = new ConcurrentBag<evetypes>();
            await Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync(await alltypetask,async i =>
            {
               
                    try
                    {
                        var groupinfo_en =
                            esi._client.Get_universe_types_type_idAsync(AcceptLanguage.EnUs, null, null, Language.EnUs,
                                i);
                        var groupinfo_cn = 
                            esi._client.Get_universe_types_type_idAsync(AcceptLanguage.Zh, null, null, Language.Zh,
                            i);
                        await Task.WhenAll(groupinfo_cn, groupinfo_en);
                        _logger.LogDebug($"GetTypeInfoV3Async {i}");
                        var oldmodel = oldtypes.FirstOrDefault(p => p.typeID == i);
                        if (oldmodel == null)
                        {
                            oldmodel = new evetypes();
                            oldmodel.typeID = i;
                            newtypes.Add(oldmodel);
                        }

                        oldmodel.typeName_en = groupinfo_en.Result.Result.Name;
                        oldmodel.typeName = groupinfo_cn.Result.Result.Name;
                        oldmodel.description = groupinfo_cn.Result.Result.Description;
                        oldmodel.description_en = groupinfo_en.Result.Result.Description;
                        oldmodel.groupID = groupinfo_en.Result.Result.Group_id;
                      

                       
                    }
                    catch (Exception e)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        _logger.LogWarning(e + "");

                    }

               

            }, MAX_THREAD);
            db.evetypes.AddRange(newtypes);
            await db.SaveChangesAsync();
            _logger.LogInformation("完成!");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await UpdateSde(true);
                await UpdateSde(false);
              
            }
            catch (Exception e)
            {
                _logger.LogError(e, "出现错误" + e);
            }

            _lifetime.StopApplication();

        }
    }
}