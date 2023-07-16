namespace Identity.Service;

public class TeamEntity
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }
    
    public required int Seats { get; set; }

    public ICollection<AccountEntity> Accounts { get; set; } = new List<AccountEntity>();
}