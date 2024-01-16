using CoralSchedular.InvoiceServices.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoralSchedular.InvoiceServices.Data
{
    public class CoralDbContext : DbContext
    {
        public CoralDbContext(DbContextOptions<CoralDbContext> options) : base(options)
        {

        }

        public DbSet<Reservation> Reservations { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reservation>().HasKey(x => x.Id);

            base.OnModelCreating(modelBuilder);
        }
        
    }
}
