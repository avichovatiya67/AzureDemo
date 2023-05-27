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
using Stryker.SmartMedic.Models;

namespace BedSide_API_Functions
{
    public static class GetICUListForHospital
    {
        [FunctionName("GetICUListForHospital")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getICUListForHospital")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                string hospital_code = req.Query["hospital_code"];
                if (string.IsNullOrWhiteSpace(hospital_code))
                {
                    string responseMessage = "This HTTP triggered function executed successfully. Pass a hospital_code in the query string ICU list";
                    return new OkObjectResult(responseMessage);
                }

                CosmosDB cosmosdb = new CosmosDB(log);
                List<ICU> icuList = await cosmosdb.getICUListForHospital(hospital_code);

                var resultObject = icuList.Select(n => new { icu_name = n.icu_name, id = n.id }).ToArray();

                return new OkObjectResult(resultObject);
            }
            catch (Exception ex)
            {
                log.LogError("GetICUListForHospital {0}", ex.Message);
                return new OkObjectResult(new Array[0]);
            }
        }

    }
}
