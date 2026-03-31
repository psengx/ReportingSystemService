using Microsoft.EntityFrameworkCore;
using ReportingSystemService.Models;

namespace ReportingSystemService.Infrastructure
{
    public class AddDbContext : DbContext
    {
        public DbSet<ReportRequestEntity> ReportRequests { get; set; }
        public DbSet<ReportResponseEntity> ReportResponses { get; set; }
        public AddDbContext(DbContextOptions<AddDbContext> options) : base(options)
        {
        }
    }
}
