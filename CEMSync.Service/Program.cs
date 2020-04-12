using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CEMSync.Model.EVEMapsDB;
using CEMSync.Service.EVEMaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using CEMSync.ESI;
using CEMSync.Model.KillBoard;
using EVEMarketSite.Model;
using Polly;
using Polly.Extensions.Http;

namespace CEMSync.Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    Console.WriteLine(hostingContext.HostingEnvironment.EnvironmentName);
                    config.AddJsonFile("appsettings.json", false)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true,
                            true);
                    ;

                }).ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                   

                    services.AddDbContextPool<EVEMapDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("EVEMapsDB")));

                    services.AddDbContext<CNMarketDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("MarketDB"), builder => builder.UseNodaTime()));
                    services.AddDbContext<TQMarketDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("MarketDB_TQ"), builder => builder.UseNodaTime()));
                    services.AddDbContext<CNKillboardDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("CNKillboardDB"), builder => builder.UseNodaTime()));
                    services.AddDbContext<TQKillboardDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("TQKillboardDB"), builder => builder.UseNodaTime()));



                    if (args.Contains("--bootstrap"))
                    {


                    }
                    else
                    {
                        services.AddSingleton<IHostedService, MarketUpdater>();
                        services.AddSingleton<IHostedService, TQKMLoader>();
                    }

                    services.AddTransient<ESICNService>();
                    services.AddTransient<ESITQService>();
                    services.AddHttpClient<ESIClient>()
                        .ConfigurePrimaryHttpMessageHandler(provider =>
                        
                            new HttpClientHandler()
                            {
                                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,

                                MaxConnectionsPerServer = 50
                            }
                        )
                        .ConfigureHttpMessageHandlerBuilder(handlerBuilder =>
                    
                        new HttpClientHandler()
                        {
                            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,

                            MaxConnectionsPerServer = 50
                        }
                    ).AddPolicyHandler(message =>
                    {
                        Random jitterer = new Random();
                        return HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .OrResult(msg => msg.StatusCode == (System.Net.HttpStatusCode) 420)
                            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))+TimeSpan.FromMilliseconds(jitterer.Next(0,500)));
                    });
                    services.AddHttpClient<ZKBService>()
                        .ConfigurePrimaryHttpMessageHandler(provider =>

                            new HttpClientHandler()
                            {
                                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,

                                MaxConnectionsPerServer = 50
                            }
                        ).ConfigureHttpMessageHandlerBuilder(handlerBuilder => 
                    
                        new HttpClientHandler()
                        {
                            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,

                            MaxConnectionsPerServer = 50
                        }
                    ).AddPolicyHandler(message =>
                    {
                        Random jitterer = new Random();
                        return HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 500)));
                    });

                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                        loggingBuilder.AddNLog(new NLogLoggingConfiguration(hostContext.Configuration.GetSection("NLog")));
                    });

                });
            
            await builder.RunConsoleAsync();

        }
    }
}
