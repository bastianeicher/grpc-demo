namespace Identity.Api.V1;

partial class TeamId
{
    public TeamId(Guid guid)
        => Uuid = guid.ToString();

    public static implicit operator Guid(TeamId teamId)
        => Guid.Parse(teamId.Uuid);
}
