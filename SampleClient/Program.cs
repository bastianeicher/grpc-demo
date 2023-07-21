using Grpc.Core;
using Grpc.Net.Client;
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

var teamId1 = new TeamId(Guid.NewGuid());
await teams.CreateAsync(new()
{
    Id = teamId1,
    Name = "Awesome Team",
    Seats = 1
});

var teamId2 = new TeamId(Guid.NewGuid());
await teams.CreateAsync(new()
{
    Id = teamId2,
    Name = "Another Awesome Team",
    Seats = 1
});

await teams.JoinAsync(new() {Account = accountId, Team = teamId1});
await teams.JoinAsync(new() {Account = accountId, Team = teamId2});

try
{
    using var streaming = teams.ListByMember(accountId);
    while (await streaming.ResponseStream.MoveNext(default))
        Console.WriteLine($"Account {accountId} is member of team {streaming.ResponseStream.Current}");
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.Unimplemented)
{
    #pragma warning disable CS0612
    Console.WriteLine($"Account {accountId} is member of team {await teams.FindByMemberAsync(accountId)}");
    #pragma warning restore CS0612
}

await teams.LeaveAsync(new() {Account = accountId, Team = teamId1});
await teams.LeaveAsync(new() {Account = accountId, Team = teamId2});
await teams.DeleteAsync(teamId1);
await teams.DeleteAsync(teamId2);
await accounts.DeleteAsync(accountId);
