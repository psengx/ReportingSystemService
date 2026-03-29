namespace ReportingSystemService.Application.Dto
{
    public class ReportRequestMessage // Dto для передачи данных о запросе в Producer
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
