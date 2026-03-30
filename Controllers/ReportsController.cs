using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportingSystemService.Application.Dto;
using ReportingSystemService.Infrastucture;
using ReportingSystemService.Infrastucture.Messaging;
using ReportingSystemService.Models;

namespace ReportingSystemService.Controllers
{
    // Контроллер для обработки запросов на создание отчетов
    [Route("/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        // Внедрение контекста базы данных для сохранения запросов на отчеты
        private readonly AddDbContext _context;
        private readonly RabbitMqProducer _producer;
        public ReportsController(AddDbContext context, RabbitMqProducer producer)
        {
            _context = context;
            _producer = producer;
        }
        // Метод для создания нового запроса на отчет
        [HttpPost]
        public async Task<IActionResult> CreateReport(CreateReportRequest request)
        {
            ReportRequest report = new()
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
            return Ok(new { report.Id });
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetReport(Guid id)
        {
            ReportRequest? reportRequest = await _context.ReportRequests.FindAsync(id); // Ищем запрос на отчет по идентификатору
            if (reportRequest == null)
                return NotFound();

            ReportResponse? reportResponse = await _context.ReportResponses
                .FirstOrDefaultAsync(response => response.ReportRequestId == id); // Ищем ответ на отчет, связанный с этим запросом

            return Ok(new
            {
                reportRequest.Id,
                reportRequest.Status,
                reportResponse
            });

        }
    }
}
