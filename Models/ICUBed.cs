using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class ICUBed : BaseModel
    {
        public string bed_name { get; set; }
        public string bed_desc { get; set; }
        public string serial_number { get; set; }
        public string icu_id { get; set; }
        public string tablet_id { get; set; }
        public int bed_status { get; set; }
        public string position_timer { get; set; }
        public int telemetryInterval { get; set; }

        public int connection_status { get; set; }   // 0-offline, 1-online, 2- created /provisioned
        public ICUBed() : base()
        {

        }
    }
}
