using Microsoft.EntityFrameworkCore;
using Agent.Core.Entities;

namespace Agent.Infrastructure.Data
{
    public class AgentDbContext : DbContext
    {
        public AgentDbContext(DbContextOptions<AgentDbContext> options) : base(options) { }

        public DbSet<RawInvoice> RawInvoices { get; set; }
        public DbSet<Vendor> Vendors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RawInvoice>().HasKey(x => x.Id);
            modelBuilder.Entity<Vendor>().HasKey(x => x.Id);
        }
    }
}