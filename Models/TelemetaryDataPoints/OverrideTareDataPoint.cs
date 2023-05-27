﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{

    public class OverrideTareDataPoint
    {

        public dynamic dynObj = null;

        public string telemetaryversion = string.Empty;
        public string devicetype = string.Empty;
        public string deviceversion = string.Empty;
        public string bedID = string.Empty;
        public string bed_Name = string.Empty;
        public string hospitalcode = string.Empty;
        public string icu_id = string.Empty;
        public string messagetype = string.Empty;
        public string eventype = string.Empty;
        public string messageversion = string.Empty;
        public string SequenceNumber = string.Empty;
        public string datetime = string.Empty; 
        public long unixdatetime;
        public string start_datetime = string.Empty;
        public string end_datetime = string.Empty;
        public decimal Tarevalue = 0;
        public string id = string.Empty;       
        public decimal soft_tare = 0;
        public Plank wp1;
        public Plank wp2;
        public Plank wp3;
        public Plank wp4;
        
        public OverrideTareDataPoint()
        {
        }
        public OverrideTareDataPoint(dynamic dynObj)
        {
            id = Guid.NewGuid().ToString();
            telemetaryversion = Convert.ToString(dynObj.telemetaryversion);
            devicetype = Convert.ToString(dynObj.devicetype);
            deviceversion = Convert.ToString(dynObj.deviceversion);
            hospitalcode = Convert.ToString(dynObj.hospitalcode);
            icu_id = Convert.ToString(dynObj.icu_id);
            bedID = Convert.ToString(dynObj.bedID);
            bed_Name = Convert.ToString(dynObj.bed_Name);
            messagetype = Convert.ToString(dynObj.messagetype);
            messageversion = Convert.ToString(dynObj.messageversion);
            eventype = Convert.ToString(dynObj.eventype);
            SequenceNumber = Convert.ToString(dynObj.SequenceNumber);
            datetime = Convert.ToString(dynObj.datetime);
            unixdatetime = Convert.ToInt64(dynObj.unixdatetime);
            soft_tare = Convert.ToDecimal(dynObj.soft_tare);
            start_datetime = Convert.ToString(dynObj.start_datetime);
            end_datetime = Convert.ToString(dynObj.end_datetime);
            Tarevalue = Convert.ToDecimal(dynObj.Tarevalue);

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
