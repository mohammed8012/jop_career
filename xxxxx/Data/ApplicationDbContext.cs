using Microsoft.EntityFrameworkCore;

using xxxxx.Models;

namespace xxxxx.Data
{
 
    
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<User> Users { get; set; }
        }
    
}
