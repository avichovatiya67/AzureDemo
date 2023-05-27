using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Stryker.SmartMedic.Models;
using Microsoft.Azure.Cosmos;

namespace BedSide_API_Functions
{
    public static class GetBedListForICU
    {
        [FunctionName("GetBedListForICU")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",  Route = "getBedListForICU")]HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                string icu_id = req.Query["icu_id"];
                if (string.IsNullOrWhiteSpace(icu_id))
                {
                    string responseMessage = "This HTTP triggered function executed successfully. Pass a icu id in the query string for bed list";
                    return new OkObjectResult(responseMessage);
                }

                CosmosDB cosmosdb = new CosmosDB(log);
                List<BedData> bedList = await cosmosdb.getBedListForHospital(icu_id);

                var resultObject = bedList.Select(n => new { bed_name = n.bed_name, id = n.id, icu_id = n.icu_id }).ToArray();

                return new OkObjectResult(resultObject);
            }
            catch (CosmosException ce)
            {
                log.LogError("GetBedListForICU {0}",ce.Message);
                return new OkObjectResult(new Array[0]);
            }
            catch (Exception ex)
            {
                log.LogError("GetBedListForICU {0}", ex.Message);
                return new OkObjectResult(new Array[0]);
            }
        }
    }
}
