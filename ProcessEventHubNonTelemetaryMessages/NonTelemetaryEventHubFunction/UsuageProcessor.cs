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

        public static bool UsuageProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                string serial_number = Convert.ToString(dynObj.serial_number);
                string eventType = Convert.ToString(dynObj.event_type);
                string datetime = Convert.ToString(dynObj.date_time);
                logger.LogInformation("Usuage Pro {0} event", eventType);
                logger.LogInformation("Usuage Pro {0} serial_number", serial_number);
                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                if(!string.IsNullOrWhiteSpace(serial_number))
                    db.InsertUsuageEventForBed(serial_number, datetime, eventType);
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
