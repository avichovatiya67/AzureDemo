using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class FallAlert : BaseModel
    {
        public string bedID { get; set; }
        public string episodeID { get; set; }
     
        public FallAlert() : base()
        {

        }
    }
}
