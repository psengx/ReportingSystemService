using Microsoft.EntityFrameworkCore;
using ReportingSystemService.Application.Dto;
using ReportingSystemService.Infrastructure.Messaging;
using ReportingSystemService.Models;

namespace ReportingSystemService.Infrastructure
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
    }
}
