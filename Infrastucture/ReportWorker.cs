
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
            while(!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scoreFactory.CreateScope()) // Создание области (scope) для получения сервиса AddDbContext
                {
                    AddDbContext db = scope.ServiceProvider.GetRequiredService<AddDbContext>();
                    
                    ReportRequest reportRequest = await db.ReportRequests
                        .Where(r => r.Status == "Pending")
                        .FirstAsync(); // Получение первого запроса со статусом "Pending"

                    reportRequest.Status = "Processing";
                    await db.SaveChangesAsync();

                    await Task.Delay(10000); // Симуляция генерации отчета (временная)

                    int views = new Random().Next(100, 1000); // Случайное количество просмотров
                    int payments = new Random().Next(10, 100); // Случайное количество платежей

                    db.ReportResponses.Add(new ReportResponse
                    {
                    Id = Guid.NewGuid(),
                        ReportRequestId = reportRequest.Id,
                        Ratio = payments / views,
                        PaymentsCount = payments
                    });

                    reportRequest.Status = "Created";
                    await db.SaveChangesAsync();
                }
                Thread.Sleep(5000); // Пауза между проверками
            }
        }
    }
}
