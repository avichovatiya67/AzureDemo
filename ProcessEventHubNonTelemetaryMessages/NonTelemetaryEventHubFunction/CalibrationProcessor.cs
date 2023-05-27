using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stryker.SmartMedic.Models;

namespace ProcessEventHubNonTelemetaryMessages
{
    public static partial class MessageProcessor
    {
        public static bool CalibrationProcessor(EventData eventData, ILogger logger)
        {
            bool result = true;
            
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                dynamic dynObj = JsonConvert.DeserializeObject(messageBody);
                logger.LogInformation("messageBody {0}", messageBody);
                string eventname = Convert.ToString(dynObj.eventName);
                string deviceID = Convert.ToString(dynObj.deviceId);
                string episodeID = Convert.ToString(dynObj.episodeId);
                bool calibrationStatus = Convert.ToBoolean(dynObj.calibrationStatus);
                object calibrationDataObject = dynObj;
                object calibrationDataObjectHead = dynObj.headPlank;
                logger.LogInformation("calibrationDataObject {0} headobj {1}", calibrationDataObject, calibrationDataObjectHead);
                dynamic calibrationData = setCalibrationData(dynObj, logger);                              
                string unixdatetime = Convert.ToString(dynObj.unixDateTime);
                double unix_date_time = 0;
                if (!string.IsNullOrEmpty(unixdatetime))
                {
                    unix_date_time = Convert.ToDouble(dynObj.unixDateTime);
                }
                logger.LogInformation("unix_date_time {0} calibrationStatus {1}", unix_date_time, calibrationStatus);
                NonTelemetaryDB db = new NonTelemetaryDB(logger);
                db.InsertCalibrationDataForDevice(eventname, deviceID, episodeID, calibrationStatus, unix_date_time, calibrationData);
            }
            catch(Exception ex)
            {
                result = false;
                logger.LogError("ERROR while processing calibration event: {0}", ex.Message);
            }
            return result;
        }
        public static object setCalibrationData(dynamic dynObj, ILogger logger)
        {

            Dictionary<string, CalibrationPlank> calibrationData = new Dictionary<string, CalibrationPlank>();
            Dictionary<string, WeightBeforeCalibration> calibrationWeightData = new Dictionary<string, WeightBeforeCalibration>();

            if (dynObj.ContainsKey("headPlank"))
            {
                object headPlank = dynObj.headPlank;
                object weightbefore = dynObj.headPlank.WeightBeforeCalibration;
                object tenkg = dynObj.headPlank.WeightBeforeCalibration.tenkg;
                bool test = dynObj.headPlank.WeightBeforeCalibration is not null;
                logger.LogInformation("calibrationData headPlank dynObj {0}", headPlank);
                logger.LogInformation("test not null {0}", test);
                logger.LogInformation("weightbefore", weightbefore);
                logger.LogInformation("tenkg", tenkg);
                calibrationData.Add("headPlank", new CalibrationPlank()
                {
                    FactorRatio = dynObj.headPlank.factorRatio,
                    FactorConstant = dynObj.headPlank.factorConst,
                    WeightBeforeCalibration = dynObj.headPlank.WeightBeforeCalibration is null ? null : new WeightBeforeCalibration()
                    {
                        Tenkg = dynObj.headPlank.WeightBeforeCalibration.Tenkg,
                        Twentykg = dynObj.headPlank.WeightBeforeCalibration.Twentykg,
                        Fiftykg = dynObj.headPlank.WeightBeforeCalibration.Fiftykg
                    }
                });
            }
            if (dynObj.ContainsKey("abdomenPlank"))
            {
                calibrationData.Add("abdomenPlank", new CalibrationPlank()
                {
                    FactorRatio = dynObj.abdomenPlank.factorRatio,
                    FactorConstant = dynObj.abdomenPlank.factorConst,
                    WeightBeforeCalibration = dynObj.abdomenPlank.WeightBeforeCalibration is null ? null : new WeightBeforeCalibration()
                    {
                        Tenkg = dynObj.abdomenPlank.WeightBeforeCalibration.Tenkg,
                        Twentykg = dynObj.abdomenPlank.WeightBeforeCalibration.Twentykg,
                        Fiftykg = dynObj.abdomenPlank.WeightBeforeCalibration.Fiftykg
                    } 
                });
            }
            if (dynObj.ContainsKey("thighPlank"))
            {
                calibrationData.Add("thighPlank", new CalibrationPlank()
                {
                    FactorRatio = dynObj.thighPlank.factorRatio,
                    FactorConstant = dynObj.thighPlank.factorConst,
                    WeightBeforeCalibration = dynObj.thighPlank.WeightBeforeCalibration is null ? null : new WeightBeforeCalibration()
                    {
                        Tenkg = dynObj.thighPlank.WeightBeforeCalibration.Tenkg,
                        Twentykg = dynObj.thighPlank.WeightBeforeCalibration.Twentykg,
                        Fiftykg = dynObj.thighPlank.WeightBeforeCalibration.Fiftykg
                    }
                });
            }
            if (dynObj.ContainsKey("footPlank"))
            {
                calibrationData.Add("footPlank", new CalibrationPlank()
                {
                    FactorRatio = dynObj.footPlank.factorRatio,
                    FactorConstant = dynObj.footPlank.factorConst,
                    WeightBeforeCalibration = dynObj.footPlank.WeightBeforeCalibration is null ? null : new WeightBeforeCalibration()
                    {
                        Tenkg = dynObj.footPlank.WeightBeforeCalibration.Tenkg,
                        Twentykg = dynObj.footPlank.WeightBeforeCalibration.Twentykg,
                        Fiftykg = dynObj.footPlank.WeightBeforeCalibration.Fiftykg
                    }
                });
            }
            logger.LogInformation("final DATA {0}", calibrationData);
            return calibrationData;
        }
    }
}
