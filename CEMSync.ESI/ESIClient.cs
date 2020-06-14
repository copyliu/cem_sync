using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CEMSync.ESI
{
    public class ESIClient
    {
        public IOptions<MyConfig> _config { get; }
        SemaphoreSlim lockSemaphoreSlim=new SemaphoreSlim(1,1);
        private class TokenInfo
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }

      

        }

       string ESI_AUTHURL => IsTq ? "https://login.eveonline.com/v2" : "https://login.evepc.163.com/v2";
        string JWKS_URL => IsTq ? "https://login.eveonline.com/oauth/jwks" : "https://login.evepc.163.com/oauth/jwks";

        private string RefreshToken => IsTq ? _config.Value.tqtoken : _config.Value.cntoken;
        private string ClientID => IsTq ? _config.Value.tqappid : _config.Value.cnappid;
        private TokenInfo Token
        {
            get;
            set;
        }

        public bool IsTq
        {
            get => _isTq;
            set
            {
                EVEJWKS = null;
                TokenExpired=DateTimeOffset.MinValue;
                Token = null;
                _isTq = value;

            }
        }

        public bool IsTokenOk => !string.IsNullOrEmpty(Token?.access_token) && TokenExpired > DateTimeOffset.Now;
        private Microsoft.IdentityModel.Tokens.JsonWebKeySet EVEJWKS = null;

        private DateTimeOffset TokenExpired;

        private readonly HttpClient http;

        private readonly IHostEnvironment _env;
        private bool _isTq;

        async Task UpdateToken(bool force=false)
        {
            await this.lockSemaphoreSlim.WaitAsync();
            try
            {
                if (!IsTokenOk || force)
                {
                    if (string.IsNullOrEmpty(RefreshToken) || string.IsNullOrEmpty(ClientID))
                    {
                        throw new Exception("未设置APPID");
                    }
                    if (EVEJWKS == null)
                    {
                        var jwks = await this.http.GetStringAsync(JWKS_URL);
                        EVEJWKS = JsonWebKeySet.Create(jwks);
                    }

                    var args = new List<KeyValuePair<string, string>>();
                    args.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
                    args.Add(new KeyValuePair<string, string>("refresh_token", RefreshToken));
                    args.Add(new KeyValuePair<string, string>("client_id", ClientID));

                    var res = await this.http.PostAsync(ESI_AUTHURL+ "/oauth/token", new FormUrlEncodedContent(args));
                    if (res.IsSuccessStatusCode)
                    {
                        var tokeninfo = await System.Text.Json.JsonSerializer.DeserializeAsync<TokenInfo>(await res.Content.ReadAsStreamAsync());
                        if (!string.IsNullOrEmpty(tokeninfo.access_token))
                        {
                            this.Token = tokeninfo;
                            if (IsTq)
                            {

                                Helpers.Helpers.AddOrUpdateAppSetting("tqtoken", this.Token.refresh_token, _env.EnvironmentName);
                                this._config.Value.tqtoken = this.Token.refresh_token;
                            }
                            else
                            {

                                Helpers.Helpers.AddOrUpdateAppSetting("cntoken", this.Token.refresh_token, _env.EnvironmentName);
                            }


                            this.TokenExpired = DateTimeOffset.Now.AddSeconds(tokeninfo.expires_in);
                        }

                    }
                    else
                    {
                        var s = await res.Content.ReadAsStringAsync();
                        throw new Exception(s);
                    }
                }
            }
            finally
            {
                this.lockSemaphoreSlim.Release();
            }
    
            
        }


        public ESIClient(HttpClient http, IOptions<MyConfig> config, IHostEnvironment env)
        {
            _config = config;
            this.http = http;
           
            _env = env;
        }
        public async Task<List<Get_sovereignty_structures_200_ok>> Get_sovereignty_structuresAsync(Datasource serenity, object o)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Get_sovereignty_campaigns_200_ok>> Get_sovereignty_campaignsAsync(Datasource serenity, object o)
        {
            throw new System.NotImplementedException();
        }


        public async Task<Get_universe_structures_structure_id_ok> GetCitidal(long id)
        {
            await UpdateToken();
            var url = "universe/structures/";
            var req=new HttpRequestMessage(HttpMethod.Get, $"{url}{id}");
            req.Headers.Authorization=new AuthenticationHeaderValue(this.Token.token_type,this.Token.access_token);
            var response=await this.http.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Get_universe_structures_structure_id_ok>(
                    await response.Content.ReadAsStringAsync());
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return null;
                }
                else
                {
                    throw new Exception("Status Code:" + response.StatusCode);
                }
                
            }



        }

        public async Task<long[]> GetAllCitidalIds(CancellationToken token)
        {
            var url = "universe/structures/";
            var response = await this.http.GetAsync(url, token);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }

            var citidalids = JsonConvert.DeserializeObject<long[]>(await response.Content.ReadAsStringAsync());
            return citidalids;
        }


        public async Task<List<Get_dogma_attributes_attribute_id_ok>> GetAllAttrs()
        {
            var url = "dogma/attributes/";
            var response = await this.http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }

            var attrs=JsonConvert.DeserializeObject<int[]>(await response.Content.ReadAsStringAsync());
            var tasks=attrs.Select(async p =>
                {
                    var res = await this.http.GetStringAsync($"{url}{p}/");
                    return JsonConvert.DeserializeObject<Get_dogma_attributes_attribute_id_ok>(res);


                }
            ).ToList();
            await Task.WhenAll(tasks);
            return tasks.Select(p => p.Result).ToList();
        }

        public async Task<HttpResponseMessage> GetMarketstructureOrdersHeaders(long structure, CancellationToken token)
        {
            try
            {
                await UpdateToken();
            }
            catch 
            {
               return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                
            }
            var url = $"markets/structures/{structure}/?order_type=all";
            var req = new HttpRequestMessage(HttpMethod.Head, url);
            req.Headers.Authorization = new AuthenticationHeaderValue(this.Token.token_type, this.Token.access_token);
            var response = await this.http.SendAsync(req, token);
            

            return response;

        }

        public async Task<List<Get_markets_structures_structure_id_200_ok>> Get_markets_structure_ordersAsync(long structure,
            int page, CancellationToken token, DateTimeOffset? lastmod)
        {
            try
            {
                await UpdateToken();
            }
            catch
            {
                return new List<Get_markets_structures_structure_id_200_ok>();

            }
            var url = $"markets/structures/{structure}/?order_type=all&page={page}";
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue(this.Token.token_type, this.Token.access_token);
            var response = await this.http.SendAsync(req, token);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Status Code:" + response.StatusCode);
            }

            if (response.Content.Headers.LastModified != lastmod)
            {
                throw new Exception("Wrong Date Time");
            }
            return JsonConvert.DeserializeObject<List<Get_markets_structures_structure_id_200_ok>>(
                await response.Content.ReadAsStringAsync());
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
