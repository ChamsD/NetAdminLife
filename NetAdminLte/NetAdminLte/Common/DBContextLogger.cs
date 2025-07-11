using Microsoft.EntityFrameworkCore;

namespace NetAdminLte.Common;

public class DBContextLogger : IHostedService
{

    private readonly IServiceProvider _serviceProvider;

    public DBContextLogger(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var connStr = dbContext.Database.GetDbConnection().ConnectionString;
            Console.WriteLine($"Verified Connection String at Startup: {connStr}");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
