using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessEventHubNonTelemetaryMessages
{
    public static partial class MessageProcessor
    {


        public static bool DischargeProcessor(EventData eventData,ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                string bedID = Convert.ToString(dynObj.bedID);
                string bed_status = Convert.ToString(dynObj.bed_status);

                string messageversion = eventData.Properties["messageversion"].ToString();
                double unixdatetime = Convert.ToDouble(dynObj.unixDateTime);
                string messagetype = eventData.Properties["messagetype"].ToString();
                logger.LogInformation("Message version {0}", messageversion);
                logger.LogInformation("unixdatetime {0}", unixdatetime);
                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                if (!string.IsNullOrEmpty(messageversion) && Convert.ToDouble(messageversion) >= 1.1)                                
                {
                    db.InsertDeviceTelemetryData(dynObj, messagetype);
                }
                //db.UpdateBedInfo(bedID, bed_status, "", "", "");
                db.SetDischargeStateForBed(bedID, bed_status, "", "", "");
                db.SetEpisodesInActiveForBed(bedID);
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
