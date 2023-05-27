using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class UsuageData : BaseModel
    {

        public string serial_number { get; set; }

        public string event_type { get; set; }
        
        //date time in base model
    }
}
