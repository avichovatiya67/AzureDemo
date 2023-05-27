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

        public static  bool WeightProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);
                
                string bedID = Convert.ToString(dynObj.bedID);
                string episodeID = Convert.ToString(dynObj.episodeID);
                float weightData = (float)Convert.ToDouble(dynObj.weight);
                string unixdatetime = Convert.ToString(dynObj.unixdatetime);
                double unix_date_time = 0;
                if (!string.IsNullOrEmpty(unixdatetime))
                {
                    unix_date_time = Convert.ToDouble(dynObj.unixdatetime);
                }
                string messagecurrentversion = eventData.Properties["messageversion"].ToString();
                logger.LogInformation("unix_date_time {0}", unix_date_time);
                logger.LogInformation("message version {0}", messagecurrentversion);
                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                if (!string.IsNullOrEmpty(messagecurrentversion) && Convert.ToDouble(messagecurrentversion) >= 1.1)
                {
                    string eventmessagetype = eventData.Properties["messagetype"].ToString();
                    dynamic additionalWeightData = new
                    {

                        devicetype = Convert.ToString(dynObj.devicetype),
                        deviceversion = Convert.ToString(dynObj.deviceversion),
                        hospitalcode = Convert.ToString(dynObj.hospitalcode),
                        messagetype = eventmessagetype,
                        messageversion = messagecurrentversion,
                        date_time = Convert.ToString(dynObj.date_time)
                    };
                    dynamic sensorData = setWeightSensorData(dynObj);
                    db.InsertGetWeightWithSensorForBed(bedID, episodeID, weightData, unix_date_time, sensorData, additionalWeightData);
                } else
                {
                    db.InsertGetWeightEventForBed(bedID, episodeID, weightData, unix_date_time);
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.LogError("ERROR while processing weight event: {0}", ex.Message);
            }
            return result;
        }

        public static object setWeightSensorData(dynamic dynObj)
        {

            Dictionary<string, object> sensorData = new Dictionary<string, object>();

            if (dynObj.ContainsKey("wp1"))
            {
                sensorData.Add("wp1", new
                {
                    BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp1.bedAngle) * 1000) / 1000,
                    BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp1.bedpitch) * 1000) / 1000,
                    BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp1.bedroll) * 1000) / 1000,
                    WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc1) * 1000) / 1000,
                    WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc2) * 1000) / 1000,
                    WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc3) * 1000) / 1000,
                    WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc4) * 1000) / 1000
                });
            }
            if (dynObj.ContainsKey("wp2"))
            {
                sensorData.Add("wp2", new {
                    BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp2.bedAngle) * 1000) / 1000,
                    BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp2.bedpitch) * 1000) / 1000,
                    BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp2.bedroll) * 1000) / 1000,
                    WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc1) * 1000) / 1000,
                    WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc2) * 1000) / 1000,
                    WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc3) * 1000) / 1000,
                    WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc4) * 1000) / 1000
                });
            }
            if (dynObj.ContainsKey("wp3"))
            {
                sensorData.Add("wp3", new
                {
                    BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp3.bedAngle) * 1000) / 1000,
                    BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp3.bedpitch) * 1000) / 1000,
                    BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp3.bedroll) * 1000) / 1000,
                    WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc1) * 1000) / 1000,
                    WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc2) * 1000) / 1000,
                    WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc3) * 1000) / 1000,
                    WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc4) * 1000) / 1000
                });
            }
            if (dynObj.ContainsKey("wp4"))
            {
                sensorData.Add("wp4", new
                {
                    BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp4.bedAngle) * 1000) / 1000,
                    BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp4.bedpitch) * 1000) / 1000,
                    BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp4.bedroll) * 1000) / 1000,
                    WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc1) * 1000) / 1000,
                    WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc2) * 1000) / 1000,
                    WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc3) * 1000) / 1000,
                    WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc4) * 1000) / 1000
                });
            }

            return sensorData;
        }
    }
}
