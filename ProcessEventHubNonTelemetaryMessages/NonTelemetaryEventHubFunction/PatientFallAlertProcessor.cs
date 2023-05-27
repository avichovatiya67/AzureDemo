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
        public static bool PatientFallAlertProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);
                string bedID = Convert.ToString(dynObj.bedID);
                string episodeID = Convert.ToString(dynObj.episodeID);
                string datetime = Convert.ToString(dynObj.datetime);
                string unixdatetime = Convert.ToString(dynObj.unixdatetime);
                double unix_date_time = 0;
                if (!string.IsNullOrEmpty(unixdatetime))
                {
                    unix_date_time = Convert.ToDouble(dynObj.unixdatetime);
                }
                NonTelemetaryDB db = new NonTelemetaryDB(logger);
		        db.InsertFallAlertInfo(bedID, episodeID, datetime, unix_date_time);
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
