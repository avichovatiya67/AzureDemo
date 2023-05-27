using Microsoft.Azure.Cosmos;

using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Stryker.SmartMedic.Models;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace ProcessEventHubNonTelemetaryMessages
{

    public class NonTelemetaryDB
    {        


        // The Cosmos client instance
        //private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        // private string databaseId = "smartmedic-db";
        private string databaseId = Environment.GetEnvironmentVariable("databaseId");
        private string episode_containerId = "episode_details";
        private string weight_container = "weight_data";
        private string turn_container = "turn_data";
        private string bedMaster_container = "bed_master";
        private string devices_container = "devices";
        private string deviceconfig_container = "device_config";
        private string hospitalMaster_container = "hospital_master";
        private string usuagedata_container = "usuage_data";
        private string errormessage_container = "error_messages";
        private string weighttrackconfig_container = "weight_track_configuration";
        private string weighttrack_container = "weight_track_data";
        private string weighttracklimit_container = "weight_track_limit";
        private string devicetelemetry_container = "device-telemetary";
        private string configuration_container = "configuration";
        private string fallalert_container = "fall_alert";
        private string gateway_devices = "gateway_devices";
        private string calibration_container = "calibration_data";
        private string calibration_history_container = "calibration_history";
        private ILogger log = null;

        private static Lazy<CosmosClient> lazyClient = new Lazy<CosmosClient>(InitializeCosmosClient);
        private static CosmosClient cosmosClient => lazyClient.Value;

        private static CosmosClient InitializeCosmosClient()
        {
            string dbConnectionString = Environment.GetEnvironmentVariable("smartmedicDBConnection");
            return new CosmosClient(dbConnectionString);
        }

        public NonTelemetaryDB(ILogger logger)
        {
            log = logger;
            //cosmosClient = new CosmosClient(dbConnectionString);
        }
        public async void InsertActiveEpisodeForBed(string bedID, string episodeID)
        {
            try
            {

                await this.SetEpisodesInActiveForBed(bedID);
                log.LogInformation("Set Episodes In Active for bed");
                //Container container = cosmosClient.GetContainer(databaseId, episode_containerId);
                database = cosmosClient.GetDatabase(databaseId);
                Container container = cosmosClient.GetContainer(databaseId, episode_containerId);
                //container = await database.CreateContainerIfNotExistsAsync(episode_containerId, "/bedID");

                Episode newEpisode = new Episode();
                newEpisode.bedID = bedID;
                newEpisode.is_active = 1;
                newEpisode.id = episodeID;

                ItemResponse<Episode> newEpisodeResponse = await container.UpsertItemAsync<Episode>(newEpisode, new PartitionKey(newEpisode.bedID));

                // ItemResponse<Episode> newEpisodeResponse = await container.CreateItemAsync<Episode>(newEpisode, new PartitionKey(newEpisode.bedID));

                log.LogInformation("Inserted new Episode {0} with bed {1}", newEpisode.id, newEpisode.bedID);
            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while inserting active episode: {0}", ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("ERROR while inserting active episode: {0}", ex.Message);
            }
        }
        public async void InsertFallAlertInfo(string bedID, string episodeID, string datetime, double unixdatetime)
        {
            try
            {
                database = cosmosClient.GetDatabase(databaseId);
                container = await database.CreateContainerIfNotExistsAsync(fallalert_container, "/bedID");

                FallAlert fallAlert = new FallAlert();
                fallAlert.bedID = bedID;
                fallAlert.episodeID = episodeID;
                fallAlert.date_time = datetime;
                fallAlert.unix_date_time = unixdatetime;

                ItemResponse<FallAlert> newFallAlertResponse = await container.CreateItemAsync<FallAlert>(fallAlert, new PartitionKey(fallAlert.bedID));

                log.LogInformation("Inserted new Fall-Alert {0} with bed {1}", fallAlert.id, fallAlert.bedID);
            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("ERROR while Fall-Alert: {0}", ex.Message);
            }
        }
        public async void insertActiveBed(string bedID, string bedStatus, string positionTimer, string bed_name, string icu_id, string tablet_id)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                // await DeleteBed(bedID);
                List<ICUBed> bedsExisting = await getDuplicateICUBedsForDevice(bedID);
                log.LogInformation("Insert Active bed started");


                ICUBed bed = new ICUBed();
                bed.serial_number = bedID;
                bed.bed_name = bed_name;
                bed.bed_desc = "";
                bed.icu_id = icu_id;
                bed.tablet_id = tablet_id;
                bed.bed_status = Convert.ToInt32(bedStatus);
                bed.position_timer = positionTimer;   //2 is default
                bed.connection_status = 1; //Device is connect since message reached till this point.
                ItemResponse<ICUBed> newEpisodeResponse = await container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number));
                log.LogInformation("Updated bed Information with id {0} and Device {1}", bed.id, bed.serial_number);


                if (bedsExisting != null)
                {
                    if (bedsExisting.Count > 0)
                    {
                        foreach (ICUBed bedToDelete in bedsExisting)
                        {
                            ItemResponse<ICUBed> newBedResponse = await container.DeleteItemAsync<ICUBed>(bedToDelete.id, new PartitionKey(bedToDelete.serial_number));
                            log.LogInformation("Deleted bed with bed id {0} and serial number {1}", bedToDelete.id, bedToDelete.serial_number);
                        }
                    }
                }
                log.LogInformation("Insert Active bed ended");
            }
            catch (CosmosException ce)
            {
                log.LogError("Cosmos error while inserting active bed: {0}", ce.Message);
            }
            catch (Exception ex)
            { 
                log.LogError("Exception while inserting active bed: {0}", ex.Message);
            }
        }


        public async void UpdatePositionTimerinPositionStatus(string bedID, string positionTimer, string episodeID)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, turn_container);
                Position position = await getLatestPositionForBed(bedID, episodeID);
                if (position != null)
                {


                    if (!string.IsNullOrWhiteSpace(positionTimer))
                        position.position_timer = positionTimer;

                    ItemResponse<Position> newEpisodeResponse = await container.UpsertItemAsync<Position>(position, new PartitionKey(position.bedID));
                    log.LogInformation("Updated bed Information with id {0} and Device {1}", position.id, position.bedID);

                }

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        public async void UpdateBedInfo(string bedID, string bedStatus, string positionTimer, string bed_name, string icu_id, string discharge_datetime = null)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                ICUBed bed = await getICUBedForDevice(bedID);
                if (bed == null)
                {
                    bed = new ICUBed();
                    bed.serial_number = bedID;
                    bed.bed_name = bed_name;
                    bed.bed_desc = "";
                    bed.icu_id = icu_id;
                    bed.bed_status = Convert.ToInt32(bedStatus);
                    bed.position_timer = positionTimer;   //2 is default
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(bed_name))
                        bed.bed_name = bed_name;

                    if (!string.IsNullOrWhiteSpace(icu_id))
                        bed.icu_id = icu_id;

                    if (!string.IsNullOrWhiteSpace(positionTimer))
                        bed.position_timer = positionTimer;

                    if (!string.IsNullOrWhiteSpace(bedStatus))
                        bed.bed_status = Convert.ToInt32(bedStatus);
                }
                bed.connection_status = 1; //DEvice is connect since message reached till this point.
                ItemResponse<ICUBed> newEpisodeResponse = await container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number));
                log.LogInformation("Updated bed Information with id {0} and Device {1}", bed.id, bed.serial_number);
            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        public async void SetDischargeStateForBed(string bedID, string bedStatus, string positionTimer, string bed_name, string icu_id)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                ICUBed bed = await getICUBedForDevice(bedID);
                if (bed != null)
                {
                    if (!string.IsNullOrWhiteSpace(bed_name))
                        bed.bed_name = bed_name;

                    if (!string.IsNullOrWhiteSpace(icu_id))
                        bed.icu_id = icu_id;

                    if (!string.IsNullOrWhiteSpace(positionTimer))
                        bed.position_timer = positionTimer;

                    if (!string.IsNullOrWhiteSpace(bedStatus))
                        bed.bed_status = Convert.ToInt32(bedStatus);

                    bed.connection_status = 1; //DEvice is connect since message reached till this point.

                    ItemResponse<ICUBed> newEpisodeResponse = await container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number));
                    log.LogInformation("Updated bed Information with id {0} and Device {1}", bed.id, bed.serial_number);


                }
                //bed.connection_status = 1; //DEvice is connect since message reached till this point.

                //ItemResponse<ICUBed> newEpisodeResponse = await container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number));
                //log.LogInformation("Updated bed Information with id {0} and Device {1}", bed.id, bed.serial_number);
            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        public async void UpdateBedStatus(string bedID, int bedStatus, string errordescription)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                ICUBed bed = await getICUBedForDevice(bedID);
                if (bed != null)
                {
                    bed.bed_status = bedStatus;

                    bed.connection_status = 1; //DEvice is connect since message reached till this point.

                    ItemResponse<ICUBed> newEpisodeResponse = await container.ReplaceItemAsync<ICUBed>(bed, bed.id);
                    log.LogInformation("Updated bed Information with id {0} and Device {1}", bed.id, bed.serial_number);
                }

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        public async void InsertErrorMessage(string bedID, string errordescription)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, errormessage_container);


                ErrorMessage error = new ErrorMessage();
                error.serial_number = bedID;
                error.error_desc = errordescription;

                ItemResponse<ErrorMessage> newErrorResponse = await container.UpsertItemAsync<ErrorMessage>(error, new PartitionKey(error.serial_number));

                log.LogInformation("Updated Error Information with Device {0}", bedID);


            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        //public async void UpdateBedInfo(string bedID,string bedStatus,string positionTimer,string bed_name,string icu_id)
        //{
        //    try
        //    {

        //        Container container = cosmosClient.GetContainer(databaseId, bedMaster_container);

        //        var sqlQueryText = "SELECT * FROM c WHERE c.serial_number = '" + bedID + "'";

        //        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        //        FeedIterator<ICUBed> queryResultSetIterator = container.GetItemQueryIterator<ICUBed>(queryDefinition);

        //        while (queryResultSetIterator.HasMoreResults)
        //        {
        //            FeedResponse<ICUBed> currentResultSet;
        //            try
        //            {
        //                currentResultSet = await queryResultSetIterator.ReadNextAsync();

        //                foreach (ICUBed bed in currentResultSet)
        //                {
        //                    if(!string.IsNullOrWhiteSpace(bed_name))
        //                        bed.bed_name = bed_name;

        //                    if (!string.IsNullOrWhiteSpace(icu_id))
        //                        bed.icu_id = icu_id;

        //                    if (!string.IsNullOrWhiteSpace(positionTimer))
        //                        bed.position_timer = positionTimer;

        //                    if (!string.IsNullOrWhiteSpace(bedStatus))
        //                        bed.bed_status = bedStatus;

        //                    ItemResponse<ICUBed> episodeUpdateResponse = await container.ReplaceItemAsync<ICUBed>(bed, bed.id, new PartitionKey(bed.serial_number));


        //                    log.LogInformation("Updated {0}\n", bedID);
        //                }
        //            }
        //            catch (CosmosException e)
        //            {
        //                if (e.StatusCode == HttpStatusCode.NotFound)
        //                {
        //                    break;
        //                }
        //                else
        //                    throw;
        //            }

        //        }

        //    }
        //    catch (Exception ce)
        //    {
        //        log.LogInformation(ce.Message);
        //    }



        //}
        public async Task SetEpisodesInActiveForBed(string bedID)
        {
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, episode_containerId);

                var sqlQueryText = "SELECT * FROM c WHERE c.bedID = '" + bedID + "' and c.is_active=1";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Episode> queryResultSetIterator = container.GetItemQueryIterator<Episode>(queryDefinition);


                List<Episode> episodes = new List<Episode>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Episode> currentResultSet;
                    try
                    {
                        currentResultSet = await queryResultSetIterator.ReadNextAsync();

                        foreach (Episode episode in currentResultSet)
                        {

                            episode.is_active = 0;
                            ItemResponse<Episode> episodeUpdateResponse = await container.ReplaceItemAsync<Episode>(episode, episode.id, new PartitionKey(episode.bedID));


                            log.LogInformation("\tRead {0}\n", episode.bedID);
                        }
                    }
                    catch (CosmosException e)
                    {
                        if (e.StatusCode == HttpStatusCode.NotFound)
                        {
                            break;
                        }
                        else
                            throw;
                    }

                }

            }
            catch (Exception ce)
            {
                log.LogError("ERROR while changing episode to inactive: {0}", ce.Message);
            }



        }

        public async void InsertGetWeightEventForBed(string bedID, string episodeID, float weightData, double unixdatetime = 0, dynamic additionalWeightData = null)
        {
            const string USUAGE_WEIGHT = "GetWeight";
            try
            {


                Container container = cosmosClient.GetContainer(databaseId, weight_container);

                Episode latestEpisode = await getEpisodeById(bedID, episodeID);
                if (latestEpisode == null)
                {
                    InsertActiveEpisodeForBed(bedID, episodeID);

                }

                Weight weight = new Weight();
                weight.episode_id = episodeID;
                weight.bedID = bedID;
                weight.weight_data = weightData;
                weight.connection_off = 0;
                if (unixdatetime != 0)
                {
                    weight.unix_date_time = unixdatetime;
                }
                log.LogInformation("Inserted new weight {0} with bed unixdatetime {1}", weight.id, weight.unix_date_time);
                ItemResponse<Weight> newEpisodeResponse = await container.UpsertItemAsync<Weight>(weight, new PartitionKey(weight.bedID));


                string eventType = "GetWeight";
                //change date time to date time of bedside display
                DateTime datetimeofUsuage = DateTime.UtcNow;
                InsertUsuageEventForBed(bedID, datetimeofUsuage.ToString(), eventType);

                log.LogInformation("Inserted new weight {0} with bed {1}", weight.id, weight.bedID);
            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while inserting weight event: {0}", ce.Message);
            }
            catch (Exception ce)
            {
                log.LogError("ERROR while inserting weight event: {0}", ce.Message);
            }
        }
        public async void InsertUsuageEventForBed(string bedID, string datetime, string eventType)
        {
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, usuagedata_container);

                UsuageData wtusuage = new UsuageData();
                wtusuage.serial_number = bedID;
                wtusuage.date_time = datetime;
                wtusuage.event_type = eventType;

                ItemResponse<UsuageData> newEpisodeResponse = await container.UpsertItemAsync<UsuageData>(wtusuage, new PartitionKey(wtusuage.serial_number));
                //log.LogInformation("InsertUsuageEventForBed eventType", eventType);
                if (eventType == "FactoryReset")
                {
                    var deviceList = await GetDevicesByTablet(bedID);
                    //log.LogInformation("Updated tablet connection status {0}", deviceList);
                    if (deviceList != null)
                    {
                        //log.LogInformation("deviceList", deviceList);
                        foreach (ICUBed device in deviceList)
                        {
                            log.LogInformation("deviceList device {0}", device.serial_number);
                            if (device.serial_number != null || device.serial_number != "")
                            {
                                UpdateBedConnectionStatus(device.serial_number, 0);
                            }
                        }
                    }
                }

                log.LogInformation("Inserted new weight usuage {0} with bed {1}", wtusuage.id, wtusuage.serial_number);
            }
            catch (CosmosException ce)
            {
                log.LogError("Inserted new weight usuage {0} with bed {0}",ce.Message);
            }
            catch (Exception ce)
            {
                log.LogError("ERROR while inserting weight usuage: {0}", ce.Message);
            }
        }

        public async void InsertTurnEventForBed(string bedID, string episodeID, string position, int position_status, string positionTimer, string date_time)
        {
            try
            {


                Container container = cosmosClient.GetContainer(databaseId, turn_container);

                Episode latestEpisode = await getEpisodeById(bedID, episodeID);
                if (latestEpisode == null)
                {
                    InsertActiveEpisodeForBed(bedID, episodeID);

                }

                Position turn = new Position();
                turn.episode_id = episodeID;
                turn.bedID = bedID;
                turn.position = position;
                turn.position_status = position_status;
                turn.position_timer = positionTimer;
                turn.connection_off = 0;

                //insert the same datetime as is generated on bedside for accurate nurse station timer
                if (!string.IsNullOrWhiteSpace(date_time))
                {
                    turn.date_time = date_time;
                    turn.unix_date_time = Math.Floor((Convert.ToDateTime(date_time) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
                }

                ItemResponse<Position> newResponse = await container.UpsertItemAsync<Position>(turn, new PartitionKey(turn.bedID));

                log.LogInformation("Inserted new Position {0} with bed {1}", turn.id, turn.bedID);
            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while inserting position: {0}", ce.Message);
            }
            catch (Exception ce)
            {
                log.LogError("ERROR while inserting position : {0}", ce.Message);
            }
        }

        private async Task<Episode> getEpisodeById(string bedID, string episodeID)
        {
            Episode latestEpisode = null;

            try
            {

                Container container = cosmosClient.GetContainer(databaseId, episode_containerId);

                var sqlQueryText = "SELECT * FROM c WHERE c.id = '" + episodeID + "' and c.bedID='" + bedID + "'";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Episode> queryResultSetIterator = container.GetItemQueryIterator<Episode>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Episode> currentResultSet;
                    try
                    {
                        currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (Episode episode in currentResultSet)
                        {
                            latestEpisode = episode;
                            log.LogInformation("\tRead {0}\n", episode.id);
                            break;

                        }
                    }
                    catch (CosmosException ce)
                    {
                        if (ce.StatusCode == HttpStatusCode.NotFound)
                            break;
                        else
                            throw;

                    }

                }

            }
            catch (Exception e)
            {
                log.LogError("ERROR while getting episode id: {0}", e.Message);
            }
            return latestEpisode;
        }
        public async void UpdateDeviceTwinProperty(string bedID, string telemetryInterval, string position_timer, string subCPU1, string subCPU2, string subCPU3, string subCPU4, string display, int connection_status, string tabletId)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                Container deviceConfigContainer = cosmosClient.GetContainer(databaseId, deviceconfig_container);
                ICUBed bed = await getICUBedForDevice(bedID); 
                DeviceConfig bedConfig = await getICUBedConfig(bedID);
                // Condition for device config table
                if (bedConfig == null)
                {
                    bedConfig = new DeviceConfig();
                }
                if (bed == null)
                {
                    log.LogInformation("No bed found with device {0}", bedID);
                }
                else
                {
                    log.LogInformation("UpdateDeviceTwinProperty bed.serial_number {0}", bed.serial_number);
                    log.LogInformation("connection_status from Event {0} for tablet {1}", connection_status, tabletId);
                    bed.telemetryInterval = Convert.ToInt32(telemetryInterval);
                    bed.position_timer = position_timer;
                    if ((connection_status == 0 && bed.tablet_id == tabletId) || (connection_status == 1))
                    {
                        bed.connection_status = connection_status;
                    }
                    bedConfig.serial_number = bedID;
                    bedConfig.subCPU1 = subCPU1;
                    bedConfig.subCPU2 = subCPU2;
                    bedConfig.subCPU3 = subCPU3;
                    bedConfig.subCPU4 = subCPU4;
                    bedConfig.display = display;
                    ItemResponse<ICUBed> newTwinPropertyResponse = await container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number));
                    log.LogInformation("After device data inserted of device");
                    ItemResponse<DeviceConfig> newDeviceConfigResponse = await deviceConfigContainer.UpsertItemAsync<DeviceConfig>(bedConfig, new PartitionKey(bedConfig.serial_number));
                    log.LogInformation("Updated smartmedic-device twin property with bed {0}", bed.serial_number);
                }
            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception e)
            {
                log.LogError("UpdateDeviceTwinProperty: {0}", e.Message);
            }
        }
        public async void UpdateBedConnectionStatus(string bedID, int connectionstatus)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                ICUBed bed = await getICUBedForDevice(bedID);
                log.LogInformation(" UpdateBedConnectionStatus connectionstatus {0}", connectionstatus);
                //log.LogInformation("bed", bed);
                if (bed == null)
                {
                   log.LogInformation(" UpdateBedConnectionStatus for new bed", connectionstatus);
                    bed = new ICUBed();
                    bed.serial_number = bedID;
                    bed.connection_status = connectionstatus;
                    bed.bed_name = "";
                    bed.bed_desc = "";
                    bed.icu_id = "-1";
                    bed.bed_status = 0;
                    bed.position_timer = "2";   //2 is default
                }
                else
                {
                    //log.LogInformation(" UpdateBedConnectionStatus not-null", connectionstatus);
                    bed.serial_number = bedID;
                    bed.connection_status = connectionstatus;
                }         
                ItemResponse<ICUBed> newEpisodeResponse = await container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number));
                //container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number)).Wait();
                log.LogInformation("Updated bed connection status {0} with bed {1}", bed.id, bed.serial_number);
            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception e)
            {
                log.LogError("UpdateBedConnectionStatus: {0}", e.Message);
            }
        }

        public async void UpdateBedConnectionStatusByTablet_device(string bedID, int connectionstatus, string tabletId)
        {
            try
            {
                ICUBed bed = null;
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                log.LogInformation("UpdateBedConnectionStatusByTablet_device tabletId {0}", tabletId);
                bed = await getICUBedForDevice(bedID);
                log.LogInformation(" UpdateBedConnectionStatus tabletId {0}", tabletId);
                log.LogInformation(" UpdateBedConnectionStatus bed {0}", bed);
                log.LogInformation(" UpdateBedConnectionStatus connectionstatus {0}", connectionstatus);

                //log.LogInformation("bed", bed);
                if (bed == null)
                {
                    //log.LogInformation(" UpdateBedConnectionStatus null", connectionstatus);
                    bed = new ICUBed();
                    bed.serial_number = bedID;
                    bed.connection_status = connectionstatus;
                    bed.tablet_id = tabletId;
                    bed.bed_name = "";
                    bed.bed_desc = "";
                    bed.icu_id = "-1";
                    bed.bed_status = 0;
                    bed.position_timer = "2";   //2 is default
                    ItemResponse<ICUBed> newEpisodeResponse = await container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number));
                    //container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number)).Wait();
                    log.LogInformation("Updated bed connection status {0} with bed {1} and with tablet {2}", bed.id, bed.serial_number, bed.tablet_id);
                }
                else
                {
                    if(tabletId == "" || bed.tablet_id == tabletId)
                    {
                        bed.serial_number = bedID;
                        bed.tablet_id = tabletId;
                        bed.connection_status = connectionstatus;
                        ItemResponse<ICUBed> newEpisodeResponse = await container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number));
                        //container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number)).Wait();
                        log.LogInformation("Updated bed connection status {0} with bed {1} and with tablet {2}", bed.id, bed.serial_number, bed.tablet_id);
                    }
                    else
                    {
                        log.LogInformation("No records updated for device {0} and tablet_id {1}", bedID, tabletId);
                    }
                    //log.LogInformation(" UpdateBedConnectionStatus not-null", connectionstatus);                   
                }
            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception e)
            {
                log.LogError("UpdateBedConnectionStatusByTablet_device: {0}", e.Message);
            }
        }
        public async void UpdateGatewayConnectionStatus(string tabletID, int connectionstatus)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, gateway_devices);
                GatewayDevice tablet = await getRegisterGatewayDevice(tabletID);
                if (tablet != null)
                {
                    tablet.tablet_id = tabletID;
                    tablet.connection_status = connectionstatus;
                    ItemResponse<GatewayDevice> newEpisodeResponse = await container.UpsertItemAsync<GatewayDevice>(tablet, new PartitionKey(tablet.tablet_id));
                    //container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number)).Wait();
                    log.LogInformation("Updated tablet connection status {0} with bed {1}", tablet.id, tablet.hospital_id);
                    //For Disconnection event, Need to fetch all devices connected to the tablet and change their connection-status
                    if (connectionstatus == 0)
                    {
                        var deviceList = await GetDevicesByTablet(tabletID);
                        log.LogInformation("Updated tablet connection status {0}", deviceList);
                        if (deviceList != null)
                        {
                            foreach (ICUBed device in deviceList)
                            {
                                log.LogInformation("deviceList device {0}", device.serial_number);
                                if (device.serial_number != null || device.serial_number != "")
                                {
                                    UpdateBedConnectionStatus(device.serial_number, 0);
                                }
                            }
                        }                       
                    }
                }              

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception e)
            {
                log.LogError("UpdateGatewayConnectionStatus: {0}", e.Message);
            }
        }
        public async void RegisterDevice(string bedID, string tabletID, int connectionStatus)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                RegisterICUBed bed = await getRegisterICUBedForDevice(bedID);
                log.LogInformation("RegisterDevice {0} connectionStatus {1}", bedID, connectionStatus);
                if (bed == null)
                {
                    bed = new RegisterICUBed();
                    bed.serial_number = bedID;
                    bed.connection_status = connectionStatus;
                    bed.bed_name = "";
                    bed.bed_desc = "";
                    bed.icu_id = "-1";
                    bed.tablet_id = tabletID;
                    bed.bed_status = 0;
                    bed.position_timer = "2";   //2 is default
                }
                else
                {
                    if ((connectionStatus == 0 && bed.tablet_id == tabletID) || (connectionStatus == 1))
                    {
                        bed.serial_number = bedID;
                        bed.connection_status = connectionStatus;
                        bed.tablet_id = tabletID;
                    }
                }
                log.LogInformation("RegisterDevice InsertData {0}  with {1}", bed.serial_number, bed.connection_status);
                ItemResponse<RegisterICUBed> newBedResponse = await container.UpsertItemAsync<RegisterICUBed>(bed, new PartitionKey(bed.serial_number));
                //container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number)).Wait();
                log.LogInformation("Updated bed connection status {0} with bed {1} for tablet {2}", bed.id, bed.serial_number, tabletID);
                IList<Calibration> calibrationList = await getCalibratedRecordForDevice(bedID, 0);
                log.LogInformation("Calibration List {0} AND Count {1}", calibrationList, calibrationList.Count);
                if (calibrationList.Count > 0)
                {
                    return;
                }
                else
                {
                    log.LogInformation("Inside calibation else {0}", calibrationList.Count);
                    Container calibrationContainer = cosmosClient.GetContainer(databaseId, calibration_container);
                    Calibration calibration = new Calibration();
                    calibration.eventName = "Calibration-Message";
                    calibration.episodeId = "";
                    calibration.deviceId = bedID;
                    calibration.calibrationStatus = false;
                    calibration.headPlank = null;
                    calibration.footPlank = null;
                    calibration.abdomenPlank = null;
                    calibration.thighPlank = null;
                    calibration.deviceRegistrationDate = Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
                    ItemResponse<Calibration> calibrationResponse = await calibrationContainer.CreateItemAsync<Calibration>(calibration, new PartitionKey(calibration.deviceId));
                    log.LogInformation("Inserted new calibration {0} with bed {1}", calibration.id, calibration.deviceId);
                }
              
            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while registering the device: {0}", ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("Error from Register Device Methods {0}", ex.Message);
            }

        }

        public async void RegisterGatewayDevice(string tabletID, string hospitalID, double unixDateTime, string createdAt)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, gateway_devices);
                GatewayDevice tablet = await getRegisterGatewayDevice(tabletID);
                if (tablet == null)
                {
                    tablet = new GatewayDevice();
                    tablet.tablet_id = tabletID;
                }
                tablet.hospital_id = hospitalID;
                tablet.created_at_unix = unixDateTime;
                tablet.connection_status = 1;
                tablet.created_at = createdAt;

                ItemResponse<GatewayDevice> newEpisodeResponse = await container.UpsertItemAsync<GatewayDevice>(tablet, new PartitionKey(tablet.tablet_id));
                //container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number)).Wait();
                log.LogInformation("Updated tablet {0} with bed {1}", tablet.id, tablet.hospital_id);
            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while registering gateway device: {0}", ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("ERROR while registering gateway device: {0}", ex.Message);
            }
        }

        public async void DeleteDuplicateICUAndBed(string bedname, string icu_id, string bedID)
        {
            List<ICUBed> bedsList = new List<ICUBed>();
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, devices_container);

                var sqlQueryText = "SELECT *  FROM c WHERE c.bed_name = '" + bedname + "' and c.icu_id='" + icu_id + "'";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<ICUBed> queryResultSetIterator = container.GetItemQueryIterator<ICUBed>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<ICUBed> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (ICUBed icuBed in currentResultSet)
                    {
                        bedsList.Add(icuBed);
                    }
                }
                foreach (ICUBed bed in bedsList)
                {
                    if (bed.serial_number == bedID || bed.serial_number != bedID)
                    {
                        ItemResponse<ICUBed> newDeleteBedResponse = await container.DeleteItemAsync<ICUBed>(bed.id, new PartitionKey(bed.serial_number));
                        log.LogInformation("Deleted bed with bed id {0} and icu id {1}", bed.bed_name, bed.icu_id);
                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while deleting duplicate icu and bed: {0}", ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("ERROR while deleting duplicate icu and bed: {0}", ex.Message);
            }
        }

        public async Task DeleteBed(string bedID)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                ICUBed bed = await getICUBedForDevice(bedID);
                if (bed != null)
                {
                    ItemResponse<ICUBed> newEpisodeResponse = await container.DeleteItemAsync<ICUBed>(bed.id, new PartitionKey(bed.serial_number));
                    log.LogInformation("Deleted bed with bed id {0} and serial number {1}", bed.id, bed.serial_number);
                }


            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while deleting duplicate icu and bed: {0}",ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("ERROR while deleting duplicate icu and bed: {0}", ex.Message);
            }
        }
        //private async Task<ICUBed> getBedbySerialNumber(string serialNumber)
        //{
        //    ICUBed iCUBed = null;

        //    try
        //    {

        //        Container container = cosmosClient.GetContainer(databaseId, episode_containerId);

        //        var sqlQueryText = "SELECT * FROM c WHERE c.serial_number = '" + serialNumber + "'";

        //        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        //        FeedIterator<ICUBed> queryResultSetIterator = container.GetItemQueryIterator<ICUBed>(queryDefinition);

        //        while (queryResultSetIterator.HasMoreResults)
        //        {
        //            FeedResponse<ICUBed> currentResultSet;
        //            try
        //            {
        //                currentResultSet = await queryResultSetIterator.ReadNextAsync();
        //                foreach (ICUBed icubed in currentResultSet)
        //                {
        //                    iCUBed = icubed;
        //                    log.LogInformation("\tRead {0}\n", icubed.id);
        //                    break;

        //                }
        //            }
        //            catch (CosmosException ce)
        //            {
        //                if (ce.StatusCode == HttpStatusCode.NotFound)
        //                    break;
        //                else
        //                    throw;

        //            }

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        log.LogInformation(e.Message);
        //    }
        //    return iCUBed;
        //}
        private async Task<Episode> getActiveEpisodeForBed(string bedID)
        {
            Episode latestEpisode = null;
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, episode_containerId);

                var sqlQueryText = "SELECT TOP 1* FROM c WHERE c.bedID = '" + bedID + "' and c.is_active=1 order by c.unix_date_time desc";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Episode> queryResultSetIterator = container.GetItemQueryIterator<Episode>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Episode> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (Episode episode in currentResultSet)
                    {
                        latestEpisode = episode;
                        log.LogInformation("\tRead {0}\n", episode.id);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("getActiveEpisodeForBed: {0}", ex.Message);
            }
            return latestEpisode;
        }
        public async Task<Position> getLatestPositionForBed(string deviceID, string episodeID)
        {
            Position latestposition = null;
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, turn_container);

                var sqlQueryText = "SELECT TOP 1*  FROM c WHERE c.bedID = '" + deviceID + "' and c.episode_id='" + episodeID + "' order by c.unix_date_time desc"; QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Position> queryResultSetIterator = container.GetItemQueryIterator<Position>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Position> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (Position position in currentResultSet)
                    {
                        latestposition = position;
                        log.LogInformation("\tRead {0}\n", position.bedID);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("getLatestPositionForBed: {0}", ex.Message);
            }
            return latestposition;
        }
        public async Task<List<ICUBed>> getDuplicateICUBedsForDevice(string deviceID)
        {
            List<ICUBed> Beds = new List<ICUBed>();
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, devices_container);

                var sqlQueryText = "SELECT *  FROM c WHERE c.serial_number = '" + deviceID + "'";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<ICUBed> queryResultSetIterator = container.GetItemQueryIterator<ICUBed>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<ICUBed> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (ICUBed icuBed in currentResultSet)
                    {
                        Beds.Add(icuBed);
                        log.LogInformation("\tRead {0}\n", icuBed.serial_number);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while getting duplicate beds: {0}", ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("ERROR while getting duplicate beds: {0}", ex.Message);
            }
            return Beds;
        }

        public async Task<ICUBed> getICUBedForDevice(string deviceID)
        {
            ICUBed Bed = null;
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, devices_container);

                var sqlQueryText = "SELECT TOP 1*  FROM c WHERE c.serial_number = '" + deviceID + "'";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<ICUBed> queryResultSetIterator = container.GetItemQueryIterator<ICUBed>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<ICUBed> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (ICUBed icuBed in currentResultSet)
                    {
                        Bed = icuBed;
                        log.LogInformation("\tRead from device {0}\n", Bed.serial_number);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("getICUBedForDevice: {0}", ex.Message);
            }
            return Bed;
        }
        public async Task<DeviceConfig> getICUBedConfig(string deviceID)
        {
            DeviceConfig Bed = null;
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, deviceconfig_container);

                var sqlQueryText = "SELECT TOP 1*  FROM c WHERE c.serial_number = '" + deviceID + "'";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<DeviceConfig> queryResultSetIterator = container.GetItemQueryIterator<DeviceConfig>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<DeviceConfig> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (DeviceConfig icuBed in currentResultSet)
                    {
                        Bed = icuBed;
                        log.LogInformation("\tRead from device config {0}\n", Bed.serial_number);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("getICUBedConfig: {0}", ex.Message);
            }
            return Bed;
        }
        public async Task<RegisterICUBed> getRegisterICUBedForDevice(string deviceID)
        {
            RegisterICUBed Bed = null;
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, devices_container);

                var sqlQueryText = "SELECT TOP 1*  FROM c WHERE c.serial_number = '" + deviceID + "'";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<RegisterICUBed> queryResultSetIterator = container.GetItemQueryIterator<RegisterICUBed>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<RegisterICUBed> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (RegisterICUBed icuBed in currentResultSet)
                    {
                        Bed = icuBed;
                        log.LogInformation("\tRead from device from register event{0}\n", Bed.serial_number);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while getting device details: {0}", ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("ERROR while getting device details: {0}", ex.Message);
            }
            return Bed;
        }

        public async Task<GatewayDevice> getRegisterGatewayDevice(string tabletID)
        {
            GatewayDevice Tablet = null;
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, gateway_devices);

                var sqlQueryText = "SELECT TOP 1*  FROM c WHERE c.tablet_id = '" + tabletID + "'";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<GatewayDevice> queryResultSetIterator = container.GetItemQueryIterator<GatewayDevice>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<GatewayDevice> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (GatewayDevice icuBed in currentResultSet)
                    {
                        Tablet = icuBed;
                        log.LogInformation("\tRead {0}\n", Tablet.tablet_id);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError("ERROR while getting gateway device details: {0}", ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("ERROR while getting gateway device details: {0}", ex.Message);
            }
            return Tablet;
        }

        //public async Task<Hospital> getHospitalCodeforDevice(string deviceID)
        //{

        //    try
        //    {

        //        Container container = cosmosClient.GetContainer(databaseId, hospitalMaster_container);

        //        var sqlQueryText = "SELECT TOP 1* FROM c WHERE c.bedID = '" + bedID + "' and c.is_active=1 orderby date_time desc";

        //        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        //        FeedIterator<Episode> queryResultSetIterator = container.GetItemQueryIterator<Episode>(queryDefinition);

        //        while (queryResultSetIterator.HasMoreResults)
        //        {
        //            FeedResponse<Episode> currentResultSet = await queryResultSetIterator.ReadNextAsync();

        //            foreach (Episode episode in currentResultSet)
        //            {
        //                latestEpisode = episode;
        //                log.LogInformation("\tRead {0}\n", episode.id);
        //                break;

        //            }
        //        }

        //    }
        //    catch (CosmosException ce)
        //    {
        //        log.LogInformation(ce.Message);
        //    }
        //}
        //private async Task QueryItemsAsync()
        //{
        //    var sqlQueryText = "SELECT * FROM c WHERE c.LastName = 'Andersen'";

        //    Console.WriteLine("Running query: {0}\n", sqlQueryText);

        //    QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        //    FeedIterator<Family> queryResultSetIterator = this.container.GetItemQueryIterator<Family>(queryDefinition);

        //    List<Family> families = new List<Family>();

        //    while (queryResultSetIterator.HasMoreResults)
        //    {
        //        FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync();
        //        foreach (Family family in currentResultSet)
        //        {
        //            families.Add(family);
        //            Console.WriteLine("\tRead {0}\n", family);
        //        }
        //    }
        //}

        public async void InsertDataForConnectionOff(string deviceID, int connectionstatus)
        {
            if (connectionstatus == 0)
            {
                Episode episodeActive = await getActiveEpisodeForBed(deviceID);
                try
                {
                    if (episodeActive.id != null)
                    {
                        Container containerWeightData = cosmosClient.GetContainer(databaseId, weight_container);
                        Position position = await getLatestPositionForBed(deviceID, episodeActive.id);
                        var sqlQueryText = "SELECT TOP 1* FROM c WHERE c.episode_id = '" + episodeActive.id + "' AND c.bedID = '" + deviceID + "' ORDER BY c.unix_date_time DESC";

                        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                        FeedIterator<Weight> queryResultSetIterator = containerWeightData.GetItemQueryIterator<Weight>(queryDefinition);

                        while (queryResultSetIterator.HasMoreResults)
                        {
                            try
                            {
                                FeedResponse<Weight> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                                foreach (Weight weight in currentResultSet)
                                {
                                    Weight newWeight = new Weight();
                                    newWeight.bedID = weight.bedID;
                                    newWeight.episode_id = weight.episode_id;
                                    newWeight.connection_off = connectionstatus == 1 ? 0 : 1;
                                    newWeight.weight_data = weight.weight_data;
                                    newWeight.date_time = DateTime.UtcNow.ToString();
                                    newWeight.unix_date_time = Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
                                    ItemResponse<Weight> newWeightResponse = await containerWeightData.CreateItemAsync<Weight>(newWeight, new PartitionKey(newWeight.bedID));
                                    log.LogInformation("\t Insert weight data for diconnect {0} with id {1}\n", episodeActive.bedID, newWeight.id);
                                }
                            }
                            catch (CosmosException e)
                            {
                                if (e.StatusCode == HttpStatusCode.NotFound)
                                {
                                    break;
                                }
                                else
                                    throw;
                            }

                        }
                        Container containerPosition = cosmosClient.GetContainer(databaseId, turn_container);
                        if (position != null && position.id != null)
                        {

                            Position newPosition = new Position();
                            if (!string.IsNullOrWhiteSpace(position.position_timer))
                                newPosition.position_timer = position.position_timer;
                            if (!string.IsNullOrWhiteSpace(position.position))
                                newPosition.position = position.position;

                            newPosition.position_status = position.position_status;
                            newPosition.bedID = position.bedID;
                            newPosition.episode_id = position.episode_id;
                            DateTime dateTime = DateTime.Now;
                            newPosition.date_time = DateTime.UtcNow.ToString();
                            newPosition.unix_date_time = Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
                            newPosition.connection_off = connectionstatus == 1 ? 0 : 1;
                            ItemResponse<Position> newPositionResponse = await containerPosition.CreateItemAsync<Position>(newPosition, new PartitionKey(newPosition.bedID));
                            log.LogInformation("Insert turn data on disconnect with Device {1} and id", newPosition.bedID, newPosition.id);
                        }
                    }
                }
                catch (CosmosException ce)
                {
                    log.LogError("Insert turn data on disconnect with Device {0}", ce.Message);
                }
                catch (Exception ex)
                {
                    log.LogError("Insert turn data on disconnect with Device {0}", ex.Message);
                }
            }

        }

        private async Task<WeightTrackConfig> getLatestTrackForBed(string bedID, string episodeID, int isTrackOn = 0)
        {
            WeightTrackConfig latestTrackConfig = null;
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, weighttrackconfig_container);
                var sqlQueryTrackOn = "";
                if (isTrackOn == 1)
                {
                    sqlQueryTrackOn = " and c.is_track_on = 1";
                }
                var sqlQueryText = "SELECT TOP 1* FROM c WHERE c.bedID = '" + bedID + "' and c.episode_id='" + episodeID + "'" + sqlQueryTrackOn + " order by c.unix_date_time desc";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<WeightTrackConfig> queryResultSetIterator = container.GetItemQueryIterator<WeightTrackConfig>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<WeightTrackConfig> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (WeightTrackConfig trackConfig in currentResultSet)
                    {
                        latestTrackConfig = trackConfig;
                        log.LogInformation("\tRead {0}\n", trackConfig.id);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError("getLatestTrackForBed {0}", ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("getLatestTrackForBed {0}", ex.Message);
            }
            return latestTrackConfig;
        }
        public async void setWeightTrackConfigForBed(string bedID, string episodeID, int isTrackOn, int angle, double baseWeight, string datetime)
        {
            Container container = cosmosClient.GetContainer(databaseId, weighttrackconfig_container);
            Episode episodeActive = await getActiveEpisodeForBed(bedID);
            if (episodeActive != null)
            {
                WeightTrackConfig trackConfigActive = await getLatestTrackForBed(bedID, episodeActive.id, isTrackOn);
                try
                {
                    WeightTrackConfig newWeightConfig = new WeightTrackConfig();
                    newWeightConfig.bedID = bedID;
                    newWeightConfig.episode_id = episodeActive.id;
                    newWeightConfig.is_track_on = isTrackOn;
                    newWeightConfig.angle = angle;
                    newWeightConfig.base_weight = baseWeight;
                    newWeightConfig.upper_limit = 0;
                    newWeightConfig.lower_limit = 0;
                    if (trackConfigActive == null && isTrackOn == 1)
                    {
                        ItemResponse<WeightTrackConfig> newWeightTrackResponse = await container.CreateItemAsync<WeightTrackConfig>(newWeightConfig, new PartitionKey(newWeightConfig.bedID));
                        log.LogInformation("Inserted Weight Config {0} with track {1}", newWeightConfig.id, newWeightConfig.is_track_on);
                    }
                    else
                    {
                        if (trackConfigActive != null && trackConfigActive.id != null)
                        {
                            newWeightConfig.id = trackConfigActive.id;
                            ItemResponse<WeightTrackConfig> newWeightConfigResponse = await container.ReplaceItemAsync<WeightTrackConfig>(newWeightConfig, trackConfigActive.id, new PartitionKey(newWeightConfig.bedID));
                            log.LogInformation("Updated current Weight Config create {0} with track {1}", trackConfigActive.id, isTrackOn);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.LogError(e.Message);
                }
            }

        }

        public async void updateWeightTrackConfigForBed(string bedID, string episodeID, int isTrackOn, int angle, double baseWeight, double upperLimit, double lowerLimit, string datetime)
        {
            Episode episodeActive = await getActiveEpisodeForBed(bedID);
            WeightTrackConfig trackConfigActive = await getLatestTrackForBed(bedID, episodeActive.id, isTrackOn);
            if (trackConfigActive != null)
            {
                Container container = cosmosClient.GetContainer(databaseId, weighttrackconfig_container);

                try
                {
                    WeightTrackConfig newWeightConfig = new WeightTrackConfig();
                    newWeightConfig.bedID = bedID;
                    newWeightConfig.episode_id = episodeActive.id;
                    newWeightConfig.is_track_on = isTrackOn;
                    newWeightConfig.angle = angle;
                    newWeightConfig.base_weight = baseWeight;
                    newWeightConfig.id = trackConfigActive.id;

                    WeightTrackLimit latestWeightTrackLimit = await getLatestWeightTrackLimits(bedID, episodeID, trackConfigActive.id);

                    if (latestWeightTrackLimit == null || upperLimit != latestWeightTrackLimit.upper_limit || lowerLimit != latestWeightTrackLimit.lower_limit)
                    {
                        Container limitContainer = cosmosClient.GetContainer(databaseId, weighttracklimit_container);
                        WeightTrackLimit newWeightTrackLimit = new WeightTrackLimit();
                        newWeightTrackLimit.bedID = bedID;
                        newWeightTrackLimit.episode_id = episodeActive.id;
                        newWeightTrackLimit.upper_limit = upperLimit;
                        newWeightTrackLimit.lower_limit = lowerLimit;
                        newWeightTrackLimit.configuration_id = trackConfigActive.id;
                        ItemResponse<WeightTrackLimit> newWeightTrackLimitResponse = await limitContainer.CreateItemAsync<WeightTrackLimit>(newWeightTrackLimit, new PartitionKey(newWeightTrackLimit.bedID));
                        log.LogInformation("Inserted new Weight track limit {0} with bed {1} with configuration {2}", newWeightTrackLimit.id, newWeightConfig.bedID, trackConfigActive.id);
                    }
                    ItemResponse<WeightTrackConfig> newWeightConfigResponse = await container.ReplaceItemAsync<WeightTrackConfig>(newWeightConfig, trackConfigActive.id, new PartitionKey(newWeightConfig.bedID));
                    log.LogInformation("Updated new Weight Configuration {0} with bed {1}", newWeightConfig.id, newWeightConfig.bedID);
                }
                catch (Exception e)
                {
                    log.LogError(e.Message);
                }
            }
        }
        public async void insertWeightTrackForBed(string bedID, string episodeID, double weightDifference, string datetime)
        {
            Episode episodeActive = await getActiveEpisodeForBed(bedID);
            if (episodeActive != null)
            {
                WeightTrackConfig trackConfigActive = await getLatestTrackForBed(bedID, episodeActive.id, 1);
                if (trackConfigActive != null)
                {
                    WeightTrackLimit weightTrackLimitActive = await getLatestWeightTrackLimits(bedID, episodeActive.id, trackConfigActive.id);
                    if (weightTrackLimitActive == null)
                    {
                        try
                        {
                            Container limitContainer = cosmosClient.GetContainer(databaseId, weighttracklimit_container);
                            WeightTrackLimit newWeightTrackLimit = new WeightTrackLimit();
                            newWeightTrackLimit.bedID = bedID;
                            newWeightTrackLimit.episode_id = episodeActive.id;
                            newWeightTrackLimit.upper_limit = 0;
                            newWeightTrackLimit.lower_limit = 0;
                            newWeightTrackLimit.configuration_id = trackConfigActive.id;
                            ItemResponse<WeightTrackLimit> newWeightTrackLimitResponse = await limitContainer.CreateItemAsync<WeightTrackLimit>(newWeightTrackLimit, new PartitionKey(newWeightTrackLimit.bedID));
                            weightTrackLimitActive = newWeightTrackLimit;
                            log.LogInformation("Added new Weight Track Limit {0} with bed {1}", newWeightTrackLimit.id, newWeightTrackLimit.bedID);
                        }
                        catch (Exception e)
                        {
                            log.LogInformation(e.Message);
                        }
                    }
                    Container container = cosmosClient.GetContainer(databaseId, weighttrack_container);
                    try
                    {
                        WeightTrack newWeightTrack = new WeightTrack();

                        newWeightTrack.bedID = bedID;
                        newWeightTrack.configuration_id = trackConfigActive.id;
                        newWeightTrack.episode_id = episodeActive.id;
                        newWeightTrack.track_limit_id = weightTrackLimitActive.id;
                        newWeightTrack.weight_difference = weightDifference;
                        ItemResponse<WeightTrack> newEpisodeResponse = await container.CreateItemAsync<WeightTrack>(newWeightTrack, new PartitionKey(newWeightTrack.bedID));
                        log.LogInformation("Added new Weight Track Change {0} with bed {1} and limit {2}", newWeightTrack.id, newWeightTrack.bedID, newWeightTrack.track_limit_id);
                    }
                    catch (Exception e)
                    {
                        log.LogError(e.Message);
                    }
                }
            }
        }

        public async Task<WeightTrackLimit> getLatestWeightTrackLimits(string bedID, string episodeID, string configId)
        {
            WeightTrackLimit latestTrackLimit = null;
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, weighttracklimit_container);

                var sqlQueryText = "SELECT TOP 1* FROM c WHERE c.bedID = '" + bedID + "' and c.episode_id='" + episodeID + "' and c.configuration_id='" + configId + "' order by c.unix_date_time desc";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<WeightTrackLimit> queryResultSetIterator = container.GetItemQueryIterator<WeightTrackLimit>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<WeightTrackLimit> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (WeightTrackLimit trackLimit in currentResultSet)
                    {
                        latestTrackLimit = trackLimit;
                        log.LogInformation("\tRead {0}\n", trackLimit.id);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            return latestTrackLimit;
        }

        /**
         * Insert weight data with sensor values and message version
         **/
        public async void InsertGetWeightWithSensorForBed(string bedID, string episodeID, float weightData, double unixdatetime = 0, dynamic sensorData = null, dynamic additionalWeightData = null)
        {
            Container container = cosmosClient.GetContainer(databaseId, weight_container);
            try
            {
                WeightSensor weight = new WeightSensor();
                weight.episode_id = episodeID;
                weight.bedID = bedID;
                weight.weight_data = weightData;
                weight.connection_off = 0;
                if (sensorData != null && additionalWeightData != null && Convert.ToDouble(additionalWeightData.messageversion) >= 1.1)
                {
                    if (unixdatetime != 0)
                    {
                        weight.unix_date_time = unixdatetime;
                    }
                    weight.date_time = additionalWeightData.date_time;
                    weight.wp1 = new Plank()
                    {

                        BedAngle = sensorData["wp1"].BedAngle,
                        BedPitch = sensorData["wp1"].BedPitch,
                        BedRoll = sensorData["wp1"].BedRoll,
                        WC1 = sensorData["wp1"].WC1,
                        WC2 = sensorData["wp1"].WC2,
                        WC3 = sensorData["wp1"].WC3,
                        WC4 = sensorData["wp1"].WC4
                    };
                    weight.wp2 = new Plank()
                    {
                        BedAngle = sensorData["wp2"].BedAngle,
                        BedPitch = sensorData["wp2"].BedPitch,
                        BedRoll = sensorData["wp2"].BedRoll,
                        WC1 = sensorData["wp2"].WC1,
                        WC2 = sensorData["wp2"].WC2,
                        WC3 = sensorData["wp2"].WC3,
                        WC4 = sensorData["wp2"].WC4
                    };
                    weight.wp3 = new Plank()
                    {
                        BedAngle = sensorData["wp3"].BedAngle,
                        BedPitch = sensorData["wp3"].BedPitch,
                        BedRoll = sensorData["wp3"].BedRoll,
                        WC1 = sensorData["wp3"].WC1,
                        WC2 = sensorData["wp3"].WC2,
                        WC3 = sensorData["wp3"].WC3,
                        WC4 = sensorData["wp3"].WC4
                    };
                    weight.wp4 = new Plank()
                    {
                        BedAngle = sensorData["wp4"].BedAngle,
                        BedPitch = sensorData["wp4"].BedPitch,
                        BedRoll = sensorData["wp4"].BedRoll,
                        WC1 = sensorData["wp4"].WC1,
                        WC2 = sensorData["wp4"].WC2,
                        WC3 = sensorData["wp4"].WC3,
                        WC4 = sensorData["wp4"].WC4
                    };
                    weight.devicetype = additionalWeightData.devicetype;
                    weight.deviceversion = additionalWeightData.deviceversion;
                    weight.hospitalcode = additionalWeightData.hospitalcode;
                    weight.messagetype = additionalWeightData.messagetype;
                    weight.messageversion = additionalWeightData.messageversion;
                }
                ItemResponse<WeightSensor> newEpisodeResponse = await container.UpsertItemAsync<WeightSensor>(weight, new PartitionKey(weight.bedID));
                string eventType = "GetWeight";
                //change date time to date time of bedside display
                DateTime datetimeofUsuage = DateTime.UtcNow;
                InsertUsuageEventForBed(bedID, datetimeofUsuage.ToString(), eventType);

                log.LogInformation("Inserted new weight {0} with bed {1}", weight.id, weight.bedID);
            }
            catch (Exception e)
            {
                log.LogError("ERROR while inserting weight with sensor: {0}", e.Message);
            }
        }

        public async void InsertDeviceTelemetryData(dynamic deviceTelemetryObj, string messagetype)
        {
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, devicetelemetry_container);

                DeviceTelemetry newDeviceTelemetryData = new DeviceTelemetry();
                newDeviceTelemetryData.bedID = Convert.ToString(deviceTelemetryObj.bedID);
                newDeviceTelemetryData.episodeID = Convert.ToString(deviceTelemetryObj.episodeID);
                newDeviceTelemetryData.bed_status = Convert.ToInt32(deviceTelemetryObj.bed_status);
                newDeviceTelemetryData.datetime = DateTime.UtcNow.ToString();
                //newDeviceTelemetryData.unixdatetime = Math.Floor((Convert.ToDateTime(DateTime.UtcNow) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
                newDeviceTelemetryData.message_type = messagetype;
                newDeviceTelemetryData.unixdatetime = deviceTelemetryObj.unixDateTime;
                log.LogInformation("unixdatetime", newDeviceTelemetryData.unixdatetime);
                ItemResponse<DeviceTelemetry> newDeviceTelemetryResponse = await container.CreateItemAsync<DeviceTelemetry>(newDeviceTelemetryData, new PartitionKey(newDeviceTelemetryData.bedID));
                log.LogInformation("Inserted new device telemetry {0} with bed {1}", newDeviceTelemetryData.id, newDeviceTelemetryData.bedID);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
            }
        }

        public async Task<IList<ICUBed>> GetDevicesByTablet(string tabletid)
        {
            List<ICUBed> deviceList = new List<ICUBed>();
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, devices_container);

                var sqlQueryText = "SELECT *  FROM c WHERE c.tablet_id = '" + tabletid + "' and c.connection_status = 1";
                log.LogInformation("sqlQueryText {0}", sqlQueryText);
                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<ICUBed> queryResultSetIterator = container.GetItemQueryIterator<ICUBed>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<ICUBed> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (ICUBed device in currentResultSet)
                    {
                        deviceList.Add(device);
                    }
                }                
            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            log.LogInformation("deviceList {0}", deviceList);
            return deviceList;
        }


        public async void removeBedStatus(string deviceId, string tabletId)
        {
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, devices_container);
                ICUBed bed = await getICUBedForDeviceForRemove(deviceId, tabletId);
                if (bed != null)
                {
                    bed.icu_id = "";
                    bed.bed_name = "";
                    bed.connection_status = 0; //Bed is removed from tablet.
                    ItemResponse<ICUBed> deviceBedResponse = await container.UpsertItemAsync<ICUBed>(bed, new PartitionKey(bed.serial_number));
                    log.LogInformation("Updated bed Information with id {0} and Device {1} with connection status 2", bed.id, bed.serial_number);
                }

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        public async Task<ICUBed> getICUBedForDeviceForRemove(string deviceID, string tabletId)
        {
            ICUBed Bed = null;
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, devices_container);

                var sqlQueryText = "SELECT TOP 1*  FROM c WHERE c.serial_number = '" + deviceID + "' and c.tablet_id = '" + tabletId + "' ";
                log.LogInformation("Query {0}", sqlQueryText);
                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<ICUBed> queryResultSetIterator = container.GetItemQueryIterator<ICUBed>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<ICUBed> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (ICUBed icuBed in currentResultSet)
                    {
                        log.LogInformation("icuBed {0}", icuBed);
                        Bed = icuBed;
                        log.LogInformation("\tRead from device {0}\n", Bed.serial_number);
                        break;

                    }
                }

            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
            return Bed;
        }

        public async Task<IList<Calibration>> getCalibratedRecordForDevice(string deviceID, int type)
        {
            IList<Calibration> calibratedList = new List<Calibration>(); ;
            try
            {

                Container container = cosmosClient.GetContainer(databaseId, calibration_container);

                var sqlQueryText = "SELECT * FROM c WHERE c.deviceId = '" + deviceID + "' ";
                            
                log.LogInformation("Query {0}", sqlQueryText);
                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Calibration> queryResultSetIterator = container.GetItemQueryIterator<Calibration>(queryDefinition);
                if (queryResultSetIterator != null)
                {
                    log.LogInformation("Inside If queryResultSetIterator", queryResultSetIterator);
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        log.LogInformation("Inside while", queryResultSetIterator.HasMoreResults);
                        FeedResponse<Calibration> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        log.LogInformation("currentResultSet {0}", currentResultSet);
                        if (currentResultSet != null)
                        {
                            foreach (Calibration calibrate in currentResultSet)
                            {
                                log.LogInformation("calibrate {0}", calibrate);
                                calibratedList.Add(calibrate);
                                if (type == 1)
                                {
                                    ItemResponse<Calibration> deleteResponse = await container.DeleteItemAsync<Calibration>(calibrate.id, new PartitionKey(calibrate.deviceId));
                                    log.LogInformation("Deleted bed with bed id {0} and serial number {1}", calibrate.id, calibrate.deviceId);
                                }                                
                            }
                        }

                    }
                }
               
            }
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
            log.LogInformation("Already Data in Calibration Container List {0}", calibratedList.Count);
            return calibratedList;
        }


        /**
        * Insert calibration data with plank values
        **/
        public async void InsertCalibrationDataForDevice(string eventname, string deviceID, string episodeID, bool calibrationStatus, double unix_date_time = 0, dynamic calibrationData = null)
        {
            Container container = cosmosClient.GetContainer(databaseId, calibration_container);
            Container archivecontainer = cosmosClient.GetContainer(databaseId, calibration_history_container);
            try
            {
                Calibration calibration = new Calibration();
                calibration.eventName = eventname;
                calibration.episodeId = episodeID;
                calibration.deviceId = deviceID;
                calibration.deviceRegistrationDate = Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds); 
                calibration.calibrationStatus = calibrationStatus;
                IList<Calibration> calibrationList = await getCalibratedRecordForDevice(deviceID, 1);
                if (calibrationList.Count == 0)
                {
                    calibration.deviceRegistrationDate = Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds); ;
                }
                else
                {
                    foreach (Calibration calibrationdata in calibrationList)
                    {
                        calibration.deviceRegistrationDate = calibrationdata.deviceRegistrationDate;
                        log.LogInformation("Inserting old data in archive container {0}", calibrationdata);
                        ItemResponse<Calibration> calibrationArchiveResponse = await archivecontainer.CreateItemAsync<Calibration>(calibrationdata, new PartitionKey(calibrationdata.deviceId));
                        log.LogInformation("Inserted new calibration in archive container {0} with device {1}", calibrationdata.id, calibrationdata.deviceId);
                    }
                }

                if (calibrationData != null)
                {
                    if (unix_date_time != 0)
                    {
                        calibration.unix_date_time = unix_date_time;
                    }
                    foreach (KeyValuePair<string, CalibrationPlank> ele in calibrationData)
                    {
                        log.LogInformation("Key = {0}, Value = {1} {2}", ele.Key, ele.Value, ele.Value.WeightBeforeCalibration);
                        Console.WriteLine("Key = {0}, Value = {1}", ele.Key, ele.Value);
                        if (ele.Key == "headPlank")
                        {
                            calibration.headPlank = new CalibrationPlank()
                            {
                                FactorRatio = ele.Value.FactorRatio,
                                FactorConstant = ele.Value.FactorConstant,
                                WeightBeforeCalibration = ele.Value.WeightBeforeCalibration
                            };
                        }
                        if (ele.Key == "abdomenPlank")
                        {
                            calibration.abdomenPlank = new CalibrationPlank()
                            {
                                FactorRatio = ele.Value.FactorRatio,
                                FactorConstant = ele.Value.FactorConstant,
                                WeightBeforeCalibration = ele.Value.WeightBeforeCalibration
                            };
                        }
                        if (ele.Key == "thighPlank")
                        {
                            calibration.thighPlank = new CalibrationPlank()
                            {
                                FactorRatio = ele.Value.FactorRatio,
                                FactorConstant = ele.Value.FactorConstant,
                                WeightBeforeCalibration = ele.Value.WeightBeforeCalibration
                            };

                        }
                        if (ele.Key == "footPlank")
                        {
                            calibration.footPlank = new CalibrationPlank()
                            {
                                FactorRatio = ele.Value.FactorRatio,
                                FactorConstant = ele.Value.FactorConstant,
                                WeightBeforeCalibration = ele.Value.WeightBeforeCalibration
                            };

                        }
                    }                   
                }
                ItemResponse<Calibration> calibrationResponse = await container.CreateItemAsync<Calibration>(calibration, new PartitionKey(calibration.deviceId));
                log.LogInformation("Inserted new calibration {0} with bed {1}", calibration.id, calibration.deviceId);
            }
            catch (Exception e)
            {
                log.LogError("ERROR while inserting calibration data: {0}", e.Message);
            }
        }
    }
}
