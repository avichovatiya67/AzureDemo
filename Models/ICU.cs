using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class ICU : BaseModel
    {
        public string icu_name { get; set; }
        public string icu_desc { get; set; }
        public string icu_type { get; set; }
        public string hospital_id { get; set; }
        public ICU() : base()
        {

        }
    }
}
