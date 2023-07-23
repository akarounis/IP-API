using Microsoft.EntityFrameworkCore;
using NovibetProject.Models;

namespace NovibetProject
{    
    public class IpDetailsContext : DbContext
    {
        public DbSet<IPAddresses> IPAddresses { get; set; }
        public DbSet<Countries> Countries { get; set; }
        public DbSet<AddressesPerCountry> AddressesPerCountry { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("ConnectionString");            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AddressesPerCountry>().HasKey(apc => apc.CountryName);
        }
    }
    
}
