using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stryker.SmartMedic.Models;

namespace ProcessEventHubNonTelemetaryMessages
{
    public static partial class MessageProcessor
    {
        [FunctionName("MessageProcessor")]
        /*Production
         * public static async Task Run([EventHubTrigger("iotHub-smartmedic", Connection = "smartmedic-NonTelemetaryhub")] EventData[] events,
            [SignalR(HubName = "notifications")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)*/
        // Development
        //[EventHubTrigger("sm-staging-nt-hub", Connection = "smartmedic-NonTelemetaryhub")]
        public static async Task Run(
            [EventHubTrigger("%EventHubName_Azure%", Connection = "%smartmedictelemetary%")]
                EventData[] events,
            [SignalR(HubName = "notifications")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log
        )
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    // string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    string messageType = eventData.Properties["messagetype"].ToString();

                    ProcessMessage(signalRMessages, messageType, eventData, log);
                    notifyClients(signalRMessages, messageType, eventData, log);

                    log.LogInformation(
                        $"C# Event Hub trigger function processed a message: {messageType}"
                    );

                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                    log.LogError(
                        "C# Event Hub trigger function processed a message: {0}",
                        e.Message
                    );
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        private static async void notifyClients(
            IAsyncCollector<SignalRMessage> signalRMessages,
            string messageType,
            EventData eventData,
            ILogger log
        )
        {
            string messageBody = Encoding.UTF8.GetString(
                eventData.Body.Array,
                eventData.Body.Offset,
                eventData.Body.Count
            );
            string hospitalCode = "1234"; //eventData.Properties["HospitalCode"].ToString();
            if (!String.IsNullOrWhiteSpace(hospitalCode))
            {
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        //  UserId = hospitalCode,
                        Target = "notify",
                        Arguments = new object[] { messageType, messageBody }
                    }
                );
                log.LogInformation($"NOtification sent to client: {messageType}");
            }
        }

        public static void ProcessMessage(
            IAsyncCollector<SignalRMessage> signalRMessages,
            string messageType,
            EventData eventdata,
            ILogger log
        )
        {
            if (!string.IsNullOrEmpty(messageType))
            {
                Dictionary<string, Delegate> functionsDict = getFunctionsDictionary();
                if (functionsDict.ContainsKey(messageType))
                {
                    object result = functionsDict[messageType].DynamicInvoke(eventdata, log);
                    //bool returnFlag = (bool)result;
                    //if (returnFlag == true)
                    //{
                    //    notifyClients(signalRMessages, messageType, eventdata, log);
                    //}

                    log.LogInformation(
                        $"C# Event Hub trigger function processed a message: {0}",
                        messageType
                    );
                }
                else
                {
                    log.LogInformation($"Message handler do not exist: {0}", messageType);
                }
            }
        }

        private static Dictionary<string, Delegate> getFunctionsDictionary()
        {
            Dictionary<string, Delegate> functionsDict = new Dictionary<string, Delegate>()
            {
                { "Admit-Message", new Func<EventData, ILogger, bool>(AdmitProcessor) },
                { "Discharge-Message", new Func<EventData, ILogger, bool>(DischargeProcessor) },
                { "Weight-Message", new Func<EventData, ILogger, bool>(WeightProcessor) },
                { "Position-Message", new Func<EventData, ILogger, bool>(PositionProcessor) },
                { "Position-Timer", new Func<EventData, ILogger, bool>(PositionTimerProcessor) },
                { "Usage-Message", new Func<EventData, ILogger, bool>(UsuageProcessor) },
                { "Error-Message", new Func<EventData, ILogger, bool>(ErrorMessageProcessor) },
                { "WeightTrack-Message", new Func<EventData, ILogger, bool>(WeightTrackConfigProcessor)},
                { "WeightTrackLimit-Message", new Func<EventData, ILogger, bool>(WeightTrackConfigProcessor)},
                { "TrackWeightChange-Message", new Func<EventData, ILogger, bool>(WeightTrackProcessor)},
                { "PatientFallAlert-Message", new Func<EventData, ILogger, bool>(PatientFallAlertProcessor)},
                { "Register-Device", new Func<EventData, ILogger, bool>(RegisterDeviceProcessor) },
                { "Register-GatewayDevice", new Func<EventData, ILogger, bool>(GatewayDeviceProcessor)},
                { "Device-ConnectionStatus", new Func<EventData, ILogger, bool>(ConnectionStatusProcessor)},
                { "GatewayDevice-ConnectionStatus", new Func<EventData, ILogger, bool>(GatewayConnectionStatusProcessor)},
                { "Config-Device", new Func<EventData, ILogger, bool>(ConfigDeviceProcessor) },
                { "RemoveBed-Message", new Func<EventData, ILogger, bool>(RemoveBedMessageProcessor)},
                { "Calibration-Message", new Func<EventData, ILogger, bool>(CalibrationProcessor) }
            };
            return functionsDict;
        }
    }

    //public class DBChangedEventArgs : EventArgs
    //{
    //    public IAsyncCollector<SignalRMessage> SignalRMessages
    //    {
    //        get;
    //        set;
    //    }
    //    public string MessageType
    //    {
    //        get;
    //        set;
    //    }

    //    public EventData EventDataArgs
    //    {
    //        get;
    //        set;
    //    }

    //    public ILogger Logger
    //    {
    //        get;
    //        set;
    //    }
    //}
}
