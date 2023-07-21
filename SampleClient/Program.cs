using Google.Rpc;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcRichError;
using Identity.Api;

using var channel = GrpcChannel.ForAddress("http://localhost:5201");
var accounts = new Accounts.AccountsClient(channel);
var teams = new Teams.TeamsClient(channel);

var accountId = new AccountId(Guid.NewGuid());
await accounts.CreateAsync(new()
{
    Id = accountId,
    Name = "Jane Doe",
    Email = $"{accountId}@example.com"
});

var teamId = new TeamId(Guid.NewGuid());
await teams.CreateAsync(new()
{
    Id = teamId,
    Name = "Awesome Team",
    Seats = 1
});

try
{
    await teams.JoinAsync(new() {Account = accountId, Team = teamId});
}
catch (RpcException ex) when (ex.GetDetail<PreconditionFailure>() is { } failure
                              && failure.Violations.Any(x => x.Type == "already_in_team"))
{
    Console.Error.WriteLine($"Account {accountId} is already in a team!");
    return;
}
catch (RpcException ex) when (ex.GetDetail<PreconditionFailure>() is { } failure
                              && failure.Violations.Any(x => x.Type == "cannot_join_team"))
{
    Console.Error.WriteLine($"Account {accountId} cannot join the team!");
    return;
}

Console.WriteLine($"Account {accountId} is member of team {await teams.FindByMemberAsync(accountId)}");

await teams.LeaveAsync(new() {Account = accountId, Team = teamId});
await teams.DeleteAsync(teamId);
await accounts.DeleteAsync(accountId);
