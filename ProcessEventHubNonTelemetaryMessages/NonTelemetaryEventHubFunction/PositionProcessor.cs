using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;

namespace ProcessEventHubNonTelemetaryMessages
{
    public static partial class MessageProcessor
    {


        public static bool PositionProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);

                string bedID = Convert.ToString(dynObj.bedID);
                string episodeID = Convert.ToString(dynObj.episodeID);
                string positionData = Convert.ToString(dynObj.position);
                int position_status= Convert.ToInt16(dynObj.position_status);
                string position_timer = Convert.ToString(dynObj.position_timer);
                string date_time = string.Empty;
                try
                {
                    date_time = Convert.ToString(dynObj.date_time);
                   
                }
                catch (RuntimeBinderException)
                {
                    //  date_time doesn't exist
                }    

                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                db.InsertTurnEventForBed(bedID, episodeID, positionData, position_status, position_timer,date_time);
                // db.InsertActiveEpisodeForBed(bedID);
            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError("ERROR while processing position event: {0}", ex.Message);
            }
            return result;
        }

        public static bool HasProperty(dynamic obj, string name)
        {
            Type objType = obj.GetType();

            if (objType == typeof(ExpandoObject))
            {
                return ((IDictionary<string, object>)obj).ContainsKey(name);
            }

            return objType.GetProperty(name) != null;
        }
    }
}
