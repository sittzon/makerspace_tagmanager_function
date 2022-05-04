using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
//using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Extensions.Configuration;
using System.IO;
//using System.Text.Json;
using System.Linq;
using Newtonsoft.Json;

namespace makerspace_tagmanager_function
{
    public static class Tag
    {    
        private static readonly string[] TagsHex = new[]
        {
            "CD:D2:2F:DB", "67:12:8C:C3"
        };

        private static readonly long[] TagsDec = new[]
        {
            20521047219, 10318140195
        };

        public class TagEntity : TableEntity
        {
            public bool MachineAccess { get; set; }
        }

        [FunctionName("Tag")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            //log.LogInformation($"PK={table.PartitionKey}, RK={poco.RowKey}");

            String stringId = req.Query["id"];
            if (string.IsNullOrEmpty(stringId)) {
                return new OkObjectResult(false);
            }

            long id = 0;
            var okParse = long.TryParse(stringId, out id);
            if (!okParse) {
                return new OkObjectResult(false);
            }
            /*
            foreach(long i in TagsDec) {
                if (i == id) {
                    return new OkObjectResult(true);
                }
            }
            */
            

            //string returnString = "";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("StorageAccount");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("Tag");

            //var to = TableOperation.Retrieve<TagEntity>("id", id.ToString());
            //var tableResult = table.ExecuteAsync(to);

			var query = new TableQuery<TagEntity>();
            //var q = query.Where(x => x.RowKey == id.ToString()).AsQueryable<TagEntity>();
            var result = table.ExecuteQuery<TagEntity>(query);
            
            /*if (tableResult.Result.HttpStatusCode.CompareTo(200) != 0) {
                return new OkObjectResult(false);
            }*/

            var r = result.Where(x => x.RowKey == id.ToString() && x.MachineAccess == true);

            if (!r.Any()) {
                return new OkObjectResult(false);
            }

            return new OkObjectResult(true);
                        
            //return new OkObjectResult(tableResult.Result.Result);


        }
    }
}
