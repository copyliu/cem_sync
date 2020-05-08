using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CEMSync.ESI
{

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
             return w.ToList();
         }

         public async Task<List<Get_sovereignty_campaigns_200_ok>> getSovCampignsAsync()
         {
             var w = await _client.Get_sovereignty_campaignsAsync(Datasource.Serenity, null);
             return w.ToList();
        }

    }
}
