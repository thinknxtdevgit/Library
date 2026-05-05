
using lib.Models;
using Microsoft.EntityFrameworkCore;
namespace lib.Dbcontext
{
    public class AppDbcontext : DbContext
    {
        public AppDbcontext(DbContextOptions<AppDbcontext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
        public DbSet<Admissions> Admissions { get; set; }
        public DbSet<MasterCourse> MasterCourses { get; set; }
    }
}
