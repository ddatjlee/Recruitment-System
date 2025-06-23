using Microsoft.EntityFrameworkCore;
using DoAn_Web.Models;

namespace DoAn_Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nếu muốn cấu hình thêm, có thể thêm tại đây (ví dụ: unique email

        }
    }
}
