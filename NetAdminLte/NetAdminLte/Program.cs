// ... [previous using statements remain the same]

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using NetAdminLte.Common;
using NetAdminLte.Repositories;
using NetAdminLte.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });


    // Configuration setup
    IConfiguration config = builder.Configuration;
    string connectionString = string.Empty;

    // Try to load from XML config first
    string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Configurations", "Database-DEV.config");
    if (File.Exists(xmlFilePath))
    {
        try
        {
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddXmlFile(xmlFilePath, optional: false, reloadOnChange: true)
                .Build();

            connectionString = config.GetConnectionString("MainStr") ??
                             config.GetSection("connectionStrings:add")
                                   .GetChildren()
                                   .FirstOrDefault(x => x["name"] == "MainStr")?["connectionString"];
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading configuration from XML");
        }
    }

    // Fallback to appsettings.json if XML fails or doesn't exist
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = config.GetConnectionString("MainStr");
    }

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("MainStr connection string is not configured in any configuration source.");
    }

    // Register services
    builder.Services.AddSingleton<IConfiguration>(config);
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

    builder.Services.AddCors(opt => opt.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    builder.Services.AddSession(options =>
    {
        options.Cookie.Name = "MyApp.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.IsEssential = true;
        options.IdleTimeout = TimeSpan.FromMinutes(20);
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys")))
        .SetApplicationName("MyApp")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

    builder.Services.AddTransient<AuthService>();

    builder.Services.AddScoped<AuthRepositories>();

    builder.Services.AddHttpClient();
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor(o => o.DetailedErrors = true);
    builder.Services.AddControllers();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();
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