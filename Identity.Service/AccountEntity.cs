namespace Identity.Service;

public class AccountEntity
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public TeamEntity? Team { get; set; }
}