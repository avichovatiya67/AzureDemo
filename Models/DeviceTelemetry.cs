using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class DeviceTelemetry : BaseModel
    {
        public string bedID { get; set; }
        public string episodeID { get; set; }

        public int bed_status { get; set; }
        public string message_type { get; set; }
        public string datetime { get; set; }
        public double unixdatetime { get; set; }

        public DeviceTelemetry() : base()
        {

        }
    }
}
