using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.Cosmos;

namespace InsertTelemetary
{
    public static class TelemataryProcessor
    {
        [FunctionName("TelemataryProcessor")]
        //Production
        //public static async Task Run([EventHubTrigger("smartmedic-telemetary-hub ", Connection = "smartmedic-telemetaryhub")] EventData[] events, ILogger log)
        //[EventHubTrigger("sm-staging-event-hub", Connection = "smartmedic-telemetaryhub")]
        //Development
        public static async Task Run([EventHubTrigger("%EventHubName_Azure%", Connection = "%smartmedictelemetary%")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();


            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                    string eventType = string.Empty;
                    eventType = Convert.ToString(dynObj.eventype);
                    log.LogInformation($"Event type: {eventType}");

                    string messageString = JsonConvert.SerializeObject(dynObj);
                    log.LogInformation("Input string {0}:",  messageString);


                    if (string.IsNullOrEmpty(eventType))
                    {
                        Cosmos cosmosdb = new Cosmos(log,dynObj, "");
                        cosmosdb.ExecuteDBTask(log, "");
                    } else
                    {
                        Cosmos cosmosdb = new Cosmos(log, dynObj, eventType);
                        cosmosdb.ExecuteDBTask(log, eventType);
                    }
                    
                    
                    
                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                    log.LogError("Input string {0}:", e.Message);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
