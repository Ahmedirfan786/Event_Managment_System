using Event_Managment_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_Managment_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions <ApplicationDbContext> option) : base(option)
        {
            
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Packages> Packages { get; set; }
        public DbSet<PackageFeatures> PackageFeatures { get; set; }
        public DbSet<Portfolios> Portfolios { get; set; }
        public DbSet<PortfolioImages> PortfolioImages { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Bookings> Bookings { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<Blogs> Blogs { get; set; }

    }
}
