using Microsoft.EntityFrameworkCore;
using PoultryManager.Api.Domain;

namespace PoultryManager.Api.Data;

/// <summary>Contexte EF Core (SQL Server) : mêmes tables que l'ancienne base SQLite locale.</summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Band> Bands => Set<Band>();
    public DbSet<EggRecord> EggRecords => Set<EggRecord>();
    public DbSet<FeedRecord> FeedRecords => Set<FeedRecord>();
    public DbSet<MortalityRecord> MortalityRecords => Set<MortalityRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            e.Property(u => u.FarmName).HasMaxLength(150);
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.UserName).HasMaxLength(256);
            e.Property(u => u.Phone).HasMaxLength(40);
            e.Property(u => u.PasswordHash).HasMaxLength(256);
            e.Property(u => u.PasswordSalt).HasMaxLength(256);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.UserName);
        });

        modelBuilder.Entity<Band>(e =>
        {
            e.ToTable("Bands");
            e.Property(b => b.Name).HasMaxLength(100).IsRequired();
            e.Property(b => b.Strain).HasMaxLength(100);
            e.Property(b => b.Notes).HasMaxLength(2000);
            e.Property(b => b.StartDate).HasColumnType("date");
            e.Property(b => b.ClosedDate).HasColumnType("date");
            e.HasIndex(b => b.UserId);
            e.HasOne(b => b.User).WithMany(u => u.Bands).HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EggRecord>(e =>
        {
            e.ToTable("EggRecords");
            e.Property(r => r.Date).HasColumnType("date");
            e.Property(r => r.Category).HasMaxLength(50);
            e.Property(r => r.Notes).HasMaxLength(2000);
            // Une seule saisie par jour et par bande.
            e.HasIndex(r => new { r.BandId, r.Date }).IsUnique();
            e.HasOne(r => r.Band).WithMany(b => b.EggRecords).HasForeignKey(r => r.BandId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeedRecord>(e =>
        {
            e.ToTable("FeedRecords");
            e.Property(r => r.Date).HasColumnType("date");
            e.Property(r => r.FeedType).HasMaxLength(100);
            e.Property(r => r.Notes).HasMaxLength(2000);
            e.HasIndex(r => new { r.BandId, r.Date }).IsUnique();
            e.HasOne(r => r.Band).WithMany(b => b.FeedRecords).HasForeignKey(r => r.BandId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MortalityRecord>(e =>
        {
            e.ToTable("MortalityRecords");
            e.Property(r => r.Date).HasColumnType("date");
            e.Property(r => r.Cause).HasMaxLength(100);
            e.Property(r => r.Notes).HasMaxLength(2000);
            e.HasIndex(r => new { r.BandId, r.Date }).IsUnique();
            e.HasOne(r => r.Band).WithMany(b => b.MortalityRecords).HasForeignKey(r => r.BandId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
