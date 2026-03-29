namespace ReportingSystemService.Models
{
    public class ReportRequest
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        // Период конверсии
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
