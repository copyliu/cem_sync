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
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using CEMSync.ESI;
using CEMSync.Model.KillBoard;
using EVEMarketSite.Model;
using NLog.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Sentry.Extensions.Logging;

namespace CEMSync.Service
{
    class Program
    {
        static async Task Main(bool bootstrap = false, bool cnmarket = false, bool tqmarket = false,bool cncontract=false)
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

                    services.Configure<MyConfig>(hostContext.Configuration);

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
                    var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(30);







                    ;
                    var asyncTimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5 * 60);
                    // Random jitterer = new Random();
                    var waitAndRetryAsync = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .OrResult(msg => msg.StatusCode == (System.Net.HttpStatusCode) 420)
                        .Or<TimeoutRejectedException>()
                        .Or<SocketException>()
                      
                        .WaitAndRetryAsync(6,
                            (retryAttempt,result,context) =>
                            {
                                if (result.Result.StatusCode == (HttpStatusCode) 420)
                                {
                                    var val = result.Result.Headers.GetValues("x-esi-error-limit-reset").FirstOrDefault();
                                    if (int.TryParse(val, out int sec))
                                    {
                                        return TimeSpan.FromSeconds(sec+1);
                                    }

                                }
                                return TimeSpan.FromSeconds(1.5 * (retryAttempt - 1));
                            }, (result, span, arg3, arg4) =>
                            {
                                result.Result.Dispose();
                                return Task.CompletedTask;
                            });


                    services.AddHttpClient<ZKBService>(client =>
                        {
                            client.Timeout = TimeSpan.FromSeconds(10);
                            client.DefaultRequestHeaders.Add("User-Agent", "CEVE-MARKET slack-copyliu CEMSync-Service");
                        })
                        .ConfigurePrimaryHttpMessageHandler(provider => new SocketsHttpHandler
                        {
                            AutomaticDecompression = DecompressionMethods.All,
                            // ConnectTimeout = TimeSpan.FromSeconds(5),
                            // ResponseDrainTimeout = TimeSpan.FromSeconds(10),


                        })
                        .AddPolicyHandler(asyncTimeoutPolicy)
                        .AddPolicyHandler(message => waitAndRetryAsync)
                        .AddPolicyHandler(timeoutPolicy);
                    services.AddHttpClient<ESIClient>("CN", client =>
                        {
                            client.BaseAddress = new Uri("https://esi.evepc.163.com/latest/");
                            client.Timeout = TimeSpan.FromSeconds(20);
                            client.DefaultRequestHeaders.Add("User-Agent", "CEVE-MARKET slack-copyliu CEMSync-Service");

                        })
                        .ConfigurePrimaryHttpMessageHandler(provider => new SocketsHttpHandler
                        {
                            AutomaticDecompression = DecompressionMethods.All,
                            // ConnectTimeout = TimeSpan.FromSeconds(5),
                            // ResponseDrainTimeout = TimeSpan.FromSeconds(10),


                        })
                        .AddPolicyHandler(asyncTimeoutPolicy)
                        .AddPolicyHandler(message => waitAndRetryAsync)
                        .AddPolicyHandler(timeoutPolicy);
                        services.AddHttpClient<ESIClient>("TQ", client =>
                            {
                                client.BaseAddress = new Uri("https://esi.evetech.net/latest/");
                                client.Timeout = TimeSpan.FromSeconds(30);
                                client.DefaultRequestHeaders.Add("User-Agent",
                                    "CEVE-MARKET slack-copyliu CEMSync-Service");
                                // client.DefaultRequestVersion=new Version(2,0);
                                
                            })
                            .ConfigurePrimaryHttpMessageHandler(provider => new SocketsHttpHandler
                            {
                                AutomaticDecompression = DecompressionMethods.All,
                                // ConnectTimeout = TimeSpan.FromSeconds(5),
                                // ResponseDrainTimeout = TimeSpan.FromSeconds(10),


                            })
                        .AddPolicyHandler(asyncTimeoutPolicy)
                        .AddPolicyHandler(message => waitAndRetryAsync)
                        .AddPolicyHandler(timeoutPolicy);

                    if (bootstrap)
                    {
                        services.AddHostedService<SdeUpdater>();

                    }
                    else if (cncontract)
                    {
                        services.AddHostedService<ContractUpdater>();
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


                }).UseNLog()
                .UseSentry();
            await builder.RunConsoleAsync();

        }
      
    }



}
