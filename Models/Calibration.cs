using System;
using System.Collections.Generic;
using System.Text;

namespace Stryker.SmartMedic.Models
{
    public class Calibration : BaseModel
    {
        public string eventName { get; set; }
        public string deviceId { get; set; }
        public string episodeId { get; set; }

        public bool calibrationStatus { get; set; }

        public CalibrationPlank headPlank { get; set; }
        public CalibrationPlank abdomenPlank { get; set; }

        public CalibrationPlank thighPlank { get; set; }
        public CalibrationPlank footPlank { get; set; }

        public double deviceRegistrationDate { get; set; } 

        public Calibration() : base()
        {

        }
    }

    public class CalibrationPlank
    {
        public decimal FactorRatio;
        public decimal FactorConstant;
        public WeightBeforeCalibration WeightBeforeCalibration;
    }

    public class WeightBeforeCalibration
    {
        public decimal Tenkg;
        public decimal Twentykg;
        public decimal Fiftykg;  
    }
}
