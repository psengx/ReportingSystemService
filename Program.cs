using Microsoft.EntityFrameworkCore;
using ReportingSystemService.Infrastructure;
using ReportingSystemService.Infrastructure.Messaging;

namespace ReportingSystemService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AddDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSingleton<RabbitMqService>(); // Регистрируем RabbitMqService как синглтон
            builder.Services.AddSingleton<RabbitMqProducer>(); // Регистрируем RabbitMqProducer как синглтон
            builder.Services.AddHostedService<RabbitMqConsumer>(); // Регистрируем RabbitMqConsumer как фоновую службу

            var app = builder.Build();
            using (var scope = app.Services.CreateScope()) // Авто-применение миграций при запуске приложения
            {
                var db = scope.ServiceProvider.GetRequiredService<AddDbContext>();
                db.Database.Migrate();
            }
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
