using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
namespace ProcessEventHubNonTelemetaryMessages
{
    public static partial class DeviceConnectionStateHandler
    {
        [FunctionName("DeviceConnectionStateHandler")]
        public static void Run([EventGridTrigger]EventGridEvent eventGridEvent, [SignalR(HubName = "notifications")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {

            string iotHubName = Environment.GetEnvironmentVariable("iothubName");
            int connected = 0;
            log.LogInformation("Inside Event Grid handler !");
           // log.LogInformation(eventGridEvent.EventType.ToString());
            
            dynamic dynObj = eventGridEvent.Data;
            try
            {
                log.LogInformation("Inside Try catch handler !");
                
                string deviceID = Convert.ToString(dynObj.deviceId);
                string hubName = Convert.ToString(dynObj.hubName);

                string eventType = eventGridEvent.EventType;
                NonTelemetaryDB db = new NonTelemetaryDB(log);
                log.LogInformation("Hub Name : " + hubName);
                if (iotHubName.Trim().ToLower() == hubName.Trim().ToLower())
                {

                    log.LogInformation("Hub Name : " + hubName);
                    if (isDeviceConnected(eventGridEvent.EventType, out connected))
                    {
                        if (connected == 0 || connected == 1)
                        {
                            log.LogInformation("Calling update bed connection status ");
                            /*if (connected == 0)
                            {
                                db.InsertDataForConnectionOff(deviceID, connected);
                                log.LogInformation("Connection off : " + connected);
                            }*/
                            if ((deviceID.Contains("--tablet"))) { 
                                db.UpdateGatewayConnectionStatus(deviceID, connected);
                                notifyClients(signalRMessages, eventType, deviceID, log);
                            }else {
                                db.UpdateBedConnectionStatus(deviceID, connected);
                                notifyClients(signalRMessages, eventType, deviceID, log);
                            }
                        }

                    }
                    else if (connected == 3)//Device disconnected
                    {
                        db.DeleteBed(deviceID);
                    }
                }
               
            }
            catch (RuntimeBinderException)
            {
                log.LogInformation("DeviceID donot exists in EventGrid Event Data !");

            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            log.LogInformation("End Event Grid handler !");
        }


        private static async void notifyClients(IAsyncCollector<SignalRMessage> signalRMessages, string eventType, string deviceid,  ILogger log)
        {
            const string DEVICE_DISCONNECTED = "DeviceDisconnected";
            string messageBody = @"{""deviceid"":""" + deviceid + @"""}";
            string hospitalCode = "1234";//eventData.Properties["HospitalCode"].ToString();//dummy
            if (!String.IsNullOrWhiteSpace(hospitalCode))
            {
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        //  UserId = hospitalCode,
                        Target = "notify",
                        Arguments = new object[] { DEVICE_DISCONNECTED, messageBody }
                    }
                    );
                log.LogInformation($"NOtification sent to client: {eventType}");
            }
        }

        //connectionstatus 0- offline, 1 -online, 3 - Created/Provisioned
        private static bool isDeviceConnected(string eventType,out int connectionstatus)
        {

            const string DEVICE_CONNECTED = "DeviceConnected";
            const string DEVICE_DISCONNECTED = "DeviceDisconnected";

            const string DEVICE_CREATED = "DeviceCreated";
            const string DEVICE_DELETED = "DeviceDeleted";

            bool flag = false;
            connectionstatus = 0;


            if (eventType.Contains(DEVICE_DISCONNECTED))
            {
                flag = true;
                connectionstatus = 0;
            }
            else if (eventType.Contains(DEVICE_CONNECTED))
            {
                flag = true;
                connectionstatus = 1;
            }
            else if (eventType.Contains(DEVICE_CREATED))
            {
                flag = true;
                connectionstatus = 2;
            }
           
            else if (eventType.Contains(DEVICE_DELETED))
            {
                flag = false;
                connectionstatus = 3;
            }
            return flag;
        }
       
      
    }
}
