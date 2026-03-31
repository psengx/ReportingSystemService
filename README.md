# ReportingSystemService

## Описание проекта
Сервис для получения отчета по просмотрам и продажам товара.

## Архитектура проекта
- WebAPI ASP.NET Core — принимает HTTP-запросы пользователей
- RabbitMQ — брокер сообщений, содержит очередь запросов на отчет.
  - RabbitMqProducer — отправляет запросы пользователей в очередь
  - RabbitMqConsumer — получает запросы из очереди и отправляет их на обработку 
- PortgreSQL — хранение запросов и готовых отчетов

## Структура репозитория
```
ReportingSystem
├── ReportingSystemService
│   ├── Models
│   │   ├── ReportRequestEntity.cs // Сущность запроса для БД
│   │   ├── ReportResponseEntity.cs // Сущность ответа для БД
│   │   └── ReportResponse.cs // Ответ из ТЗ (ratio, payments)
│   ├── Infrastructure
│   │   ├── Messaging
│   │   │   └── RabbitMqService.cs // Сервис для подключения RabbitMq
│   │   │   └── RabbitMqProducer.cs // Входная точка в очередь соообщений
│   │   │   └── RabbitMqConsumer.cs // "Потребитель" для получения сообщений из очереди, отправляет запросы на обработку
│   │   ├── AddDbContext.cs // Инициализация контекста для БД
│   │   └── ReportsService.cs // Сервис для логики http-запросов и обработки запросов
│   ├── Controllers
│   │   └── ReportsController.cs 
│   ├── Application / Dto
│   │   ├── CreateReportRequest.cs // Dto для запроса
│   │   ├── ReportRequestMessage.cs // Dto для передачи данных о запросе в Producer
│   │   └── ReportResponseDto.cs // Dto для отправки ответа в контроллер
│   └── Program.cs
└── ReportingSystemTests
    └── ReportServiceTests.cs // Тесты для ReportsService (залить не получилось)
```

## Установка проекта

```
git clone https://github.com/psengx/ReportingSystem/
```

## Запуск WebAPI

```
cd ReportingSystemService
```

### С контейнеризацией (Docker)

```
docker-compose up --build
```

### Без контейнеризации
Установка зависимостей:

```
dotnet restore
```

Далее нужно вручную запустить:
- PostgreSQL
- RabbitMQ
- Сам сервис ReportingSystemService с помощью `dotnet run`

## Эндпоинты

### POST /reports
Отправка запроса на создание отчёта

Запрос:

```
{
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "from": "2026-03-31T20:29:58.465Z",
  "to": "2026-03-31T20:29:58.465Z"
}
```

Ответ: идентификатор запроса

```
"6a340c5f-7a95-4abf-a799-8c2fdf0800a9"
```

---

### GET /reports/id
Получение отчета по идентификатору запроса

Запрос:

```
"6a340c5f-7a95-4abf-a799-8c2fdf0800a9"
```

Ответ:

```
{
  "ratio": 0.7699115044247787,
  "paymentsCount": 87
}
```

#### Возможные ответы / ошибки:
Несуществующий идентификатор запроса

```
{
    Status = "Not Found",
    ReportResponse = {}
}
```

Отчет еще не готов

```
{
  "status": "Pending" ИЛИ "Processing",
  "reportResponse": {
    "ratio": 0,
    "paymentsCount": 0
  }
}
```
