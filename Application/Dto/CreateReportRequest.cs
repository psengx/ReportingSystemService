namespace ReportingSystemService.Application.Dto
{
    public class CreateReportRequest
    {
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        // Период конверсии
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
