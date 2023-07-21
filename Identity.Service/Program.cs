var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddDbContext<IdentityDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("Database")));

var app = builder.Build();
app.MapHealthChecks("health");
app.MapGrpcService<AccountsV1Service>();
app.MapGrpcService<TeamsV1Service>();
app.MapGrpcService<AccountsV2Service>();
app.MapGrpcService<TeamsV2Service>();
app.MapGrpcReflectionService();
await app.Services.GetRequiredService<IdentityDbContext>().Database.EnsureCreatedAsync();

app.Run();
