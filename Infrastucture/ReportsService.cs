using Microsoft.EntityFrameworkCore;
using ReportingSystemService.Application.Dto;
using ReportingSystemService.Infrastucture.Messaging;
using ReportingSystemService.Models;

namespace ReportingSystemService.Infrastucture
{
    public class ReportsService
    {
        private readonly AddDbContext _context;
        private readonly RabbitMqProducer _producer;
        private readonly IServiceScopeFactory _scopeFactory; // Фабрика для создания области (scope) для получения сервиса AddDbContext
        public ReportsService(AddDbContext context, RabbitMqProducer producer)
        {
            _context = context;
            _producer = producer;
        }

        public async Task<Guid> CreateReportAsync(CreateReportRequest request)
        {
            ReportRequestEntity report = new()
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                OrderId = request.OrderId,
                From = request.From,
                To = request.To,
                Status = "Pending"
            };
            // Сохранение запроса в базе данных
            _context.ReportRequests.Add(report);
            await _context.SaveChangesAsync();

            await _producer.SendMessage(new ReportRequestMessage // Отправляем сообщение в шину RabbitMQ
            {
                Id = report.Id,
                ProductId = report.ProductId,
                OrderId = report.OrderId,
                From = report.From,
                To = report.To
            });

            // Возвращаем идентификатор созданного отчета
            return report.Id;
        }

        public async Task<ReportResponseDto> GetReportAsync(Guid id)
        {
            ReportRequestEntity? reportRequest = await _context.ReportRequests.FindAsync(id); // Ищем запрос на отчет по идентификатору
            if (reportRequest == null)
                return new()
                {
                    Status = "Not Found",
                    ReportResponse = { }
                };

            ReportResponseEntity? reportResponse = await _context.ReportResponses
                .FirstOrDefaultAsync(response => response.ReportRequestId == id); // Ищем ответ на отчет, связанный с этим запросом

            return new()
            {
                Status = reportRequest.Status,
                ReportResponse = new() { Ratio = reportResponse?.Ratio ?? 0, PaymentsCount = reportResponse?.PaymentsCount ?? 0 } // Возвращаем статус запроса и данные отчета (если он готов)
            };
        }

        public async Task ReportProcessing(ReportRequestMessage reportRequestMessage)
        {
            // Логика обработки сообщения
            using var scope = _scopeFactory.CreateScope(); // Создание области (scope) для получения сервиса AddDbContext

            AddDbContext db = scope.ServiceProvider.GetRequiredService<AddDbContext>();

            ReportRequestEntity? reportRequest = await db.ReportRequests
                .FirstOrDefaultAsync(request => request.Id == reportRequestMessage.Id);
            
            reportRequest!.Status = "Processing"; // Обновление статуса на "Processing"
            await db.SaveChangesAsync();

            // Симуляция генерации отчета
            await Task.Delay(10000);
            int views = new Random().Next(100, 1000);
            int payments = new Random().Next(10, 100);
            decimal ratio = payments > 0 ? (decimal)payments / views : 0;

            // Сохранение результата в базу данных
            db.ReportResponses.Add(new ReportResponseEntity
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
