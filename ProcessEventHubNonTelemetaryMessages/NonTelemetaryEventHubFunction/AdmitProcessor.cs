using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;
using System.Dynamic;

namespace ProcessEventHubNonTelemetaryMessages
{
    public static partial class MessageProcessor
    {


        public static bool AdmitProcessor(EventData eventData,ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);
               
                string bedID = Convert.ToString(dynObj.bedID);
                string episodeID = Convert.ToString(dynObj.episodeID);
                
                string bed_status = Convert.ToString(dynObj.bed_status);
                string icu_id = Convert.ToString(dynObj.icu_id);
                string bed_Name = Convert.ToString(dynObj.bed_Name);
                string position_timer = Convert.ToString(dynObj.position_timer);
                string tablet_id = null;

                if (dynObj.ContainsKey("tablet_id"))
                {
                    tablet_id = Convert.ToString(dynObj.tablet_id);
                }
                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                db.DeleteDuplicateICUAndBed(bed_Name,icu_id,bedID);
                // db.UpdateBedInfo(bedID, bed_status, position_timer, bed_Name, icu_id);
                db.insertActiveBed(bedID, bed_status, position_timer, bed_Name, icu_id, tablet_id);
                db.InsertActiveEpisodeForBed(bedID, episodeID);
                //db.disposeClient();
            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError("ERROR while Admitting: {0}", ex.Message);
            }
            return result;
        }

       
    }
}
