using Google.Rpc;
using GrpcRichError;
using Identity.Api;
using Status = Grpc.Core.Status;

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

#pragma warning disable CS8602
    public override async Task<Empty> Join(TeamMembership request, ServerCallContext context)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(x => x.Id == request.Account);
        if (account.Team != null)
        {
            throw new Google.Rpc.Status
            {
                Code = (int)StatusCode.FailedPrecondition,
                Message = "Already in a team",
                Details =
                {
                    new PreconditionFailure
                    {
                        Violations =
                        {
                            new PreconditionFailure.Types.Violation {Type = "already_in_team"},
                            new PreconditionFailure.Types.Violation {Type = "cannot_join_team"}
                        }
                    }
                }
            }.ToException();
        }

        account.Team = await FindAsync(request.Team);
        await _db.SaveChangesAsync();

        return new();
    }

    private async Task<TeamEntity> FindAsync(TeamId teamId)
        => await _db.Teams.FirstOrDefaultAsync(x => x.Id == teamId)
        ?? throw new RpcException(new Status(StatusCode.NotFound, "not_found"));

    public override async Task<Empty> Leave(TeamMembership request, ServerCallContext context)
    {
        var account = await FindAccountAsync(request.Account);
        if (account.Team?.Id != request.Team) throw new RpcException(new Status(StatusCode.FailedPrecondition, "not_in_team"));

        account.Team = null;
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

    public override async Task<TeamId> FindByMember(AccountId request, ServerCallContext context)
    {
        var account = await FindAccountAsync(request);
        if (account.Team == null) throw new RpcException(new Status(StatusCode.FailedPrecondition, "not_in_team"));

        return new(account.Team.Id);
    }

    private async Task<AccountEntity> FindAccountAsync(AccountId accountId)
        => await _db.Accounts
               .Include(x => x.Team)
               .FirstOrDefaultAsync(x => x.Id == accountId) 
        ?? throw new RpcException(new Status(StatusCode.NotFound, "not_found"));
}