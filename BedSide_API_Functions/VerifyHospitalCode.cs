using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BedSide_API_Functions
{
    public static class VerifyHospitalCode
    {
        [FunctionName("VerifyHospitalCode")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "VerifyHospitalCode")] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                string responseMessage = string.Empty;
                string hospital_code = req.Query["hospital_code"];
                if (string.IsNullOrWhiteSpace(hospital_code))
                {
                    responseMessage = "Error:This HTTP triggered function executed successfully. Pass a hospital_code in the query string";

                }
                else
                {

                    CosmosDB cosmosdb = new CosmosDB(log);
                    bool flag = await cosmosdb.IsHospitalCodeExists(hospital_code);
                    if (flag == false)
                    {
                        responseMessage = "Error: No hospital code exists !";
                    }
                    else
                    {
                        responseMessage = "Success";
                    }

                }
                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex)
            {
                log.LogError("VerifyHospitalCode {0}",ex.Message);
                return new OkObjectResult(new Array[0]);
            }

        }
    }
}
