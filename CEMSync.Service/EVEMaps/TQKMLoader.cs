using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;
using CEMSync.Model.KillBoard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CEMSync.Service.EVEMaps
{
    public class TQKMLoader : BackgroundService
    {
        private readonly ZKBService _zkb;
        private readonly ILogger<TQKMLoader> _logger;
        private readonly IServiceProvider _service;

        public TQKMLoader(ZKBService zkb, ILogger<TQKMLoader> logger, IServiceProvider service)
        {
            _zkb = zkb;
            _logger = logger;
            _service = service;
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
                        TQKillboardDB _db = _service.GetService<TQKillboardDB>();
                        _logger.LogInformation("New KM: "+model.killID);
                        _db.killboard_waiting_api.Add(model);
                        try
                        {
                            await _db.SaveChangesAsync();

                        }
                        finally
                        {
                            _db.ChangeTracker.Entries()
                                .Where(e => e.Entity != null).ToList()
                                .ForEach(e => e.State = EntityState.Detached);
                        }
                       
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
