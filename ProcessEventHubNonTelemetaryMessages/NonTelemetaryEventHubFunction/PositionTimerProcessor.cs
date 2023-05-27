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
        public static bool PositionTimerProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                string bedID = Convert.ToString(dynObj.bedID);
                string episodeID = Convert.ToString(dynObj.episodeID);
                string positionTimerInterval = Convert.ToString(dynObj.position_timer); 
                

                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                db.UpdateBedInfo(bedID,"",positionTimerInterval,"","");
                db.UpdatePositionTimerinPositionStatus(bedID, positionTimerInterval, episodeID);
                // db.InsertTurnEventForBed(bedID, episodeID, positionData);
                // db.InsertActiveEpisodeForBed(bedID);
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
