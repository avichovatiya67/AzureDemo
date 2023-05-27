using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
namespace BedSide_API_Functions
{
    public static class GetSignalRInfo
    {
        [FunctionName("negotiate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
           //  [SignalRConnectionInfo(HubName = "notifications")] SignalRConnectionInfo connectionInfo,
             IBinder binder,
           // [SignalRConnectionInfo(HubName = "notifications")] SignalRConnectionInfo connectionInfo,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

  

        //    if (req.Headers.ContainsKey("Authorization"))
            {
                var principal = "1234";// req.Headers["Authorization"].ToString();
                if (principal != null)
                {
                    var connectionInfo = await binder.BindAsync<SignalRConnectionInfo>(new SignalRConnectionInfoAttribute
                    {
                        HubName = "notifications",
                        UserId = principal
                        
                    }); ;
                    return new OkObjectResult(connectionInfo);
                }
            }

            return new UnauthorizedResult();
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            //return new OkObjectResult(responseMessage);
         //   return new OkObjectResult(connectionInfo);
            // return connectionInfo;
        }
    }
}
