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


        public static bool GatewayDeviceProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                string tabletID = Convert.ToString(dynObj.tablet_id);
                string hospitalID = Convert.ToString(dynObj.hospital_id);
                double unixDateTime = dynObj.device_unix_time;
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(unixDateTime).ToLocalTime();
                string createdAt = dtDateTime.ToString();

                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                db.RegisterGatewayDevice(tabletID, hospitalID, unixDateTime, createdAt);
            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError("ERROR while processing gateway register event: {0}", ex.Message);
            }
            return result;
        }


    }
}
