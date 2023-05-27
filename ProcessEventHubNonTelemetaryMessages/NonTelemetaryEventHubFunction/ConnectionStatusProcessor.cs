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


        public static bool ConnectionStatusProcessor(EventData eventData,ILogger logger)
        {
            bool result = true;
            string tablet_id = "";
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);
                if(dynObj.ContainsKey("tablet_id") && dynObj.tablet_id != null)
                {
                    tablet_id = Convert.ToString(dynObj.tablet_id);
                }
                string deviceID = Convert.ToString(dynObj.device_id);
                int connected = dynObj.connected;
                
                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                //db.UpdateBedConnectionStatus(deviceID, connected);
                db.UpdateBedConnectionStatusByTablet_device(deviceID, connected, tablet_id);

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
