using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetAdminLte.Common;
using NetAdminLte.Repositories;
using NetAdminLte.Services;
using Serilog;
using Serilog.Events;
using System.Xml;
using System.IO;

try
{
    var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
    Directory.CreateDirectory(logDirectory);

    string initialLogFile = Path.Combine(logDirectory, "log-init.txt");
    if (!File.Exists(initialLogFile))
    {
        File.WriteAllText(initialLogFile, $"Log initialized at: {DateTime.Now}");
    }

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(logDirectory, "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1))
        .Enrich.FromLogContext()
        .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

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

    string protectionKeysDir = Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys");
    Directory.CreateDirectory(protectionKeysDir);

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(protectionKeysDir))
        .SetApplicationName("NetAdminLte");

    
    builder.Services.AddTransient<AuthService>(); // user auth
    builder.Services.AddScoped<AuthRepositories>();

    builder.Services.AddTransient<MenusHirarkiServices>(); // menu listed data
    builder.Services.AddScoped<MenuHirarki>();



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
    Directory.CreateDirectory(configDirectory);

    string configProd = Path.Combine(configDirectory, "Database-PROD.config");
    string configDev = Path.Combine(configDirectory, "Database-DEV.config");

    if (!File.Exists(configProd))
    {
        throw new FileNotFoundException($"Configuration file not found: {configProd}");
    }

    var xmlDocProd = new XmlDocument();
    var xmlDocDev = new XmlDocument();

    xmlDocProd.Load(configProd);
    xmlDocDev.Load(configDev);

    var conStringNodeProd = xmlDocProd.SelectSingleNode("//connectionStrings/add[@name='MainStr']");
    var conStringNodeDev = xmlDocDev.SelectSingleNode("//connectionStrings/add[@name='local']");

    if (conStringNodeProd == null || conStringNodeDev == null)
    {
        throw new InvalidOperationException("No valid connection string found in configuration file");
    }

    string conStringProd = conStringNodeProd.Attributes["connectionString"].Value;
    string conStringDev = conStringNodeDev.Attributes["connectionString"].Value;

    if (string.IsNullOrWhiteSpace(conStringProd) || string.IsNullOrWhiteSpace(conStringDev))
    {
        throw new InvalidOperationException("Connection string is empty in configuration file");
    }

    if (await TestConnection(conStringProd))
    {
        return conStringProd;
    }

    return conStringDev;
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
