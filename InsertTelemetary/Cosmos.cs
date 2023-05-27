using Microsoft.Azure.Cosmos;

using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stryker.SmartMedic.Models;

using Microsoft.Extensions.Logging;
namespace InsertTelemetary
{
    public class Cosmos
    {


        //private static string dbConnectionString = Environment.GetEnvironmentVariable("smartmedicDBConnection");

        private static Lazy<CosmosClient> lazyClient = new Lazy<CosmosClient>(InitializeCosmosClient);
        private static CosmosClient cosmosClient => lazyClient.Value;

        private static CosmosClient InitializeCosmosClient()
        {
            string dbConnectionString = Environment.GetEnvironmentVariable("smartmedicDBConnection");
            return new CosmosClient(dbConnectionString);
        }

        //// The Cosmos client instance
        //private static CosmosClient cosmosClient = new CosmosClient(dbConnectionString);

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        // private string databaseId = "smartmedic-db";
        private string databaseId = Environment.GetEnvironmentVariable("databaseId");
        private string containerId = "device-telemetary";
        private TelemetryDataPoint telemetarydataPoint = null;
        private dynamic eventDataPoint = null;

        private ILogger log = null;
        public Cosmos(ILogger logger, dynamic dynObj, string eventType = "")
        {
            logger.LogInformation("Init {0}", eventType);
            if(string.IsNullOrEmpty(eventType))
            {
                initDataPoint(dynObj, "");
            } else
            {
                initDataPoint(dynObj, eventType);
            }
            
        }

        public async void ExecuteDBTask(ILogger logger, string eventType = "")
        {
            log = logger;
            // Create a new instance of the Cosmos Client
            //cosmosClient = new CosmosClient(dbConnectionString);
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            log.LogInformation("execute : {0}", eventType);
            if (string.IsNullOrEmpty(eventType))
            {
                await this.AddItemsByMessageTypeAsync("");
            } else
            {
                await this.AddItemsByMessageTypeAsync(eventType);
            }
            log.LogInformation("Event type: {0}\n", eventType);
            // cosmosClient.Dispose();
        }
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            log.LogInformation("Created Database: {0}\n", this.database.Id);
        }

        /// Create the container if it does not exist. 
        /// Specifiy "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
        private async Task CreateContainerAsync()
        {
            try
            {
                // Create a new container
                this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/bedID");
                log.LogInformation("Created Container: {0}\n", this.container.Id);
            }
            catch (CosmosException ex)
            {
                log.LogError("Exception: {0} ", ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError("Exception: {0} ",ex.Message);
            }
        }
        private async Task AddItemsToContainerAsync()
        {
            try
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen".
                ItemResponse<TelemetryDataPoint> telemetarydataResponse = await this.container.CreateItemAsync<TelemetryDataPoint>(telemetarydataPoint, new PartitionKey(telemetarydataPoint.bedID));
                 log.LogInformation("Created item in database with id: {0} Operation consumed {1} RUs.\n", telemetarydataResponse.Resource.bedID, telemetarydataResponse.RequestCharge);
            }
            catch (CosmosException ex) 
            {
                log.LogError("Item in database with id: {0} already exists {1}\n", telemetarydataPoint.bedID,ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError("Error in AddItemsToContainerAsync {0}\n", ex.Message);
            }
        }

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
        private async Task AddItemsByMessageTypeAsync(string eventType)
        {
            try
            {
                log.LogInformation("Add item in: {0}\n", eventType);
                switch (eventType)
                {
                    case "OverrideTare-Message":
                        {
                            ItemResponse<OverrideTareDataPoint> telemetarydataResponse = await this.container.CreateItemAsync<OverrideTareDataPoint>(eventDataPoint, new PartitionKey(eventDataPoint.bedID));
                            log.LogInformation("Created item in database with id: {0} and BedID {1} Operation consumed {2} RUs.\n", telemetarydataResponse.Resource.id, telemetarydataResponse.Resource.bedID, telemetarydataResponse.RequestCharge);
                            break;
                        }
                    case "PatientStatus-Message":
                        {
                            ItemResponse<PatientStatusDataPoint> telemetarydataResponse = await this.container.CreateItemAsync<PatientStatusDataPoint>(eventDataPoint, new PartitionKey(eventDataPoint.bedID));
                            log.LogInformation("Created item in database with id: {0} Operation consumed {1} RUs.\n", telemetarydataResponse.Resource.bedID, telemetarydataResponse.RequestCharge);
                            break;
                        }
                    case "start-Message":
                        {
                            ItemResponse<StartDataPoint> telemetarydataResponse = await this.container.CreateItemAsync<StartDataPoint>(eventDataPoint, new PartitionKey(eventDataPoint.bedID));
                            log.LogInformation("Created item in database with id: {0} Operation consumed {1} RUs.\n", telemetarydataResponse.Resource.bedID, telemetarydataResponse.RequestCharge);
                            break;
                        }
                    case "Warning-Message":
                        {
                            ItemResponse<WarningDataPoint> telemetarydataResponse = await this.container.CreateItemAsync<WarningDataPoint>(eventDataPoint, new PartitionKey(eventDataPoint.bedID));
                            log.LogInformation("Created item in database with id: {0} Operation consumed {1} RUs.\n", telemetarydataResponse.Resource.bedID, telemetarydataResponse.RequestCharge);
                            break;
                        }
                    case "Tare-Message":
                        {
                            ItemResponse<TareDataPoint> telemetarydataResponse = await this.container.CreateItemAsync<TareDataPoint>(eventDataPoint, new PartitionKey(eventDataPoint.bedID));
                            log.LogInformation("Created item in database with id: {0} Operation consumed {1} RUs.\n", telemetarydataResponse.Resource.bedID, telemetarydataResponse.RequestCharge);
                            break;
                        }
                    default:
                        {
                            // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen".
                            ItemResponse<TelemetryDataPoint> telemetarydataResponse = await this.container.CreateItemAsync<TelemetryDataPoint>(telemetarydataPoint, new PartitionKey(telemetarydataPoint.bedID));
                            log.LogInformation("Created item in database with id: {0} Operation consumed {1} RUs.\n", telemetarydataResponse.Resource.bedID, telemetarydataResponse.RequestCharge);
                            break;
                        }
                }
            }
            catch (CosmosException ex)
            {
                log.LogError("Exception message : {0}\n", ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError("Error in AddItemsByMessageTypeAsync {0}\n", ex.Message);
            }
        }

        private void initDataPoint(dynamic dynobj, string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
            {
                telemetarydataPoint = new TelemetryDataPoint(dynobj);
            }
            else
            {

                switch (eventType)
                {
                    case "OverrideTare-Message":
                        {
                            eventDataPoint = new OverrideTareDataPoint(dynobj);
                            break;
                        }
                    case "PatientStatus-Message":
                        {
                            eventDataPoint = new PatientStatusDataPoint(dynobj);
                            break;
                        }
                    case "start-Message":
                        {
                            eventDataPoint = new StartDataPoint(dynobj);
                            break;
                        }
                    case "Warning-Message":
                        {
                            eventDataPoint = new WarningDataPoint(dynobj);
                            break;
                        }
                    case "Tare-Message":
                        {
                            eventDataPoint = new TareDataPoint(dynobj);
                            break;
                        }
                    default:
                        {
                            telemetarydataPoint = new TelemetryDataPoint(dynobj);
                            break;
                        }
                }
            }
        }
    }
}
