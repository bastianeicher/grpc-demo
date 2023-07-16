namespace Identity.Service;

public class IdentityDbContext : DbContext
{
    public required DbSet<AccountEntity> Accounts { get; set; }
    public required DbSet<TeamEntity> Teams { get; set; }

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
        => builder
            .Entity<AccountEntity>()
            .HasIndex(x => x.Email)
            .IsUnique();
}
