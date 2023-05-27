using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProcessEventHubNonTelemetaryMessages
{
    public static partial class MessageProcessor
    {

        public static bool ErrorMessageProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                string bedID = Convert.ToString(dynObj.bedID);
                string errordescription = Convert.ToString(dynObj.errordescription);
                int bed_status = Convert.ToInt16(dynObj.bed_status);

                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                db.UpdateBedStatus(bedID, bed_status, errordescription);  //Error status on bed
                db.InsertErrorMessage(bedID, errordescription);
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
