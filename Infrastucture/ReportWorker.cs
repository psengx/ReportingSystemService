using Microsoft.EntityFrameworkCore;
using ReportingSystemService.Models;

namespace ReportingSystemService.Infrastucture
{
    public class ReportWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scoreFactory;
        public ReportWorker(IServiceScopeFactory scopeFactory)
        {
            _scoreFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var scope = _scoreFactory.CreateScope(); // Создание области (scope) для получения сервиса AddDbContext

                AddDbContext db = scope.ServiceProvider.GetRequiredService<AddDbContext>();

                ReportRequest? reportRequest = await db.ReportRequests
                    .Where(report => report.Status == "Pending")
                    .FirstOrDefaultAsync(stoppingToken); // Получение первого запроса со статусом "Pending"
                if (reportRequest == null)
                    continue; // Если нет запросов, продолжить цикл
                reportRequest.Status = "Processing";
                await db.SaveChangesAsync();

                await Task.Delay(10000); // Симуляция генерации отчета (временная)

                int views = new Random().Next(100, 1000); // Случайное количество просмотров
                int payments = new Random().Next(10, 100); // Случайное количество платежей
                decimal ratio = payments > 0 ? (decimal)(views / payments) : 0; // Вычисление соотношения просмотров к платежам

                db.ReportResponses.Add(new ReportResponse
                {
                    Id = Guid.NewGuid(),
                    ReportRequestId = reportRequest.Id,
                    Ratio = ratio,
                    PaymentsCount = payments,
                    ViewsCount = views
                });

                reportRequest.Status = "Created";
                await db.SaveChangesAsync();

                Thread.Sleep(50000); // Пауза между проверками
            }
        }
    }
}
