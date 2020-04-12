using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CEMSync.ESI
{

    public class ESITQService : ESIService
    {
        private readonly HttpClient _http;


        public ESITQService(ESIClient client, HttpClient http) : base(client)
        {
            _http = http;
            _client.BaseUrl = "https://esi.evetech.net/latest/";
        }

      

    }

    public class ESICNService : ESIService
    {


        public ESICNService(ESIClient client) : base(client)
        {
            _client.BaseUrl = "https://esi.evepc.163.com/latest";
        }


    }
    public abstract class ESIService
    {
        public readonly ESIClient _client;
      

        public ESIService(ESIClient client)
        {
            _client = client;
            
        }

         public async Task<List<Get_sovereignty_structures_200_ok>> getSovereigntyStructuresAsync()
         {
             var w = await _client.Get_sovereignty_structuresAsync(Datasource.Serenity, null);
             return w.Result.ToList();
         }

         public async Task<List<Get_sovereignty_campaigns_200_ok>> getSovCampignsAsync()
         {
             var w = await _client.Get_sovereignty_campaignsAsync(Datasource.Serenity, null);
             return w.Result.ToList();
        }

    }
}
