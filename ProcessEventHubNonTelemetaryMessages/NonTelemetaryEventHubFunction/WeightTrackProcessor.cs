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
        public static bool WeightTrackProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);
		        string bedID = Convert.ToString(dynObj.bedID);
                string episodeID = Convert.ToString(dynObj.episode_id);
		        double weightDifference = Convert.ToDouble(dynObj.weightDifference);
                string datetime = Convert.ToString(dynObj.datetime);
		        NonTelemetaryDB db = new NonTelemetaryDB(logger);
		        db.insertWeightTrackForBed(bedID, episodeID, weightDifference, datetime);
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
