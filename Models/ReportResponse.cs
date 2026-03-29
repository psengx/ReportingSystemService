namespace ReportingSystemService.Models
{
    public class ReportResponse
    {
        public Guid Id { get; set; }
        public Guid ReportRequestId { get; set; }
        public int ViewsCount { get; set; }
        public int PaymentsCount { get; set; }
        public decimal Ratio { get; set; }
    }
}
