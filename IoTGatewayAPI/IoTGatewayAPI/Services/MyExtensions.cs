namespace IoTGatewayAPI.Services
{
    public static class MyExtensions
    {
        public static IotDeviceTableEntity ToTableEntity(this IotDevice record)
        {
            var IoTDeviceEntity = new IotDeviceTableEntity() {
                DeviceName = record.DeviceName,
                PartitionKey = record.DeviceId,
                RowKey = record.DeviceId.ToString(),
                Body = record.Body,
                Lat = record.Lat,
                Lon = record.Lon
            };
            return IoTDeviceEntity;
        }

        
        public static List<IotDeviceTableEntity> ToTableEntity(this List<IotDevice> record)
        {
            return record.Select(x => x.ToTableEntity()).ToList();
        }

        public static IotDevice ToModel(this IotDeviceTableEntity record)
        {
            var IoTDeviceEntity = new IotDevice()
            {
                DeviceName = record.DeviceName,
                DeviceId= record.PartitionKey,
                Body = record.Body,
                Lat = record.Lat,
                Lon = record.Lon
            };
            return IoTDeviceEntity;
        }


        public static List<IotDevice> ToModel(this List<IotDeviceTableEntity> record)
        {
            return record.Select(x => x.ToModel()).ToList();
        }
    }
}
