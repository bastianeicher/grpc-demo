namespace Identity.Api.V2;

partial class AccountId
{
    public AccountId(Guid guid)
        => Uuid = guid.ToString();

    public static implicit operator Guid(AccountId accountId)
        => Guid.Parse(accountId.Uuid);
}
