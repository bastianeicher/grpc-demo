using Identity.Api;

namespace Identity.Service;

public class TeamsService : Teams.TeamsBase
{
    private readonly IdentityDbContext _db;
    public TeamsService(IdentityDbContext db) => _db = db;

    public override async Task<Empty> Create(Team request, ServerCallContext context)
    {
        await _db.Teams.AddAsync(new()
        {
            Id = request.Id,
            Name = request.Name,
            Seats = request.Seats
        });
        await _db.SaveChangesAsync();

        return new();
    }

    public override async Task<Team> Read(TeamId request, ServerCallContext context)
    {
        var team = await FindAsync(request);
        return new()
        {
            Id = new(team.Id),
            Name = team.Name,
            Seats = team.Seats
        };
    }

    public override async Task<Empty> Update(Team request, ServerCallContext context)
    {
        var team = await FindAsync(request.Id);
        team.Name = request.Name;
        team.Seats = request.Seats;
        await _db.SaveChangesAsync();

        return new();
    }

    public override async Task<Empty> Delete(TeamId request, ServerCallContext context)
    {
        _db.Teams.Remove(await FindAsync(request));
        await _db.SaveChangesAsync();

        return new();
    }

    public override async Task<Empty> Join(TeamMembership request, ServerCallContext context)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(x => x.Id == request.Account)
                   ?? throw new RpcException(new Status(StatusCode.NotFound, "not_found"));

        account.Teams.Add(await FindAsync(request.Team));
        await _db.SaveChangesAsync();

        return new();
    }

    private async Task<TeamEntity> FindAsync(TeamId teamId)
        => await _db.Teams.FirstOrDefaultAsync(x => x.Id == teamId)
        ?? throw new RpcException(new Status(StatusCode.NotFound, "not_found"));

    public override async Task<Empty> Leave(TeamMembership request, ServerCallContext context)
    {
        var account = await FindAccountAsync(request.Account);
        var team = account.Teams.FirstOrDefault(x => x.Id == request.Team)
                ?? throw new RpcException(new Status(StatusCode.FailedPrecondition, "not_in_team"));

        account.Teams.Remove(team);
        await _db.SaveChangesAsync();
        
        return new();
    }

    public override async Task ListMembers(TeamId request, IServerStreamWriter<AccountId> responseStream, ServerCallContext context)
    {
        var team = await _db.Teams
            .Include(x => x.Accounts)
            .FirstOrDefaultAsync(x => x.Id == request)
         ?? throw new RpcException(new Status(StatusCode.NotFound, "not_found"));

        foreach (var account in team.Accounts)
            await responseStream.WriteAsync(new(account.Id));
    }

    [Obsolete]
    public override async Task<TeamId> FindByMember(AccountId request, ServerCallContext context)
    {
        var account = await FindAccountAsync(request);
        var team = account.Teams.FirstOrDefault()
                ?? throw new RpcException(new Status(StatusCode.FailedPrecondition, "not_in_team"));
        return new(team.Id);
    }

    public override async Task ListByMember(AccountId request, IServerStreamWriter<TeamId> responseStream, ServerCallContext context)
    {
        var account = await FindAccountAsync(request);
        foreach (var team in account.Teams)
            await responseStream.WriteAsync(new(team.Id));
    }

    private async Task<AccountEntity> FindAccountAsync(AccountId accountId)
        => await _db.Accounts
               .Include(x => x.Teams)
               .FirstOrDefaultAsync(x => x.Id == accountId) 
        ?? throw new RpcException(new Status(StatusCode.NotFound, "not_found"));
}