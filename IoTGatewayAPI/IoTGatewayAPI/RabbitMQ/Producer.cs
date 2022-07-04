using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace IoTGatewayAPI.RabbitMQ
{
    public class Producer : IMessageProducer
    {
        private string _hostname = Environment.GetEnvironmentVariable("HostName");
        private string _username = Environment.GetEnvironmentVariable("UserName");
        private string _password = Environment.GetEnvironmentVariable("Password");
        private int _port = Convert.ToInt32(Environment.GetEnvironmentVariable("Port"));
        private string _queueName = Environment.GetEnvironmentVariable("QueueName");
        private ConnectionFactory _factory;

        public Producer(string hostname, string username, string password, int port, string queuename) { 

            _hostname = hostname;
            _username = username; 
            _password = password;   
            _port = port;   
            _queueName = queuename; 
        
        }

        public async Task<bool> SendMessage<T>(T message)
        {
            Console.WriteLine("Entering send message");
            Console.WriteLine("Entering send message");

            _factory = new ConnectionFactory { HostName = _hostname, UserName = _username, Password = _password, Port = _port };
            var connection = _factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                Console.WriteLine("Declaring queue ...");
                channel.QueueDeclare(queue: _queueName,
                                                 durable: false,
                                                 exclusive: false,
                                                 autoDelete: false,
                                                 arguments: null);
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                Console.WriteLine("Entering basic publish ...");
                channel.BasicPublish(exchange: "", routingKey: _queueName, body: body);
                Console.WriteLine("Basic publish finished ...");
                return true;


            }
            Console.WriteLine("Basic publish false.. ");
            return false;

        }
    }
}
