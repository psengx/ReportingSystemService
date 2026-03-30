using ReportingSystemService.Models;

namespace ReportingSystemService.Application.Dto
{
    public class ReportResponseDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public ReportResponseEntity? ReportResponse { get; set; }
    }
}
