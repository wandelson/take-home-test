using Fundo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Infrastructure.Persistence;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
    }

    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Amount).HasPrecision(18, 2);
            entity.Property(l => l.CurrentBalance).HasPrecision(18, 2);
            entity.Property(l => l.ApplicantName).HasMaxLength(200).IsRequired();
            entity.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(l => l.CreatedAt).IsRequired();
            entity.Property(l => l.RowVersion).IsRowVersion();
            entity.HasMany(l => l.Payments).WithOne(p => p.Loan).HasForeignKey(p => p.LoanId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.RecordedAt).IsRequired();
            entity.Property(p => p.IdempotencyKey).HasMaxLength(100);
            entity.HasIndex(p => new { p.LoanId, p.IdempotencyKey })
                .IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");
        });
    }
}
