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


        public static bool ConfigDeviceProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            int connectionStatus = 1;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                if (dynObj.ContainsKey("connection_status") && dynObj.connection_status != null)
                {
                    connectionStatus = Convert.ToInt32(dynObj.connection_status);
                }
                string tabletID = Convert.ToString(dynObj.tablet_id);
                string deviceID = Convert.ToString(dynObj.deviceId);
                string telemetryInterval = Convert.ToString(dynObj.telemetryInterval);
                string position_timer = Convert.ToString(dynObj.positionTimer);
                string subCPU1 = Convert.ToString(dynObj.subCPU1);
                string subCPU2 = Convert.ToString(dynObj.subCPU1);
                string subCPU3 = Convert.ToString(dynObj.subCPU1);
                string subCPU4 = Convert.ToString(dynObj.subCPU1);
                string display = Convert.ToString(dynObj.display);

                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                db.UpdateDeviceTwinProperty(deviceID, telemetryInterval, position_timer, subCPU1, subCPU2, subCPU3, subCPU4, display, connectionStatus, tabletID);
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
