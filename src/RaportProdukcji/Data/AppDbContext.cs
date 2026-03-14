using Microsoft.EntityFrameworkCore;
using RaportProdukcji.Models;

namespace RaportProdukcji.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ProductionOrder> ProductionOrders { get; set; } = null!;
    public DbSet<Batch> Batches { get; set; } = null!;
    public DbSet<Pallet> Pallets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ProductionOrder configuration
        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.RecipeNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PlannedWeightKg).HasPrecision(10, 2);
            entity.HasMany(e => e.Batches)
                .WithOne()
                .HasForeignKey(b => b.ProductionOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Pallets)
                .WithOne(p => p.ProductionOrder)
                .HasForeignKey(p => p.ProductionOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Batch configuration
        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Operator).HasMaxLength(100);
            entity.Property(e => e.ActualWeightKg).HasPrecision(10, 2);
        });

        // Pallet configuration
        modelBuilder.Entity<Pallet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PalletNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TotalWeightKg).HasPrecision(10, 2);
        });
    }
}
