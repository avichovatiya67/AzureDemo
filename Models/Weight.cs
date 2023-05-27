using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{

    public class Weight : BaseModel
    {

        public string bedID { get; set; }
        public string episode_id { get; set; }

        public float weight_data { get; set; }

        public int connection_off { get; set; }

        public Weight() : base()
        {


        }
    }
}
