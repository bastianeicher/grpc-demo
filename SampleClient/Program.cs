using Grpc.Net.Client;
using Identity.Api;

const string apiKey = "my-secret-key";
using var channel = GrpcChannel.ForAddress("http://localhost:5201", new()
{
    HttpClient = new() {DefaultRequestHeaders = {{"x-api-key", apiKey}}}
});
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

Console.WriteLine($"Account {accountId} is member of team {await teams.FindByMemberAsync(accountId)}");

await teams.LeaveAsync(new() {Account = accountId, Team = teamId});
await teams.DeleteAsync(teamId);
await accounts.DeleteAsync(accountId);
