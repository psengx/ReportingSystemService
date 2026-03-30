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
        private readonly ReportsService _reportsService;
        public ReportsController(AddDbContext context, RabbitMqProducer producer)
        {
            _context = context;
            _producer = producer;
            _reportsService = new ReportsService(context, producer);
        }
        // Метод для создания нового запроса на отчет
        [HttpPost]
        public async Task<IActionResult> CreateReport(CreateReportRequest request)
        {
            var id = await _reportsService.CreateReportAsync(request);
            return Ok(id);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetReport(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid report ID.");

            var report = await _reportsService.GetReportAsync(id);
            if (report.Status == "Ready")
            {
                return Ok(new ReportResponse()
                {
                    Ratio = report.ReportResponse.Ratio,
                    PaymentsCount = report.ReportResponse.PaymentsCount,
                });
            }
            return Ok(report);
        }
    }
}
