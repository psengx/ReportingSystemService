using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ReportingSystemService.Infrastucture.Messaging
{
    public class RabbitMqProducer
    {
        private readonly RabbitMqService _rabbitMqService; // Сервис для работы с RabbitMq
        public RabbitMqProducer(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService; 
        }

        public async Task SendMessage(object obj)
        {
            IChannel channel = await _rabbitMqService.GetChannelAsync(); // Получение канала для работы с RabbitMq
            if (channel == null)
                throw new Exception("RabbitMQ channel is NULL");

            channel?.QueueDeclareAsync(queue: "ReportRequests", // Объявление очереди для сообщений
                               durable: false,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj)); // Кодирование сообщения в массив байтов для отправки

            channel?.BasicPublishAsync(exchange: "", // Отправка сообщения в очередь
                           routingKey: "ReportRequests",
                           body: body);
            await Task.CompletedTask;
        }
    }
}
