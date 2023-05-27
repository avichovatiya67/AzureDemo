using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{

    public class TareDataPoint
    {

        public dynamic dynObj = null;
        public string id = string.Empty;
        public string SequenceNumber = string.Empty;
        public decimal Tarevalue = 0;
        public string  bedID = string.Empty;
        public string  bed_Name = string.Empty;
        public string  datetime = string.Empty;
        public string  devicetype = string.Empty;
        public string  deviceversion = string.Empty;
        public string  end_datetime = string.Empty;
        public string  eventype = string.Empty;
        public string  hospitalcode = string.Empty;
        public string  icu_id = string.Empty;
        public string  messagetype = string.Empty;
        public string  messageversion = string.Empty;
        public decimal soft_tare = 0;
        public string  start_datetime = string.Empty;
        public string  telemetaryversion = string.Empty;
        public long  unixdatetime;
        public Plank wp1 = null;
        public Plank wp2 = null;
        public Plank wp3 = null;
        public Plank wp4 = null;

        public TareDataPoint()
        {
        }
        public TareDataPoint(dynamic dynObj)
        {
            id = Guid.NewGuid().ToString();
            SequenceNumber = Convert.ToString(dynObj.SequenceNumber);
            Tarevalue = Convert.ToDecimal(dynObj.Tarevalue);
            bedID = Convert.ToString(dynObj.bedID);
            bed_Name = Convert.ToString(dynObj.bed_Name);
            datetime = Convert.ToString(dynObj.datetime);
            devicetype = Convert.ToString(dynObj.devicetype);
            deviceversion = Convert.ToString(dynObj.deviceversion);
            end_datetime = Convert.ToString(dynObj.end_datetime);
            eventype = Convert.ToString(dynObj.eventype);
            hospitalcode = Convert.ToString(dynObj.hospitalcode);
            icu_id = Convert.ToString(dynObj.icu_id);
            messagetype = Convert.ToString(dynObj.messagetype);
            messageversion = Convert.ToString(dynObj.messageversion);
            soft_tare = Convert.ToDecimal(dynObj.soft_tare);
            start_datetime = Convert.ToString(dynObj.start_datetime);
            telemetaryversion = Convert.ToString(dynObj.telemetaryversion);
            unixdatetime = Convert.ToInt64(dynObj.unixdatetime);
            wp1 = dynObj.wp1 != null ? new Plank()
            {
                BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp1.bedAngle) * 1000) / 1000,
                BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp1.bedpitch) * 1000) / 1000,
                BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp1.bedroll) * 1000) / 1000,
                WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc1) * 1000) / 1000,
                WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc2) * 1000) / 1000,
                WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc3) * 1000) / 1000,
                WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp1.wc4) * 1000) / 1000
            } : null;
            wp2 = dynObj.wp2 != null ? new Plank()
            {

                BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp2.bedAngle) * 1000) / 1000,
                BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp2.bedpitch) * 1000) / 1000,
                BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp2.bedroll) * 1000) / 1000,
                WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc1) * 1000) / 1000,
                WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc2) * 1000) / 1000,
                WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc3) * 1000) / 1000,
                WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp2.wc4) * 1000) / 1000
            } : null;
            wp3 = dynObj.wp3 != null ?  new Plank()
            {

                BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp3.bedAngle) * 1000) / 1000,
                BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp3.bedpitch) * 1000) / 1000,
                BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp3.bedroll) * 1000) / 1000,
                WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc1) * 1000) / 1000,
                WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc2) * 1000) / 1000,
                WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc3) * 1000) / 1000,
                WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp3.wc4) * 1000) / 1000
            } : null;
            wp4 = dynObj.wp4 != null ?  new Plank()
            {
                BedAngle = Math.Truncate(Convert.ToDecimal(dynObj.wp4.bedAngle) * 1000) / 1000,
                BedPitch = Math.Truncate(Convert.ToDecimal(dynObj.wp4.bedpitch) * 1000) / 1000,
                BedRoll = Math.Truncate(Convert.ToDecimal(dynObj.wp4.bedroll) * 1000) / 1000,
                WC1 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc1) * 1000) / 1000,
                WC2 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc2) * 1000) / 1000,
                WC3 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc3) * 1000) / 1000,
                WC4 = Math.Truncate(Convert.ToDecimal(dynObj.wp4.wc4) * 1000) / 1000
            }: null;

        }

    }

}
