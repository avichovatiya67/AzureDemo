using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class GatewayDevice : BaseModel
    {
        public string tablet_id { get; set; }
        public string hospital_id { get; set; }
        public string icu_id { get; set; }
        public int connection_status { get; set; }   // 0-offline, 1-online, 2- created /provisioned
        public double created_at_unix { get; set; }
        public string created_at { get; set; }
        public GatewayDevice() : base()
        {

        }
    }
}
