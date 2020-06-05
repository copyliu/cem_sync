using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CEMSync.ESI
{
    public class ESIClient
    {
        private readonly HttpClient http;

        public ESIClient(HttpClient http)
        {
            this.http = http;
        }
        public async Task<List<Get_sovereignty_structures_200_ok>> Get_sovereignty_structuresAsync(Datasource serenity, object o)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Get_sovereignty_campaigns_200_ok>> Get_sovereignty_campaignsAsync(Datasource serenity, object o)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Get_markets_region_id_orders_200_ok>> Get_markets_region_id_ordersAsync(int regionid,
            int page, CancellationToken token, DateTimeOffset? lastmod)
        {
            var url = $"markets/{regionid}/orders/?order_type=all&page={page}";
            var response = await this.http.GetAsync(url, token);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }

            if (response.Content.Headers.LastModified != lastmod)
            {
                throw new Exception("Wrong Date Time");
            }
            return JsonConvert.DeserializeObject<List<Get_markets_region_id_orders_200_ok>>(
                await response.Content.ReadAsStringAsync());
        }

        public async Task<List<int>> Get_universe_typesAsync(int page)
        {
            var url = $"universe/types/?page={page}";
            var response = await this.http.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }
            return JsonConvert.DeserializeObject<List<int>>(
                await response.Content.ReadAsStringAsync());
           

        }
        public async Task<HttpResponseMessage> GetAllTypesPages()
        {
            var url = $"universe/types/";
            var response = await this.http.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }

            return response;

        }
        public async Task<HttpResponseMessage> GetMarketOrdersHeaders(int regionid, CancellationToken token)
        {
            var url = $"markets/{regionid}/orders/?order_type=all";
            var response =  await this.http.SendAsync(new HttpRequestMessage(HttpMethod.Head, url), token);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }

            return response;

        }

        public async Task<List<int>> Get_markets_groupsAsync()
        {
            var url = $"markets/groups/";
            var response = await this.http.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }
            return JsonConvert.DeserializeObject<List<int>>(
                await response.Content.ReadAsStringAsync());

        }

        public async Task<Get_markets_groups_market_group_id_ok> Get_markets_groups_market_group_idAsync(string lang,int groupid)
        {
            var url = $"markets/groups/{groupid}/?language={lang}";
            var response = await this.http.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }
            return JsonConvert.DeserializeObject<Get_markets_groups_market_group_id_ok>(
                await response.Content.ReadAsStringAsync());
        }

        public async Task<Get_universe_types_type_id_ok> Get_universe_types_type_idAsync(string lang, int groupid)
        {
            var url = $"universe/types/{groupid}/?language={lang}";
            var response = await this.http.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }
            return JsonConvert.DeserializeObject<Get_universe_types_type_id_ok>(
                await response.Content.ReadAsStringAsync());
        }
    }
}