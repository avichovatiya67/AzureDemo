using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class WeightTrackConfig : BaseModel
    {

        public string bedID { get; set; }
        public string episode_id { get; set; }

        public int is_track_on { get; set; }
        public int angle { get; set; }
	    public double base_weight { get; set; }
        public double upper_limit { get; set; }

        public double lower_limit { get; set; }
        public WeightTrackConfig() : base()
        {


        }
    }
}
