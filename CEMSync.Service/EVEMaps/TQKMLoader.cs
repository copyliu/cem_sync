using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;
using CEMSync.Model.KillBoard;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CEMSync.Service.EVEMaps
{
    public class TQKMLoader : BackgroundService
    {
        private readonly TQKillboardDB _db;
        private readonly ZKBService _zkb;
        private readonly ILogger<TQKMLoader> _logger;

        public TQKMLoader(TQKillboardDB db, ZKBService zkb, ILogger<TQKMLoader> logger)
        {
            _db = db;
            _zkb = zkb;
            _logger = logger;
        }
       
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = Task.Delay(TimeSpan.FromSeconds(1),stoppingToken);
                try
                {
                    var model=await _zkb.GetZkb(stoppingToken);
                    if (model != null)
                    {
                        _logger.LogInformation("New KM: "+model.killID);
                        _db.killboard_waiting_api.Add(model);
                        await _db.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
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
