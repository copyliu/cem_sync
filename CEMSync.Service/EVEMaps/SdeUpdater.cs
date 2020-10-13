using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;
using CEMSync.Model.KillBoard;
using EVEMarketSite.Model;
using ICSharpCode.SharpZipLib.BZip2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace CEMSync.Service.EVEMaps
{
    public class SdeUpdater : BackgroundService
    {
        private readonly CNMarketDB _cndb;
        private readonly TQMarketDB _tqdb;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _service;
        private readonly ESIClient _esi;
        private readonly ESIClient _tqesi;
        private readonly ILogger<SdeUpdater> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        private static int MAX_THREAD = 30;
        public SdeUpdater(CNMarketDB cndb,TQMarketDB tqdb, IHttpClientFactory httpClientFactory, IServiceProvider service, ILogger<SdeUpdater> logger, IHostApplicationLifetime lifetime)
        {
            _cndb = cndb;
            _tqdb = tqdb;
            _httpClientFactory = httpClientFactory;
            _service = service;
            var http1 = httpClientFactory.CreateClient("CN");
            this._esi = service.GetService<ITypedHttpClientFactory<ESIClient>>().CreateClient(http1);
            var http2 = httpClientFactory.CreateClient("TQ");
            this._tqesi = service.GetService<ITypedHttpClientFactory<ESIClient>>().CreateClient(http2);

            _logger = logger;
            _lifetime = lifetime;
        }


        public async Task<List<int>> GetAllTypeAsync(bool tq = false)
        {
            ESIClient esi = tq ? (ESIClient)_tqesi : _esi;
            ConcurrentBag<int> result = new ConcurrentBag<int>();
            var pageheaders = await esi.GetAllTypesPages();


            if (!pageheaders.Headers.TryGetValues("x-pages", out var xPages) ||
                !int.TryParse(xPages.FirstOrDefault(), out var pages))
            {
                pages = 1;
            }


            await Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync( Enumerable.Range(1, pages),async page =>
            {
                var thispage = await esi.Get_universe_typesAsync( page);

              
                foreach (var i in thispage)
                {
                    result.Add(i);
                }
            }, MAX_THREAD);
            return result.Distinct().ToList();
        }

        public async Task UpdateSde(bool tq=false)
        {

            MarketDB db = tq ? (MarketDB) _tqdb : _cndb;

            ESIClient esi = tq ? (ESIClient) _tqesi : _esi;
            var oldgroups = await EntityFrameworkQueryableExtensions.ToListAsync(db.marketgroup);

            var attr = esi.GetAllAttrs();
            var allmarketgroupstask = esi.Get_markets_groupsAsync();


            var alltypetask =  GetAllTypeAsync(tq);

          
             var newmodels = new ConcurrentBag<EVEMarketSite.Model.marketgroup>();
             var oldattr = await EntityFrameworkQueryableExtensions.ToListAsync(db.dogma_attributes);
             foreach (var m in await attr)
             {
                 var model = oldattr.FirstOrDefault(p => p.attribute_id == m.Attribute_id);
                 if (model == null)
                 {
                     model = new dogma_attributes();
                     db.dogma_attributes.Add(model);
                 }

                 model.attribute_id = m.Attribute_id;
                 model.name = m.Name;
                 model.description = m.Description;
                 model.default_value = m.Default_value;
                 model.display_name = m.Display_name;
                 model.high_is_good = m.High_is_good;
                 model.icon_id = m.Icon_id;
                 model.published = m.Published;
                 model.stackable = m.Stackable;
                 model.unit_id = m.Unit_id;
             }


            await Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync((await allmarketgroupstask).Distinct(),async i =>
            {
                while (true)
                {
                    try
                    {
                        var groupinfo_en = esi.Get_markets_groups_market_group_idAsync("en-us",i);
                        var groupinfo_cn = esi.Get_markets_groups_market_group_idAsync("zh",i);

                        await Task.WhenAll(groupinfo_cn, groupinfo_en);
                        _logger.LogDebug($"GetItemMarketGroupInfoV1Async {i}");
                        var oldmodel = oldgroups.FirstOrDefault(p => p.marketGroupID == i);
                        if (oldmodel == null)
                        {
                            oldmodel = new marketgroup();
                            oldmodel.marketGroupID = i;
                            newmodels.Add(oldmodel);
                        }

                        oldmodel.marketGroupName_en = groupinfo_en.Result.Name;
                        oldmodel.marketGroupName = groupinfo_cn.Result.Name;
                        oldmodel.description = groupinfo_cn.Result.Description;
                        oldmodel.description_en = groupinfo_en.Result.Description;
                        oldmodel.parentGroupID = groupinfo_en.Result.Parent_group_id;
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

       
            await db.SaveChangesAsync();
            var oldtypes = await EntityFrameworkQueryableExtensions.ToListAsync(db.evetypes);
            var oldtypeattr = await EntityFrameworkQueryableExtensions.ToListAsync(db.type_attributes);
            var newtypes = new ConcurrentBag<evetypes>();
            var newtypeattrs=new ConcurrentBag<type_attributes>();
            await Dasync.Collections.ParallelForEachExtensions.ParallelForEachAsync(await alltypetask,async i =>
            {
               
                    try
                    {
                        var groupinfo_en =
                            esi.Get_universe_types_type_idAsync("en-us",
                                i);
                        var groupinfo_cn = 
                            esi.Get_universe_types_type_idAsync("zh", i);
                        await Task.WhenAll(groupinfo_cn, groupinfo_en);
                        _logger.LogDebug($"GetTypeInfoV3Async {i}");
                        var oldmodel = oldtypes.FirstOrDefault(p => p.typeID == i);
                        if (oldmodel == null)
                        {
                            oldmodel = new evetypes();
                            oldmodel.typeID = i;
                            newtypes.Add(oldmodel);
                        }

                        oldmodel.typeName_en = groupinfo_en.Result.Name;
                        oldmodel.typeName = groupinfo_cn.Result.Name;
                        oldmodel.description = groupinfo_cn.Result.Description;
                        oldmodel.description_en = groupinfo_en.Result.Description;
                        oldmodel.groupID = groupinfo_en.Result.Group_id;
                      
                    oldmodel.attributes.Clear();
                    if (groupinfo_cn.Result.Dogma_attributes?.Count > 0)
                    {
                        foreach (var resultDogmaAttribute in groupinfo_cn.Result.Dogma_attributes)
                        {
                            newtypeattrs.Add(new type_attributes()
                            {
                                attribute_id = resultDogmaAttribute.Attribute_id,
                                value = resultDogmaAttribute.Value,
                                type_id = i
                            });
                        }
                    }

                  



                    }
                    catch (Exception e)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        _logger.LogWarning(e + "");

                    }

               

            }, MAX_THREAD);
           
            db.evetypes.AddRange(newtypes);
            await db.SaveChangesAsync();
            db.type_attributes.RemoveRange(db.type_attributes);
            db.AddRange(newtypeattrs);
            await db.SaveChangesAsync();

            var http = _httpClientFactory.CreateClient("CN");
            db.InvMetaTypes.RemoveRange(db.InvMetaTypes);

            await using var bz2file1 = await http.GetStreamAsync("https://www.fuzzwork.co.uk/dump/latest/invMetaTypes.csv.bz2");
            await using var fs = new MemoryStream();
            BZip2.Decompress(bz2file1,fs,false);
            fs.Seek(0, SeekOrigin.Begin);
            using var textreader = new StreamReader(fs);
            await textreader.ReadLineAsync(); //ignore 1st
            var r=await textreader.ReadLineAsync(); 
            while (r!=null)
            {
                var imp = r.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                var newmodel=new invMetaTypes();
                newmodel.typeID = int.Parse(imp[0]);
                if (imp[1] == "None")
                {
                    newmodel.parentTypeID = null;
                }
                else
                {
                    newmodel.parentTypeID = int.Parse(imp[1]);
                }
                if (imp[2] == "None")
                {
                    newmodel.metaGroupID = null;
                }
                else
                {
                    newmodel.metaGroupID = int.Parse(imp[2]);
                }
                db.InvMetaTypes.Add(newmodel);

                r = await textreader.ReadLineAsync(); //ignore 1st
            }

            await db.SaveChangesAsync();
            

            _logger.LogInformation("完成!");
        }




        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                await Task.WhenAll(UpdateSde(false));//,UpdateSde(true));

            }
            catch (Exception e)
            {
                _logger.LogError(e, "出现错误" + e);
            }

            _lifetime.StopApplication();

        }
    }
}