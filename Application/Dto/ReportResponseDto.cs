using ReportingSystemService.Models;

namespace ReportingSystemService.Application.Dto
{
    public class ReportResponseDto
    {
        public string Status { get; set; }
        public ReportResponse ReportResponse { get; set; }
    }
}
