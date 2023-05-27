using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    
    public class Position : BaseModel
    {

        public string bedID { get; set; }
        public string episode_id { get; set; }

        public string position { get; set; }

        public int position_status { get; set; }

        public string position_timer { get; set; }

        public int connection_off { get; set; }

        public Position() : base()
        {


        }
    }
}
