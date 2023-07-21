namespace Identity.Service;

public class AccountEntity
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public ICollection<TeamEntity> Teams { get; set; } = new List<TeamEntity>();
}