// This function configures database schema, enforces data integrity, and optimizes queries for secure, efficient application data management.
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique index on User email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Unique index on RefreshToken hash
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.TokenHash)
            .IsUnique();

        // Set foreign key between RefreshToken and User
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes to optimize product queries
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Price);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Stock);

        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.Stock, p.Id });
    }
}
