using Microsoft.EntityFrameworkCore;
using Shared;

namespace API.Persistence;

internal class WebAuthenticationDbContext : DbContext
{
    public WebAuthenticationDbContext(DbContextOptions<WebAuthenticationDbContext> options)
        : base(options)
    {
        UserDetails = Set<UserDetail>();
        KanBanSections = Set<KanBanSection>();
        KanBanTaskItems = Set<KanBanTaskItem>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDetail>()
            .HasMany(section => section.KanBanSections)
            .WithOne(item => item.UserDetail);

        modelBuilder.Entity<KanBanSection>()
            .HasMany(section => section.KanBanTaskItems)
            .WithOne(item => item.KanBanSection)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<UserDetail> UserDetails { get; set; }
    public DbSet<KanBanSection> KanBanSections { get; set; }
    public DbSet<KanBanTaskItem> KanBanTaskItems { get; set; }
}
