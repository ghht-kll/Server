using Microsoft.EntityFrameworkCore;

namespace Server.src;

public partial class ApplicationContext : DbContext
{
    public ApplicationContext()
    {
    }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Message> Messages { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=posgres;Port=5432;Database=Chat;Username=guest;Password=guest");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Name).HasColumnName("name");

            entity.Property(e => e.Text).HasColumnName("text");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}