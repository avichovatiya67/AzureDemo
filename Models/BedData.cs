using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class BedData : BaseModel
    {
        public string bed_name { get; set; }
        public string bed_desc { get; set; }
       
        public string icu_id { get; set; }
        public int is_active { get; set; }
     
        public BedData() : base()
        {

        }
    }
}
