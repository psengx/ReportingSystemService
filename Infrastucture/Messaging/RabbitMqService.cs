using RabbitMQ.Client;

namespace ReportingSystemService.Infrastucture.Messaging
{
    public class RabbitMqService
    {
        public IConnection? connection = null; // Соединение с RabbitMq
        public IChannel? channel = null; // Канал для RabbitMq
        public async Task<IChannel> GetChannelAsync()
        {
            if (channel != null)
                return channel;

            ConnectionFactory factory = new ConnectionFactory { HostName = "rabbitmq" }; // Фабрика соединений с названием контейнера

            connection =  await factory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();

            return channel;
        }
    }
}
