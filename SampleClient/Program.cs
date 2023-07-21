using Grpc.Net.Client;
using Identity.Api.V2;

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

await teams.JoinAsync(new() {Account = accountId, Team = teamId});

using var streaming = teams.ListByMember(accountId);
while (await streaming.ResponseStream.MoveNext(default))
    Console.WriteLine($"Account {accountId} is member of team {streaming.ResponseStream.Current}");

await teams.LeaveAsync(new() {Account = accountId, Team = teamId});
await teams.DeleteAsync(teamId);
await accounts.DeleteAsync(accountId);
