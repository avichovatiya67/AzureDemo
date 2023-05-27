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


        public static bool RegisterDeviceProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                string tabletID = Convert.ToString(dynObj.tablet_id);
                string deviceID = Convert.ToString(dynObj.device_id);
                int connectionStatus = 0;

                if (dynObj.ContainsKey("connection_status") && dynObj.connection_status != null)
                {
                    connectionStatus = Convert.ToInt32(dynObj.connection_status);
                }

                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                db.RegisterDevice(deviceID, tabletID, connectionStatus);
            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError("ERROR while processing device register: {0}", ex.Message);
            }
            return result;
        }


    }
}
