using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry.Extensions.Logging;

namespace CEMSync.Service
{

    public class RetryHandler : DelegatingHandler
    {
        // Strongly consider limiting the number of retries - "retry forever" is
        // probably not the most user friendly way you could respond to "the
        // network cable got pulled out."
        private const int MaxRetries = 5;

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken token)
        {
            HttpResponseMessage result = null;
            for (var i = 0; i < MaxRetries; i++)
            {
                try
                {
                    result = await base.SendAsync(request, token).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode) return result;
                    else
                    {
                        if ((int) result.StatusCode >= 500 && (int) result.StatusCode < 599)
                        {
                            throw new Exception("Server Error");
                        }
                        else if (result.StatusCode == (HttpStatusCode) 420)
                        {
                            await Task.Delay(
                                TimeSpan.FromSeconds(Convert.ToInt32(result.Headers
                                    .GetValues("x-esi-error-limit-remain").FirstOrDefault())), token);

                            throw new Exception("Server Error");
                        }

                        return result;
                    }
                }
                catch (TaskCanceledException e)
                {
                    if (e.CancellationToken == token)
                    {
                        throw;
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1 + 1.5 * i), token);
                    }
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(1 + 1.5 * i), token);
                }
            }

            if (result == null)
            {
                throw new Exception("Server Error");

            }
            return result;
        }
    }
    public static class HttpClientExt
    {
        public static async Task<HttpResponseMessage> SendAndRetryAsync(this HttpClient http, HttpRequestMessage request,
            HttpCompletionOption completionOption, CancellationToken token)
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    var result = await http.SendAsync(request, completionOption, token);
                    if (result.IsSuccessStatusCode) return result;
                    else
                    {
                        if ((int) result.StatusCode >= 500 && (int) result.StatusCode < 599)
                        {
                            throw new Exception("Server Error");
                        }
                        else if (result.StatusCode == (HttpStatusCode) 420)
                        {
                            await Task.Delay(
                                TimeSpan.FromSeconds(Convert.ToInt32(result.Headers
                                    .GetValues("x-esi-error-limit-remain").FirstOrDefault())), token);

                            throw new Exception("Server Error");
                        }

                        return result;
                    }
                }
                catch (TaskCanceledException e)
                {
                    if (e.CancellationToken == token)
                    {
                        throw;
                    }
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromSeconds(1 + 1.5 * i), token);
                }
            }

            throw new Exception("Server Error");

        }
    }

    public class ForceHttp2Handler : DelegatingHandler
    {
        public ForceHttp2Handler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // request.Version = HttpVersion.Version20;
            return base.SendAsync(request, cancellationToken);
        }
    }
    public static class Utils
   {
       public static IHostBuilder UseSentry(this IHostBuilder builder) =>
           builder.ConfigureLogging((context, logging) =>
           {
               IConfigurationSection section = context.Configuration.GetSection("Sentry");

               logging.Services.Configure<SentryLoggingOptions>(section);
               logging.AddSentry((c) =>
               {
                   var version = Assembly
                       .GetEntryAssembly()
                       .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                       .InformationalVersion;
                   c.Release = $"my-application@{version}";
               });
           });
        public static string esi_url = "https://esi.evepc.163.com/";
       public static HttpClient httpClient=new HttpClient();
       public static int ConvertRange(this Get_markets_region_id_orders_200_okRange range)
       {
          
           switch (range)
           {
                case  Get_markets_region_id_orders_200_okRange.Region:
                    return 65535;
                case Get_markets_region_id_orders_200_okRange.Solarsystem:
                    return 32767;
                case Get_markets_region_id_orders_200_okRange.Station:
                    return 0;
                case Get_markets_region_id_orders_200_okRange._1:
                    return 1;
                case Get_markets_region_id_orders_200_okRange._2:
                    return 2;
                case Get_markets_region_id_orders_200_okRange._3:
                    return 3;
                case Get_markets_region_id_orders_200_okRange._4:
                    return 4;
                case Get_markets_region_id_orders_200_okRange._5:
                    return 5;
                case Get_markets_region_id_orders_200_okRange._10:
                    return 10;
                case Get_markets_region_id_orders_200_okRange._20:
                    return 20;
                case Get_markets_region_id_orders_200_okRange._30:
                    return 30;
                case Get_markets_region_id_orders_200_okRange._40:
                    return 40;

               default: return 65535;
           }
       }
    }
}
