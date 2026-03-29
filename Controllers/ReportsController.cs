using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportingSystemService.Application.Dto;
using ReportingSystemService.Infrastucture;
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
        public ReportsController(AddDbContext context)
        {
            _context = context;
        }
        // Метод для создания нового запроса на отчет
        [HttpPost]
        public async Task<IActionResult> CreateReport(CreateReportRequest request)
        {
            // Логика создания отчета
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

            // Возвращаем идентификатор созданного отчета
            return Ok(new { report.Id });
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetReport(Guid id)
        {
            ReportRequest? reportRequest = await _context.ReportRequests.FindAsync(id);
            if (reportRequest == null)
                return NotFound();

            ReportResponse reportResponse = await _context.ReportResponses
                .FirstAsync(response => response.ReportRequestId == id);

            return Ok(new
            {
                reportRequest.Id,
                reportRequest.Status,
                reportResponse
            });

        }
    }
}
