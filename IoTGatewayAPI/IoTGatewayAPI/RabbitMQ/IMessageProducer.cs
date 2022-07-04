namespace IoTGatewayAPI.RabbitMQ
{
    public interface IMessageProducer
    {
        Task<bool> SendMessage<T>(T message);
    }
}
