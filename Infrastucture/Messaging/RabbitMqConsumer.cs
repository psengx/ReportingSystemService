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

        private readonly IServiceScopeFactory _scopeFactory; // Фабрика для создания области (scope) для получения сервиса AddDbContext
        public RabbitMqConsumer(IServiceScopeFactory scopeFactory, RabbitMqService rabbitMqService)
        {
            _scopeFactory = scopeFactory;
            _rabbitMqService = rabbitMqService;
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

                await ReportProcessing(reportRequestMessage); // Метод для обработки запроса на генерацию отчета
            };
            await channel!.BasicConsumeAsync(queue: "ReportRequests",
                                    autoAck: false,
                                    consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken); // Бесконечная задержка, чтобы сервис продолжал работать и обрабатывать сообщения
        }

        public async Task ReportProcessing(ReportRequestMessage reportRequestMessage)
        {
            // Логика обработки сообщения
            using var scope = _scopeFactory.CreateScope(); // Создание области (scope) для получения сервиса AddDbContext

            AddDbContext db = scope.ServiceProvider.GetRequiredService<AddDbContext>();

            ReportRequest? reportRequest = await db.ReportRequests
                .FirstOrDefaultAsync(request => request.Id == reportRequestMessage.Id);
            if (reportRequest == null)
                throw new Exception($"ReportRequest with Id {reportRequestMessage.Id} not found in database");

            reportRequest.Status = "Processing"; // Обновление статуса на "Processing"
            await db.SaveChangesAsync();

            // Симуляция генерации отчета
            await Task.Delay(10000); 
            int views = new Random().Next(100, 1000);
            int payments = new Random().Next(10, 100); 
            decimal ratio = payments > 0 ? (decimal)payments / views : 0;

            // Сохранение результата в базу данных
            db.ReportResponses.Add(new ReportResponse
            {
                Id = Guid.NewGuid(),
                ReportRequestId = reportRequest.Id,
                Ratio = ratio,
                PaymentsCount = payments,
                ViewsCount = views
            });

            reportRequest.Status = "Ready";
            await db.SaveChangesAsync();
        }
    }
}
