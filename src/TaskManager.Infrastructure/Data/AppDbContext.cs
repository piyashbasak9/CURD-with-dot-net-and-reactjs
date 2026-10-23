
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Identity;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

protected override void OnModelCreating(ModelBuilder builder){
    base.OnModelCreating(builder);

    builder.Entity<EmailVerificationCode>(entity =>
    {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.CodeHash)
            .HasMaxLength(128)
            .IsRequired();

        entity.HasIndex(x => new
        {
            x.UserId,
            x.CreatedAtUtc
        });

        entity.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });
}

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<EmailVerificationCode> EmailVerificationCodes
    => Set<EmailVerificationCode>();
}
