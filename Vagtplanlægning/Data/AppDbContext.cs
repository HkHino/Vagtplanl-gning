using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Data
{
    public class AppDbContext
    {
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public DbSet<User> Users { get; set; }
        }
    }
}



