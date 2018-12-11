using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace FM.Redis.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            var builder = new ConfigurationBuilder();
            builder.Add(new JsonConfigurationSource()
            {
                Path = "appsettings.json"
            });

            var redisconfig = builder.Build().GetSection("RedisConfig").Get<RedisConfig>();
            
            var client = new RedisClient(redisconfig);
            var result = client.GetDatabase().StringGet("Sam_Last_Asset_500337066_106");
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
