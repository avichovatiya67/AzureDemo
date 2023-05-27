using Microsoft.Azure.Cosmos;

using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Stryker.SmartMedic.Models;

namespace BedSide_API_Functions
{
    public class CosmosDB
    {
        //private string dbConnectionString = Environment.GetEnvironmentVariable("smartmedicDBConnection");

        private static Lazy<CosmosClient> lazyClient = new Lazy<CosmosClient>(
            InitializeCosmosClient
        );
        private static CosmosClient cosmosClient => lazyClient.Value;

        private static CosmosClient InitializeCosmosClient()
        {
            string dbConnectionString = Environment.GetEnvironmentVariable(
                "smartmedicDBConnection"
            );
            return new CosmosClient(dbConnectionString);
        }

        // The Cosmos client instance
        //private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        //private Container container;

        // The name of the database and container we will create
        //  private string databaseId = "smartmedic-db";
        private string databaseId = Environment.GetEnvironmentVariable("databaseId");
        private string icu_containerId = "icu_master";
        private string bed_containerId = "bed_master";
        private string hospital_containerId = "hospital_master";

        private ILogger log = null;

        public CosmosDB(ILogger logger)
        {
            log = logger;
            // Create a new instance of the Cosmos Client

            log.LogInformation("Inside cosmos class");
            //cosmosClient = new CosmosClient(dbConnectionString);
        }

        public CosmosDB(CosmosClient client, ILogger logger)
        {
            log = logger;
            // Create a new instance of the Cosmos Client

            log.LogInformation("Inside cosmos class");
            //cosmosClient = client;
        }

        public async Task<bool> IsHospitalCodeExists(string hospital_code)
        {
            bool hospitalcodeExists = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(hospital_code))
                {
                    this.database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                    log.LogInformation("Created Database: {0}\n", this.database.Id);

                    string hospitalID = await getHospitalIDFromHospitalCode(hospital_code);

                    if (!string.IsNullOrWhiteSpace(hospitalID))
                    {
                        hospitalcodeExists = true;
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
            return hospitalcodeExists;
        }

        public async Task<List<ICU>> getICUListForHospital(string hospital_code)
        {
            List<ICU> icuList = new List<ICU>();
            try
            {
                if (!string.IsNullOrWhiteSpace(hospital_code))
                {
                    this.database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                    log.LogInformation("Created Database: {0}\n", this.database.Id);

                    string hospitalID = await getHospitalIDFromHospitalCode(hospital_code);

                    Container container = this.database.GetContainer(icu_containerId);

                    var sqlQueryText =
                        "SELECT c.icu_name,c.id FROM c WHERE c.hospital_id = '" + hospitalID + "'";

                    Console.WriteLine("Running query: {0}\n", sqlQueryText);

                    QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                    FeedIterator<ICU> queryResultSetIterator = container.GetItemQueryIterator<ICU>(
                        queryDefinition
                    );
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<ICU> currentResultSet =
                            await queryResultSetIterator.ReadNextAsync();
                        foreach (ICU icu in currentResultSet)
                        {
                            icuList.Add(icu);
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
            return icuList;
        }

        private async Task<string> getHospitalIDFromHospitalCode(string hospitalCode)
        {
            string hospitalID = String.Empty;
            Hospital latestHospital;
            try
            {
                Container container = cosmosClient.GetContainer(databaseId, hospital_containerId);

                var sqlQueryText = "SELECT * FROM c WHERE c.hospital_code = '" + hospitalCode + "'";

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                FeedIterator<Hospital> queryResultSetIterator =
                    container.GetItemQueryIterator<Hospital>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Hospital> currentResultSet;
                    try
                    {
                        currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (Hospital hospital in currentResultSet)
                        {
                            latestHospital = hospital;
                            hospitalID = hospital.id;
                            log.LogInformation("\tRead {0}\n", hospital.id);
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
            catch (CosmosException ce)
            {
                log.LogError(ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
            return hospitalID;
        }

        public async Task<List<BedData>> getBedListForHospital(string icu_id)
        {
            List<BedData> bedList = new List<BedData>();
            try
            {
                if (!string.IsNullOrWhiteSpace(icu_id))
                {
                    this.database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                    log.LogInformation("Created Database: {0}\n", this.database.Id);

                    Container container = this.database.GetContainer(bed_containerId);

                    var sqlQueryText =
                        "SELECT c.bed_name,c.id, c.icu_id,c.is_active FROM c WHERE c.icu_id = '"
                        + icu_id
                        + "'";

                    Console.WriteLine("Running query: {0}\n", sqlQueryText);

                    QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                    FeedIterator<BedData> queryResultSetIterator =
                        container.GetItemQueryIterator<BedData>(queryDefinition);
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<BedData> currentResultSet =
                            await queryResultSetIterator.ReadNextAsync();
                        foreach (BedData bed in currentResultSet)
                        {
                            bedList.Add(bed);
                        }
                    }
                }
            }
            catch (CosmosException ce)
            {
                log.LogError("getBedListForHospital {0}", ce.Message);
            }
            catch (Exception ex)
            {
                log.LogError("getBedListForHospital {0}", ex.Message);
            }
            return bedList;
        }
    }
}
