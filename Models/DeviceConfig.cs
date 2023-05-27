using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class DeviceConfig : BaseModel
    {
        public string serial_number { get; set; }
        public string subCPU1 { get; set; }
        public string subCPU2 { get; set; }
        public string subCPU3 { get; set; }
        public string subCPU4 { get; set; }
        public string display { get; set; }
        public DeviceConfig() : base()
        {

        }
    }
}
