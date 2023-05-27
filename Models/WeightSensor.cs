using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class WeightSensor : BaseModel
    {

        public string bedID { get; set; }
        public string episode_id { get; set; }

        public string date_time { get; set; }

        public float weight_data { get; set; }

        public int connection_off { get; set; }

        public string devicetype { get; set; }

        public string deviceversion { get; set; }

        public string hospitalcode { get; set; }
        public string messagetype { get; set; }
        public string messageversion { get; set; }
        public Plank wp1 { get; set; }

        public Plank wp2 { get; set; }

        public Plank wp3 { get; set; }

        public Plank wp4 { get; set; }

        public WeightSensor() : base()
        {


        }
    }
}
