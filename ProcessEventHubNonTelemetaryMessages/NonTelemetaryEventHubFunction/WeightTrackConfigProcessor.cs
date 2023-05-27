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
        public static bool WeightTrackConfigProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                logger.LogInformation("In Weight Track");
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                string bedID = Convert.ToString(dynObj.bedID);
                string episodeID = Convert.ToString(dynObj.episode_id);
                int is_track_on = Convert.ToInt16(dynObj.is_track_on);
                int angle = Convert.ToInt16(dynObj.angle);
                string datetime = Convert.ToString(dynObj.datetime);
                double baseWeight = Convert.ToDouble(dynObj.baseWeight);
                string messageType = eventData.Properties["messagetype"].ToString();
                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                if (messageType == "WeightTrackLimit-Message") {
                    double upperLimit = (!string.IsNullOrWhiteSpace(Convert.ToString(dynObj.upperLimit))) ? Convert.ToDouble(dynObj.upperLimit) : 0;
		            double lowerLimit = (!string.IsNullOrWhiteSpace(Convert.ToString(dynObj.lowerLimit))) ? Convert.ToDouble(dynObj.lowerLimit) : 0;
                    db.updateWeightTrackConfigForBed(bedID, episodeID, is_track_on, angle, baseWeight, upperLimit, lowerLimit, datetime);
		        } else
                {
                    db.setWeightTrackConfigForBed(bedID, episodeID, is_track_on, angle, baseWeight, datetime);
                }
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
