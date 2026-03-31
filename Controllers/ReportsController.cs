using Microsoft.AspNetCore.Mvc;
using ReportingSystemService.Application.Dto;
using ReportingSystemService.Infrastructure;
using ReportingSystemService.Infrastructure.Messaging;

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
            var report = await _reportsService.GetReportAsync(id);

            return (report.Status != "Ready") ? Ok(report) : Ok(report.ReportResponse);
        }
    }
}
