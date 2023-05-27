using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class Hospital : BaseModel
    {
        public string hospital_name { get; set; }
        public string hospital_code { get; set; }
        public string hospital_desc { get; set; }

        public Address hospital_address { get; set; }

        public string contact_number { get; set; }

        public class Address
        {
            public string location { get; set; }
            public string City { get; set; }

            public string State { get; set; }
            public string PinCode { get; set; }
        };
        public Hospital() : base()
        {

        }
    }

}
