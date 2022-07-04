using Azure;
using Azure.Data.Tables;
using IoTGatewayAPI.Contracts;

namespace IoTGatewayAPI.Services
{
    public class AzureTableRepository : IAzureTableRepository<IotDeviceTableEntity>
    {
        private string _connectionString;
        private string _tableName;


        private TableServiceClient _tableServiceClient;
        public AzureTableRepository(string connectionString, string tableNamePrefix)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException("connectionString", "connectionString is null");
            _tableName = tableNamePrefix ?? throw new ArgumentNullException("tablePrefix", "tablePrefix is null.");
            _tableServiceClient = new TableServiceClient(_connectionString);
        }

        public async Task CreateOrUpdate(IotDeviceTableEntity record)
        {
            await _tableServiceClient.GetTableClient(_tableName).CreateIfNotExistsAsync();

            Response response = null;
            try
            {
                response = await _tableServiceClient.GetTableClient(_tableName).AddEntityAsync(record);
            }
            catch (Exception ex)
            {
                response = await _tableServiceClient.GetTableClient(_tableName).UpdateEntityAsync(record, record.ETag);
            }
            Console.WriteLine(response);
        }


        public async Task<IotDeviceTableEntity> Get(string id)
        {
            var filter = $"PartitionKey eq '{id}' ";
            AsyncPageable<IotDeviceTableEntity> query =  _tableServiceClient.GetTableClient(_tableName).QueryAsync<IotDeviceTableEntity>(filter: filter);

             List<IotDeviceTableEntity> entities = new List<IotDeviceTableEntity>();    

            await foreach (IotDeviceTableEntity item in query)
            {
                entities.Add(item);
            }
            //IotDeviceTableEntity entity = query.Select(x=>x).FirstOrDefault();
           
            
            return entities.FirstOrDefault();

           
        }
    }
}


/*{
    "deviceName": "Sanjo Mostar",
    "body": null,
    "partitionKey": "00000000-0000-0000-0000-000000000027",
    "rowKey": "00000000-0000-0000-0000-000000000027",
    "timestamp": "2022-05-26T09:51:41.0876672+00:00",
    "eTag": {}
}*/