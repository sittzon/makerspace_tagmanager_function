using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace makerspace_tagmanager_function
{
    public static class Tag
    {
        public class TagEntity : TableEntity
        {
            public bool MachineAccess { get; set; }
        }

        [FunctionName("Tag")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var today = DateTime.Now;
            log.LogInformation(today.ToShortDateString()+" "+today.TimeOfDay+": Tag function processed a request.");

            String stringId = req.Query["id"];
            if (string.IsNullOrEmpty(stringId)) {
                log.LogError("Id not found in request");
                return new OkObjectResult(false);
            }

            long id = 0;
            var okParse = long.TryParse(stringId, out id);
            if (!okParse) {
                log.LogError("Id parse error");
                return new OkObjectResult(false);
            }

            var connectionString = Environment.GetEnvironmentVariable("StorageAccountConnectionString");
            if (string.IsNullOrEmpty(connectionString)) {
                log.LogError("Environment variable StorageAccountConnectionString not found");
                return new OkObjectResult(false);
            }

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("Tag");

			var query = new TableQuery<TagEntity>();
            var result = table.ExecuteQuery<TagEntity>(query);

            var r = result.Where(x => x.RowKey == id.ToString() && x.MachineAccess == true);

            if (!r.Any()) {
                log.LogError("Id " +id+" not found with MachineAccess = true");
                return new OkObjectResult(false);
            }

            log.LogInformation("Id " +id+" found with MachineAccess = true");
            return new OkObjectResult(true);
        }
    }
}
