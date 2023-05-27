using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class Plank {

        public decimal BedAngle;
        public decimal BedPitch;
        public decimal BedRoll;
        public decimal WC1;
        public decimal WC2;
        public decimal WC3;
        public decimal WC4;

    }
    
    public class TelemetryDataPoint
    {

        public dynamic dynObj = null;
        public string SequenceNumber = string.Empty;
        public string telemetaryversion = string.Empty;
        public string messageversion = string.Empty;
        public string devicetype = string.Empty;
        public string deviceversion = string.Empty;
        public string hospitalcode = string.Empty;
        public string episodeID = string.Empty;
        public string bedID = string.Empty;
         public string messagetype = string.Empty;
        public string isValidSensorDataPresent = string.Empty;
        public string patient_position = string.Empty;
        public string exitAlarm = string.Empty;
        public string weight_on_demand = string.Empty;
        public string datetime = string.Empty;
        public dynamic unixdatetime = 0;
        public string id = string.Empty;
        public decimal soft_tare = 0;
        public Plank wp1;
        public Plank wp2;
        public Plank wp3;
        public Plank wp4;
        
        public TelemetryDataPoint()
        {
        }
        public TelemetryDataPoint(dynamic dynObj)
        {
             id = Guid.NewGuid().ToString();
            SequenceNumber = Convert.ToString(dynObj.SequenceNumber);
            telemetaryversion = Convert.ToString(dynObj.telemetaryversion);
            messageversion = Convert.ToString(dynObj.messageversion);
            devicetype = Convert.ToString(dynObj.devicetype);
             deviceversion = Convert.ToString(dynObj.deviceversion);
             bedID = Convert.ToString(dynObj.bedID);
            hospitalcode = Convert.ToString(dynObj.hospitalcode);
            episodeID = Convert.ToString(dynObj.episodeID);
             isValidSensorDataPresent = Convert.ToString(dynObj.isValidSensorDataPresent);
             patient_position = Convert.ToString(dynObj.patient_position);
             exitAlarm = Convert.ToString(dynObj.exitAlarm);
             weight_on_demand = Convert.ToString(dynObj.weight_on_demand);
             datetime = Convert.ToString(dynObj.datetime);
             unixdatetime = Convert.ToInt64(dynObj.unixdatetime);
            messagetype = Convert.ToString(dynObj.messagetype);
             soft_tare = Convert.ToDecimal(dynObj.soft_tare);
            wp1 = new Plank()
            {
                BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp1.bedAngle) * 1000) / 1000,
                BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp1.bedpitch) * 1000) / 1000,
                BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp1.bedroll) * 1000) / 1000,
                WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc1) * 1000) / 1000,
                WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc2) * 1000) / 1000,
                WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc3) * 1000) / 1000,
                WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc4) * 1000) / 1000
            };
            wp2 = new Plank()
            {

                BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp2.bedAngle) * 1000) / 1000,
                BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp2.bedpitch) * 1000) / 1000,
                BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp2.bedroll) * 1000) / 1000,
                WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc1) * 1000) / 1000,
                WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc2) * 1000) / 1000,
                WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc3) * 1000) / 1000,
                WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc4) * 1000) / 1000
            };
            wp3 = new Plank()
            {

                BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp3.bedAngle) * 1000) / 1000,
                BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp3.bedpitch) * 1000) / 1000,
                BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp3.bedroll) * 1000) / 1000,
                WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc1) * 1000) / 1000,
                WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc2) * 1000) / 1000,
                WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc3) * 1000) / 1000,
                WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc4) * 1000) / 1000
            };
            wp4 = new Plank()
            {
                BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp4.bedAngle) * 1000) / 1000,
                BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp4.bedpitch) * 1000) / 1000,
                BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp4.bedroll) * 1000) / 1000,
                WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc1) * 1000) / 1000,
                WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc2) * 1000) / 1000,
                WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc3) * 1000) / 1000,
                WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc4) * 1000) / 1000
            };

        }

    }

}
