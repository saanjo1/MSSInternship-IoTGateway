using IoTGatewayAPI;
using IoTGatewayAPI.Contracts;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using IoTGatewayAPI.Services;
using System.Collections.Concurrent;

namespace IoT_ConsoleApp
{
    class Recieve
    {
        private static string _hostname = Environment.GetEnvironmentVariable("HostName");
        private static string _username = Environment.GetEnvironmentVariable("UserName");
        private static string _password = Environment.GetEnvironmentVariable("Password");
        private static int _port = Convert.ToInt32(Environment.GetEnvironmentVariable("Port"));
        private static string _connectionString = Environment.GetEnvironmentVariable("AzureTableStorage");
        private static string _tableName = Environment.GetEnvironmentVariable("TableName");
        private static string _queueName = Environment.GetEnvironmentVariable("QueueName");
        private static int _sleepTime = Convert.ToInt32(Environment.GetEnvironmentVariable("SleepTime"));

        private static IAzureTableRepository<IotDeviceTableEntity> azureTableRepository;
        private static ConcurrentQueue<IotDevice> msgqueue;
        public static async Task Main()
        {
            Console.WriteLine("Hello from Recieve.cs");
            msgqueue = new ConcurrentQueue<IotDevice>();
            do
            {

                try
                {
                 
                    azureTableRepository = new AzureTableRepository(_connectionString, _tableName);
                    Console.WriteLine("Entering try catch");
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    CloudTable table = tableClient.GetTableReference(_tableName);
                    table.CreateIfNotExistsAsync();

                    string message = "";
                    ConnectionFactory factory = new ConnectionFactory { HostName = _hostname, UserName = _username, Password = _password, Port = _port };
                    IConnection connection = factory.CreateConnection();
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: _queueName,
                                              durable: false,
                                              exclusive: false,
                                              autoDelete: false,
                                              arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, eventArgs) =>
                    {
                        IDictionary<string, object> header = eventArgs.BasicProperties.Headers;
                        if(header.TryGetValue("DeviceName", out var deviceNameHeader))
                        {
                            var takenHeader = deviceNameHeader;
                            var stringDeviceName = (Byte[])takenHeader;
                            message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                            IotDevice device = JsonConvert.DeserializeObject<IotDevice>(message);
                            device.DeviceName = Encoding.UTF8.GetString(stringDeviceName);
                            msgqueue.Enqueue(device);
                            Console.WriteLine(message);
                        }
                       else
                        {
                            Console.WriteLine("Could not get deviceName from Header");
                        }
                    };

                    channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

                   var _devices = new List<IotDevice>();

                    if (msgqueue.Any())
                    {
                        //Console.WriteLine("Message is not recieved from consumer.");
                        var recievedMessages = msgqueue.ToArray();
                        msgqueue.Clear();
                        _devices = recievedMessages.ToList();
                       
                        await ProcessMessage(_devices.ToTableEntity());
                        Console.WriteLine("Json desereliaze.");
                    }
                    else
                    {
                        Console.WriteLine("There is no new messages. Going to the new iteration ...");
                    }

                    Console.WriteLine($"Console sleeps for {_sleepTime} seconds");

                    Thread.Sleep(TimeSpan.FromSeconds(_sleepTime));


                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    throw ex;
                }
            } while (true);

        }

        private static async Task ProcessMessage(List<IotDeviceTableEntity> devices)
        {
            //List<TableResult> insertResults = new List<TableResult>();
            var counter = 0;

            Console.WriteLine("Entering ProcessMessage");
            Console.WriteLine($"Number of devices: {devices.Count()}");

           
            foreach (var item in devices)
            {
                await azureTableRepository.CreateOrUpdate(item);
                Console.WriteLine($"Device inserted. Current number of inserted devices: {counter++} ");
                
            }



            //var newDevice = new IotDevice() { Body = "asdas " + new Random().Next(0, 10000), DeviceName = "Sanjo" };
            //var tableOperation = TableOperation.Insert(item);
            //var insertResponse = await table.ExecuteAsync(tableOperation);
            //insertResults.Add(insertResponse);


            //Console.WriteLine($"Number of insert responses: {insertResults.Count()}");
            Console.WriteLine("Insert finished.");


        }


    }
}
