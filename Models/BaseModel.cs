using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class BaseModel
    {
        public string id { get; set; }
        public string date_time 
        { 
            get; 
            
            set; 
        
        }

        public double unix_date_time { get; set; }

        public BaseModel()
        {
            id = Guid.NewGuid().ToString();
            date_time = DateTime.UtcNow.ToString();
            unix_date_time = Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
         
        }
    }
}
