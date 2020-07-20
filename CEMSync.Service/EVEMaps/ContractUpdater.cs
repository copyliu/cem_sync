using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

namespace CEMSync.Service.EVEMaps
{
    public class ContractUpdater:BackgroundService
    {

        static int[] AbyssItems=new int[]
        {
            
            47740,
            47408,
            47745,
            47749,
            47753,
            47757,
            47800,
            47804,
            47808,
            47812,
            47817,
            47820,
            47781,
            47785,
            47789,
            47793,
            47769,
            47773,
            47777,
            47836,
            47838,
            47840,
            47842,
            47844,
            47846,
            47824,
            47828,
            47832,
            48419,
            48423,
            48427,
            48431,
            48435,
            48439,
            47702,
            47732,
            47736,
            49730,
            49722,
            49726,
            49738,
            49734,
            52227,
            52230,
        };


        private readonly CNMarketDB db;
        private readonly ILogger<MarketUpdater> _logger;
        private readonly ESIClient esi;

        public ContractUpdater(CNMarketDB db, IHttpClientFactory httpClientFactory, ILogger<MarketUpdater> logger,
            IServiceProvider service)
        {
            this.db = db;
            _logger = logger;
            var http = httpClientFactory.CreateClient("CN");
            this.esi = service.GetService<ITypedHttpClientFactory<ESIClient>>().CreateClient(http);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);


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

        private async Task Update(CancellationToken token)
        {

            var regions = await db.regions.ToListAsync();
            foreach (var region in regions)
            {
               
                var contracts= this.esi.GetRegionContracts(region.regionid, token);
                var oldmodels = await db.contracts_info.Where(p => p.region_id == region.regionid).ToListAsync();
                
                foreach (var con in await contracts)
                {
                    bool newmodel = false;
                    var oldmodel = oldmodels.FirstOrDefault(p => p.ID == con.Contract_id);
                    if (oldmodel == null)
                    {
                        newmodel = true;
                        oldmodel=new contracts_info();
                        
                    }

                    oldmodel.region_id = (int)region.regionid;
                    oldmodel.ID = con.Contract_id;
                    oldmodel.buyout = (decimal?) con.Buyout;
                    oldmodel.date_issued = Instant.FromDateTimeOffset(con.Date_issued);
                    oldmodel.date_expired = Instant.FromDateTimeOffset(con.Date_expired);
                    oldmodel.issuer_corporation_id = con.Issuer_corporation_id;
                    oldmodel.issuer_id = con.Issuer_id;
                    oldmodel.type = con.Type.ToString();
                    oldmodel.title = con.Title;
                    oldmodel.price = (decimal?)con.Price;
                    oldmodel.start_location_id = con.Start_location_id;
                    oldmodel.end_location_id = con.End_location_id;
                    oldmodel.for_corporation = con.For_corporation;


                    if (newmodel)
                    {

                        var items = await this.esi.GetContractsItems(con.Contract_id, token);
                        foreach (var item in items.Where(p => AbyssItems.Contains(p.Type_id)
                        && p.Item_id>0))
                        {
                            var itemdata = await this.esi.GetItemDogma(item.Type_id, item.Item_id ?? 0, token);
                            var m = new contracts_itemdata()
                            {
                                type_id = item.Type_id,
                                created_by = itemdata.Created_by,
                                mutator_type_id = itemdata.Mutator_type_id,
                                source_type_id = itemdata.Source_type_id,
                                item_id = item.Item_id??0,
                                

                            };
                            foreach (var itemdataDogmaAttribute in itemdata.Dogma_attributes)
                            {
                                
                            }
                            // m.contracts_itemattr.
                            // db.contracts_itemdata.Add()


                            oldmodel.contracts_items.Add(new contracts_items()
                            {
                                type_id = item.Type_id,
                                is_blueprint_copy = item.Is_blueprint_copy,
                                is_included = item.Is_included,
                                item_id = item.Item_id??0,
                                material_efficiency = item.Material_efficiency,
                                time_efficiency = item.Time_efficiency,
                                runs = item.Runs,
                                quantity = item.Quantity,
                                ID=item.Record_id,

                            });
                            
                        }

                       


                        db.contracts_info.Add(oldmodel);
                    }


                }
                

            }


            throw new NotImplementedException();
        }
    }
}
