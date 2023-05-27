using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class WeightTrack : BaseModel
    {

        public string bedID { get; set; }
        public string episode_id { get; set; }

        public string configuration_id { get; set; }

        public double weight_difference { get; set; }

        public string track_limit_id { get; set; }

        public WeightTrack() : base()
        {


        }
    }
}