using Microsoft.EntityFrameworkCore;
using ReportingSystemService.Models;

namespace ReportingSystemService.Infrastucture
{
    public class AddDbContext : DbContext
    {
        public DbSet<ReportRequest> ReportRequests { get; set; }
        public DbSet<ReportResponse> ReportResponses { get; set; }
        public AddDbContext(DbContextOptions<AddDbContext> options) : base(options)
        {
        }
    }
}
