using HomeWifiQR.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeWifiQR.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<WifiNetwork> WifiNetworks { get; set; } = null!;
    }
}