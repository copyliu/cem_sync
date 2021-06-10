using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
        public static async Task AsyncParallelForEach<T>(this IAsyncEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };
            if (scheduler != null)
                options.TaskScheduler = scheduler;

            var block = new ActionBlock<T>(body, options);

            await foreach (var item in source)
                block.Post(item);

            block.Complete();
            await block.Completion;
        }

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
