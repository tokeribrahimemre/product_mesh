using ProductMesh.Auth.Application;
using ProductMesh.Auth.API.Extensions;
using ProductMesh.Auth.API.Middleware;
using ProductMesh.Auth.Domain.Entities;
using ProductMesh.Auth.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "AuthService")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // 12-Factor: configuration from environment variables
    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddControllers();
    builder.Services.AddAuthApplication();
    builder.Services.AddAuthInfrastructure(builder.Configuration);

    // Policy-Based Authorization: different authorization levels
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(AppPolicies.CanManageProducts, policy =>
            policy.RequireRole(AppRoles.Admin, AppRoles.User));

    var app = builder.Build();

    // Seed roles and admin user on startup
    await app.Services.SeedRolesAndAdminAsync();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Auth Service starting on port {Port}", builder.Configuration["ASPNETCORE_URLS"] ?? "5001");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Auth Service terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
