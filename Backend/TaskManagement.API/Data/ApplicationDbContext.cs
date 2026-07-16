using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data.Configurations;
using TaskManagement.API.Entities;

namespace TaskManagement.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<TaskItem> Tasks { get; set; }

    public DbSet<TaskAttachment> TaskAttachments { get; set; }

    public DbSet<TaskComment> TaskComments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        var demoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        modelBuilder.ApplyConfiguration(new TaskAttachmentConfiguration());

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = demoUserId,
            Username = "demo",
            Email = "demo@taskmanagement.com",
            PasswordHash = "$2a$11$demoHashWillBeChangedLater",
            FirstName = "Demo",
            LastName = "User",
            CreatedAt = DateTime.SpecifyKind(
                new DateTime(2026, 1, 1),
                DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(
                new DateTime(2026, 1, 1),
                DateTimeKind.Utc),
            IsActive = true
        });
    }
}