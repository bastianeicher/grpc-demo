using Identity.Api.V1;

namespace Identity.Service;

public class AccountsV1Service : Accounts.AccountsBase
{
    private readonly IdentityDbContext _db;
    public AccountsV1Service(IdentityDbContext db) => _db = db;

    public override async Task<Empty> Create(Account request, ServerCallContext context)
    {
        await _db.Accounts.AddAsync(new()
        {
            Id = request.Id,
            Name = request.Name,
            Email = request.Email
        });
        await _db.SaveChangesAsync();

        return new();
    }

    public override async Task<Account> Read(AccountId request, ServerCallContext context)
    {
        var account = await FindAsync(request);
        return new()
        {
            Id = new(account.Id),
            Name = account.Name,
            Email = account.Email
        };
    }

    public override async Task<Empty> Update(Account request, ServerCallContext context)
    {
        var account = await FindAsync(request.Id);
        account.Name = request.Name;
        account.Email = request.Email;
        await _db.SaveChangesAsync();

        return new();
    }

    public override async Task<Empty> Delete(AccountId request, ServerCallContext context)
    {
        _db.Accounts.Remove(await FindAsync(request));
        await _db.SaveChangesAsync();

        return new();
    }

    private async Task<AccountEntity> FindAsync(AccountId accountId)
        => await _db.Accounts.FirstOrDefaultAsync(x => x.Id == accountId)
        ?? throw new RpcException(new Status(StatusCode.NotFound, "not_found"));
}
