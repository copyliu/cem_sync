using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CEMSync.Service.Model;

namespace CEMSync.Service
{
    public class ZKBService
    {
        private readonly HttpClient _httpClient;



        public ZKBService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CEMSync.Model.KillBoard.killboard_waiting_api> GetZkb(CancellationToken ctsToken)
        {
            var req = _httpClient.GetStreamAsync(
                "http://redisq.zkillboard.com/listen.php?queueID=ceve-market.org&ttw=5");


            var crestresult = await
                System.Text.Json.JsonSerializer.DeserializeAsync<RedisQ>(
                    await req, null, ctsToken);
            if (crestresult?.package != null)
            {
                return new CEMSync.Model.KillBoard.killboard_waiting_api()
                
                    {killID = crestresult.package.killID, hash = crestresult.package.zkb?.hash}
                ;
            }

            return null;

        }
    }
}
