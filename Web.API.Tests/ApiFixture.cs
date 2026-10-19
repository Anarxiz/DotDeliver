using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;
using Testcontainers.PostgreSql;
using Web.API.Tests;
using Web.API.Tests.Extensions;
using Web.Application.Contracts.Auth;
using Web.Application.Interfaces.Repository;
using Web.Application.Interfaces.Service;
using Web.Domain.Enums;
using Web.Infrastructure;
using Xunit;

public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ShopDbContext>>();
            services.AddDbContext<ShopDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
            services.Configure<RateLimiterOptions>(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("login", context =>
                    RateLimitPartition.GetNoLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown"));
            });

            services.RemoveAll<INotificationChannel>();
            services.AddScoped<INotificationChannel, FakeNotificationChannel>();
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__ShopDbContext", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("JwtSettings__SecretKey", "test-secret-key-for-integration-tests-only-1234567890");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "TestAudience");
        Environment.SetEnvironmentVariable("JwtSettings__ExpiryInMinutes", "60");

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        await db.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task<HttpClient> CreateAdminClientAsync()
    {
        using var scope = Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var email = TestDataFactory.UniqueEmail();
        var phone = TestDataFactory.UniquePhone();
        var (hash, salt) = passwordHasher.HashPassword("Password1!");

        var registerRequest = new RegisterRequest(
            "Админ", "Админов", email, phone, Roles.Administrator, hash, salt);

        var userId = await userRepository.CreateAsync(registerRequest);
        var token = tokenService.GenerateToken(userId, email, Roles.Administrator);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}