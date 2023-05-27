using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class WeightTrackLimit : BaseModel
    {

        public string bedID { get; set; }
        public string episode_id { get; set; }
        public string configuration_id { get; set; }
        public double upper_limit { get; set; }
        public double lower_limit { get; set; }
        public WeightTrackLimit() : base()
        {


        }
    }
}
