using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ReportingSystemService.Infrastucture.Messaging
{
    public class RabbitMqProducer
    {
        public IConnection? connection = null; // Соединение с RabbitMq
        public IChannel? channel = null; // Канал для отправки сообщений в RabbitMq
        public async Task SendMessage(object obj)
        {
            var message = JsonSerializer.Serialize(obj);

            var factory = new ConnectionFactory { HostName = "rabbitmq" }; // Фабрика соединений с названием контейнера
            
            if (connection == null)
                connection = await factory.CreateConnectionAsync();
            if (channel == null)
                channel = await connection.CreateChannelAsync();

            channel?.QueueDeclareAsync(queue: "ReportRequests", // Объявление очереди для сообщений
                               durable: false,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);

            var body = Encoding.UTF8.GetBytes(message); // Кодирование сообщения в массив байтов для отправки

            channel?.BasicPublishAsync(exchange: "", // Отправка сообщения в очередь
                           routingKey: "ReportRequests",
                           body: body);
        }
    }
}
