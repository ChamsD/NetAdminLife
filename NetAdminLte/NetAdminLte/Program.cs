using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetAdminLte.Common;
using NetAdminLte.Repositories;
using NetAdminLte.Services;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Xml;

try
{
    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Logs"));

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

    // Add HttpContextAccessor first
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });

    string connectionString = await GetConnectionString(builder);
    Log.Information("Database connection string: {ConnectionString}", connectionString);

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    });

    builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    builder.Services.AddSession(options =>
    {
        options.Cookie.Name = "NetAdminLte.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.IdleTimeout = TimeSpan.FromMinutes(30);
    });

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys")))
        .SetApplicationName("NetAdminLte");

    // Register services after HttpContextAccessor
    builder.Services.AddTransient<AuthService>();
    builder.Services.AddScoped<AuthRepositories>();

    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddControllers();

    var app = builder.Build();

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
    app.UseSession();

    app.MapRazorPages();
    app.MapControllers();
    app.MapBlazorHub();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

async Task<string> GetConnectionString(WebApplicationBuilder builder)
{
    string configDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Configurations");
    string configPath = Path.Combine(configDirectory, "Database-PROD.config");

    if (!File.Exists(configPath))
    {
        throw new FileNotFoundException($"Configuration file not found: {configPath}");
    }

    try
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(configPath);

        var connectionStringNode = xmlDoc.SelectSingleNode("//connectionStrings/add[@name='MainStr']");
        if (connectionStringNode == null)
        {
            connectionStringNode = xmlDoc.SelectSingleNode("//connectionStrings/add[@name='local']");
        }

        if (connectionStringNode == null)
        {
            throw new InvalidOperationException("No valid connection string found in configuration file");
        }

        string connectionString = connectionStringNode.Attributes["connectionString"].Value;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string is empty in configuration file");
        }

        if (await TestConnection(connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException("Database connection test failed");
    }
    catch (XmlException ex)
    {
        throw new InvalidOperationException($"Invalid XML configuration file: {ex.Message}");
    }
}

async Task<bool> TestConnection(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return false;
    }

    try
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var cmd = new SqlCommand("SELECT 1", connection);
        await cmd.ExecuteScalarAsync();
        return true;
    }
    catch (Exception ex)
    {
        Log.Warning("Connection test failed: {ErrorMessage}", ex.Message);
        return false;
    }
}