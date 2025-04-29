using Microsoft.EntityFrameworkCore;

namespace Pioneer.Models
{
    public class PioneerDBContext : DbContext
    {
        public PioneerDBContext(DbContextOptions<PioneerDBContext> options) : base(options)
        {

        }

        // Implement code here
        public DbSet<Freshman> Freshmen { set; get; }
    }
}
