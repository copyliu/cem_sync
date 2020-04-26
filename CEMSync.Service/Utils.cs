using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.ESI;

namespace CEMSync.Service
{
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
