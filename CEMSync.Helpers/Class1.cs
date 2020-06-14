using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CEMSync.Helpers
{
    public static class Helpers
    {

        public static void AddOrUpdateAppSetting<T>(string key, T value, string envnname)
        {
            try
            {
                string filePath;
                if (string.IsNullOrEmpty(envnname))
                {
                    filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                }
                else
                {
                     filePath = Path.Combine(AppContext.BaseDirectory, $"appsettings.{envnname}.json");
                }
               
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

               
                
                jsonObj[key] = value; // if no sectionpath just set the value
               
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, output);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing app settings "+e);
            }
        }
    }
}
