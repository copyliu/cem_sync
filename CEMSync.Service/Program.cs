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
using System.Reflection.Metadata.Ecma335;
using CEMSync.ESI;
using CEMSync.Model.KillBoard;
using EVEMarketSite.Model;
using NLog.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace CEMSync.Service
{
    class Program
    {
        static async Task Main(bool bootstrap = false, bool cnmarket = false, bool tqmarket = false)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                  
                    config.AddJsonFile("appsettings.json", false)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true,
                            true);
                    ;

                }).ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                  
                 

                    services.AddDbContext<EVEMapDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("EVEMapsDB")));

                    services.AddDbContext<CNMarketDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("MarketDB"),
                            builder => builder.UseNodaTime()));
                    services.AddDbContext<TQMarketDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("MarketDB_TQ"),
                            builder => builder.UseNodaTime()));
                    services.AddDbContext<CNKillboardDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("CNKillboardDB"),
                            builder => builder.UseNodaTime()));
                    services.AddDbContext<TQKillboardDB>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("TQKillboardDB"),
                            builder => builder.UseNodaTime()));
                    services.AddHttpClient<ZKBService>(client =>
                        {
                            client.Timeout = TimeSpan.FromSeconds(10);
                            client.DefaultRequestHeaders.Add("User-Agent", "CEVE-MARKET slack-copyliu CEMSync-Service");
                        })
                  
                        .ConfigurePrimaryHttpMessageHandler(provider =>
                        {
                            var handler = new HttpClientHandler();
                            
                            if (handler.SupportsAutomaticDecompression)
                            {
                                handler.AutomaticDecompression = DecompressionMethods.All;
                            }
                            return handler;
                        }
                        ).AddPolicyHandler(message =>
                        {
                            Random jitterer = new Random();
                            return

                                HttpPolicyExtensions
                                    .HandleTransientHttpError()
                                    .OrResult(msg => msg.StatusCode == (System.Net.HttpStatusCode) 420)
                                    .Or<TimeoutRejectedException>()
                                    .WaitAndRetryAsync(6,
                                        retryAttempt =>
                                            TimeSpan.FromSeconds(2 * (retryAttempt - 1)) +
                                            TimeSpan.FromMilliseconds(jitterer.Next(0, 500)));




                        }).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10));


                    services.AddHttpClient<ESIClient>(client =>
                        {
                            client.Timeout = TimeSpan.FromSeconds(10);
                            client.DefaultRequestHeaders.Add("User-Agent", "CEVE-MARKET slack-copyliu CEMSync-Service");
                        })
                        .ConfigurePrimaryHttpMessageHandler(provider =>
                        {
                            var handler = new HttpClientHandler();
                            if (handler.SupportsAutomaticDecompression)
                            {
                                handler.AutomaticDecompression = DecompressionMethods.All;
                            }
                            return handler;
                        }
                        )
                        .AddPolicyHandler(message =>
                        {
                            Random jitterer = new Random();

                            return
                               
                                
                                HttpPolicyExtensions
                                .HandleTransientHttpError()
                                .OrResult(msg => msg.StatusCode == (System.Net.HttpStatusCode) 420)
                                .Or<TimeoutRejectedException>()
                                .WaitAndRetryAsync(6,
                                    retryAttempt =>
                                        TimeSpan.FromSeconds(2*(retryAttempt-1)) +
                                        TimeSpan.FromMilliseconds(jitterer.Next(0, 500)))
                                ;
                        }).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10)); ;
                        
                    services.AddTransient<ESICNService>();
                    services.AddTransient<ESITQService>();
                    if (bootstrap)
                    {
                        services.AddHostedService<SdeUpdater>();

                    }
                    else if (cnmarket)
                    {
                        services.AddHostedService<CNMarketUpdater>();

                    }
                    else if (tqmarket)
                    {
                        services.AddHostedService<TQMarketUpdater>();

                    }
                    else
                    {
                        services.AddHostedService<CNMarketUpdater>();
                        services.AddHostedService<TQMarketUpdater>();
                        services.AddHostedService<TQKMLoader>();

                    }
                    // services.AddLogging(loggingBuilder =>
                    // {
                    //     loggingBuilder.ClearProviders();
                    //     loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                    //     loggingBuilder.AddConsole();
                    // });


                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                        loggingBuilder.AddNLog(
                            new NLogLoggingConfiguration(hostContext.Configuration.GetSection("NLog")));
                    });


                }).UseNLog();
            await builder.RunConsoleAsync();

        }
    }



}
