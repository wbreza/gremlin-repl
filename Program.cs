using System;
using System.Threading.Tasks;
using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GremlinRepl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.dev.json", true)
                .AddEnvironmentVariables()
                .Build();

            var databaseName = configuration["Cosmos:DatabaseName"];
            var graphName = configuration["Cosmos:GraphName"];
            var hostname = configuration["Cosmos:Hostname"];
            var primaryKey = configuration["Cosmos:PrimaryKey"];
            var username = $"/dbs/{databaseName}/colls/{graphName}";

            var server = new GremlinServer(hostname, 443, true, username, primaryKey);
            using var client = new GremlinClient(server, new GraphSON2Reader(), new GraphSON2Writer(), GremlinClient.GraphSON2MimeType);
            while (true)
            {
                Console.WriteLine("Query:");
                var query = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(query))
                {
                    break;
                }

                try
                {
                    var results = await client.SubmitAsync<dynamic>(query);
                    var json = JsonConvert.SerializeObject(results, Formatting.Indented);
                    Console.WriteLine(json);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
        }
    }
}
