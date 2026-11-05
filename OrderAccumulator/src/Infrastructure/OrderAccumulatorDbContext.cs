using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public sealed class OrderAccumulatorDbContext(DbContextOptions<OrderAccumulatorDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>(); 

        public DbSet<AssetExposure> AssetExposures => Set<AssetExposure>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderAccumulatorDbContext).Assembly);
        }
    }
}
