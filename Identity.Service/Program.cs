var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication("api-key")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("api-key", _ => {});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, GrpcAuthorizationMiddlewareResultHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddDbContext<IdentityDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("Database")));

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("health");
app.MapGrpcService<AccountsService>();
app.MapGrpcService<TeamsService>();
app.MapGrpcReflectionService();
await app.Services.GetRequiredService<IdentityDbContext>().Database.EnsureCreatedAsync();

app.Run();
