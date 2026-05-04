using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Domain.Entities;

namespace VehicleParts.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Part> Parts { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; }

    public DbSet<CustomerVehicle> CustomerVehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // CustomerVehicle configuration
        builder.Entity<CustomerVehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VehicleNumber)
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(e => e.Make)
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(32);
            entity.Property(e => e.Color)
                .HasMaxLength(32);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.VehicleNumber).IsUnique();
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
