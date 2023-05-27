using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
      public class Episode : BaseModel
    {
        public string bedID { get; set; }

        public int is_active { get; set; }

	    public string reference_number { get; set; }

        public Episode() : base()
        {

        }
    }
}
