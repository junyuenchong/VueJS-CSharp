using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Services.Products;
using Backend.Services.Auth;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Use Development environment when running locally (dotnet run)
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
{
    // Do not override explicit Test environment.
    if (builder.Environment.IsProduction())
    builder.Environment.EnvironmentName = Microsoft.Extensions.Hosting.Environments.Development;
}

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT auth setup
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "dev-only-secret-change-me";
// Ensure key is at least 256 bits; hash if too short.
var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtSecret);
if (jwtKeyBytes.Length < 32)
{
    jwtKeyBytes = SHA256.HashData(jwtKeyBytes);
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

// Rate limit auth endpoints to reduce brute force
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// CORS for browser client
var corsPolicyName = "AllowClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        var clientUrl = builder.Configuration["CLIENT_URL"] ?? "http://localhost:5173";
        var origins = new List<string> { clientUrl, clientUrl.Replace("localhost", "127.0.0.1") };
        
        // Also allow Kubernetes client ports when needed
        if (clientUrl.Contains("30073") || clientUrl.Contains("5173"))
        {
            origins.AddRange(new[] { "http://localhost:30073", "http://127.0.0.1:30073" });
        }
        
        policy.WithOrigins(origins.Distinct().ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // needed for cookie-based auth calls
    });
});

// DB connection: env var first, then appsettings
var connectionString = builder.Configuration["DB_CONNECTION_STRING"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "server=db;port=3306;database=crudapp;user=root;password=secret";

// Log basic DB info in Development (hide password)
if (builder.Environment.IsDevelopment())
{
    Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
    var safeConnectionString = connectionString.Replace("password=secret", "password=***");
    Console.WriteLine($"Connection String: {safeConnectionString}");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)), 
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Register app services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Run migrations and seed on startup (skip in Test)
if (!app.Environment.IsEnvironment("Test"))
{
var retries = 5;
var delay = TimeSpan.FromSeconds(5);
for (int i = 0; i < retries; i++)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Baseline for old DBs created with EnsureCreated (no migrations table)
                var creator = db.Database.GetService<IRelationalDatabaseCreator>();
                var productsExists = false;
                if (await creator.ExistsAsync())
                {
                    var conn = db.Database.GetDbConnection();
                    if (conn.State != System.Data.ConnectionState.Open)
                        await conn.OpenAsync();

                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText =
                        "SELECT COUNT(*) " +
                        "FROM INFORMATION_SCHEMA.TABLES " +
                        "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Products';";
                    var countObj = await cmd.ExecuteScalarAsync();
                    var count = Convert.ToInt32(countObj);
                    productsExists = count > 0;
                }

                if (productsExists)
                {
                    await db.Database.ExecuteSqlRawAsync(
                        "CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (" +
                        "`MigrationId` varchar(150) NOT NULL," +
                        "`ProductVersion` varchar(32) NOT NULL," +
                        "PRIMARY KEY (`MigrationId`)" +
                        ");");

                    // Mark InitialCreate as applied for legacy databases
                    await db.Database.ExecuteSqlRawAsync(
                        "INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) " +
                        "VALUES ('20251103000000_InitialCreate', '8.0.6');");
                }
            
            // Apply EF Core migrations
            await db.Database.MigrateAsync();
            Console.WriteLine("Database migrations applied successfully.");
            
            // Seed initial data
            await DbSeeder.SeedAsync(db);
            break;
        }
    }
    catch (Exception ex) when (i < retries - 1)
    {
        Console.WriteLine($"Database setup attempt {i + 1} failed: {ex.Message}. Retrying in {delay.TotalSeconds}s...");
        await Task.Delay(delay);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database setup failed after {retries} attempts: {ex.Message}");
        throw;
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

// Basic security headers for API responses
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-site";

    // Set CSP in non-Development
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
    }

    await next();
});

app.UseAuthentication();
app.UseCors(corsPolicyName);

// Simple Origin check for CSRF on unsafe methods
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var isUnsafe =
        HttpMethods.IsPost(method) ||
        HttpMethods.IsPut(method) ||
        HttpMethods.IsPatch(method) ||
        HttpMethods.IsDelete(method);

    if (isUnsafe)
    {
        var clientUrl = builder.Configuration["CLIENT_URL"] ?? "http://localhost:5173";
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            clientUrl,
            clientUrl.Replace("localhost", "127.0.0.1")
        };

        // Require Origin only for refresh/logout cookie endpoints
        var isAuthCookieEndpoint =
            context.Request.Path.StartsWithSegments("/api/Auth/refresh") ||
            context.Request.Path.StartsWithSegments("/api/Auth/logout");

        if (isAuthCookieEndpoint)
        {
            if (!context.Request.Headers.TryGetValue("Origin", out var originValues))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden (missing Origin)");
                return;
            }

            var origin = originValues.ToString();
            if (!allowed.Contains(origin))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden (invalid Origin)");
                return;
            }
        }
    }

    await next();
});

app.UseAuthorization();
app.MapControllers();

app.Run();

// Required for integration testing via WebApplicationFactory<TEntryPoint>
public partial class Program { }

