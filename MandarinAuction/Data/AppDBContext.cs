using MandarinAuction.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MandarinAuction.Data
{
    public class appDBContext : IdentityDbContext<ApplicationUser>
    {
        public appDBContext(DbContextOptions<appDBContext> options) : base(options) { }

        public DbSet<Mandarin> Mandarins { get; set; }
        public DbSet<Bid> Bids { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) { 
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
