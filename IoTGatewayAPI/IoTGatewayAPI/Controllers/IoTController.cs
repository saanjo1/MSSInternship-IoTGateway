using IoTGatewayAPI.Contracts;
using IoTGatewayAPI.RabbitMQ;
using IoTGatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace IoTGatewayAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class IoTController : ControllerBase
    {
        private readonly IMessageProducer _messageProducer;
        private IAzureTableRepository<IotDeviceTableEntity> azureTableRepository;
        public IoTController(IMessageProducer messageProducer, IAzureTableRepository<IotDeviceTableEntity> _azureTableRepository)
        {
            _messageProducer = messageProducer;
            azureTableRepository = _azureTableRepository;
        }
        private static CloudTable TableMethod()
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureTableStorage");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(Environment.GetEnvironmentVariable("TableName"));
            table.CreateIfNotExists();
            return table;
        }

        [HttpPost]
        [RequireCustomHeader("SecurityKey")]
        public async Task<bool> Post([FromBody] AddDeviceObject record)
        {
            string _deviceName = Request.Headers["DeviceName"];


            //IotDeviceTableEntity device = new IotDeviceTableEntity(body, deviceId, lat, lon)
            IotDevice device = new IotDevice()
            {
                DeviceName = _deviceName,
                DeviceId = record.DeviceId,
                Body = record.Body,
                Lat = record.Lat,
                Lon = record.Lon
            };

            var sendMessageResponse = await _messageProducer.SendMessage<IotDevice>(device);
            await azureTableRepository.CreateOrUpdate(device.ToTableEntity());
            return await Task.FromResult(true);
        }


        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            var response = await azureTableRepository.Get(id);
            return Ok(response);
        }

    }

    public class AddDeviceObject{

        public string DeviceId { get; set; }
        public dynamic Body { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }

    }
}
