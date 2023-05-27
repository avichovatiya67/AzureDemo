using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;
namespace ProcessEventHubNonTelemetaryMessages
{
    public static partial class MessageProcessor
    {


        public static bool GatewayConnectionStatusProcessor(EventData eventData,ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);
               
                string tabletID = Convert.ToString(dynObj.tablet_id);
                int connected = dynObj.connected;
                
                NonTelemetaryDB db = new NonTelemetaryDB(logger);                   
                db.UpdateGatewayConnectionStatus(tabletID, connected);

            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError(ex.Message);
            }
            return result;
        }

       
    }
}
