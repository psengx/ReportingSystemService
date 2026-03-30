using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportingSystemService.Application.Dto;
using ReportingSystemService.Models;

namespace ReportingSystemService.Infrastucture.Messaging
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly RabbitMqService _rabbitMqService; // Сервис для работы с RabbitMq
        private readonly ReportsService _reportsService;

        public RabbitMqConsumer(RabbitMqService rabbitMqService, ReportsService reportsService)
        {
            _rabbitMqService = rabbitMqService;
            _reportsService = reportsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = await _rabbitMqService.GetChannelAsync(); // Получение канала для работы с RabbitMq
            if (channel == null)
                throw new Exception("RabbitMQ channel is NULL");

            channel?.QueueDeclareAsync(queue: "ReportRequests", // Объявление очереди, из которой буду забирать сообщения
                               durable: false,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel!); // Объявляю потребителя для получения сообщений
            consumer.ReceivedAsync += async (model, ea) => // Событие, которое срабатывает при получении сообщения
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var reportRequestMessage = JsonSerializer.Deserialize<ReportRequestMessage>(message);

                if (reportRequestMessage == null)
                    throw new Exception("Received message is NULL");

                await _reportsService.ReportProcessing(reportRequestMessage); // Метод для обработки запроса на генерацию отчета
            };
            await channel!.BasicConsumeAsync(queue: "ReportRequests",
                                    autoAck: false,
                                    consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken); // Бесконечная задержка, чтобы сервис продолжал работать и обрабатывать сообщения
        }
    }
}
