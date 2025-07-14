using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetAdminLte.Common;
using NetAdminLte.Repositories;
using NetAdminLte.Services;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Logs"));

try
{
    // Logger configuration
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7)
        .Enrich.FromLogContext()
        .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Authentication setup
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        });

    // Connection string resolution with fallback logic
    string connectionString = await GetConnectionStringWithFallback();

    // Database configuration
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
        options.EnableSensitiveDataLogging();
    });

    // Additional services
    builder.Services.AddCors(opt => opt.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    builder.Services.AddSession(options =>
    {
        options.Cookie.Name = "MyApp.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.IdleTimeout = TimeSpan.FromMinutes(20);
    });

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys")));

    // Application services
    builder.Services.AddTransient<AuthService>();
    builder.Services.AddScoped<AuthRepositories>();
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddControllers();

    var app = builder.Build();

    // Middleware pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapRazorPages();
    app.MapControllers();
    app.MapBlazorHub();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Helper method for connection string with fallback
async Task<string> GetConnectionStringWithFallback()
{
    string prodConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "Configurations", "Database-PROD.config");
    string devConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "Configurations", "Database-DEV.config");

    // Try production connection first
    if (File.Exists(prodConfigPath))
    {
        try
        {
            var prodConfig = new ConfigurationBuilder()
                .AddXmlFile(prodConfigPath)
                .Build();

            var prodConnection = prodConfig.GetConnectionString("MainStr");

            if (await TestConnection(prodConnection))
            {
                Log.Information("Using PRODUCTION database connection");
                return prodConnection;
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Production config failed: {ex.Message}");
        }
    }

    // Fallback to development connection
    if (File.Exists(devConfigPath))
    {
        try
        {
            var devConfig = new ConfigurationBuilder()
                .AddXmlFile(devConfigPath)
                .Build();

            var devConnection = devConfig.GetConnectionString("local");

            if (await TestConnection(devConnection))
            {
                Log.Warning("Falling back to DEVELOPMENT database connection");
                return devConnection;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Development config failed: {ex.Message}");
        }
    }

    throw new InvalidOperationException("No valid database connection could be established");
}

// Connection test method
async Task<bool> TestConnection(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return false;

    try
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return true;
    }
    catch
    {
        return false;
    }
}