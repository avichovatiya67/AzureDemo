using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.Dynamic;

namespace ProcessEventHubNonTelemetaryMessages
{
    public static partial class MessageProcessor
    {

        public static bool RemoveBedMessageProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                string bedID = Convert.ToString(dynObj.device_id);
                string tabletID = Convert.ToString(dynObj.tablet_id);
                logger.LogInformation("bedID {0} and tablet_id {1}", bedID, tabletID);
                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                db.removeBedStatus(bedID, tabletID);
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
