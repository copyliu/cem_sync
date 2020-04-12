using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;
using CEMSync.Model.EVEMapsDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CEMSync.Service.EVEMaps
{
    public class StructUpdateService : BackgroundService
    {
        private readonly EVEMapDB _db;
        private readonly ILogger<StructUpdateService> _logger;
        private readonly ESIService _esi;

        public StructUpdateService(EVEMapDB db, ILogger<StructUpdateService> logger,ESIService esi)
        {
            _db = db;
            _logger = logger;
            _esi = esi;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = Task.Delay(TimeSpan.FromMinutes(1),stoppingToken);
                try
                {
                    var r=await _esi.getSovereigntyStructuresAsync();
                    _logger.LogInformation(r.Count+"");
                    var allstructs = await _db.maps_struct.Where(p => p.valid == true).ToListAsync();
                    _logger.LogInformation(allstructs.Count + "");
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e,"出现错误");
                }



                await delay;
            }
        }
    }

    public class CampaignUpdateService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("4156");
            await Task.Delay(TimeSpan.FromSeconds(5));
            Console.WriteLine("123");
        }
    }

    public class AllianceUpdateService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
    public class PosUpdateService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
    public class SovUpdateService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }

    public class MapStatUpdateService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
