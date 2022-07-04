using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json.Linq;
using System;

namespace IoTGatewayAPI
{
    public class IotDeviceTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; } = new ETag("*");
        public string DeviceName { get; set; }
        public string Body { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.Now;
        public IotDeviceTableEntity() { }

        public IotDeviceTableEntity(Guid deviceId, string deviceName, string body, string lat, string lon, DateTimeOffset timestamp)
        {
            DeviceName = deviceName;
            Body = body;
            PartitionKey = deviceId.ToString();
            RowKey = timestamp != null ? timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fff") : DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            Lat = lat;
            Lon = lon;
        }
    }
    public class IotDevice
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string Body { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }

        public IotDevice()
        {
        }
    }
}