using FluentAssertions.Common;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using NetAdminLte.Models;
using System.Configuration;

namespace NetAdminLte.Common;
public class AppDbContext : DbContext
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration; 
    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("MainStr");
        Console.WriteLine($"Using connection string: {_connectionString}");
    }
    public DbSet<LoginViewModel> LoginViewModels { get; set; } = default!; 
    public DbSet<SystemUser> SystemUsers { get; set; } = default!;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string useRemote = Environment.GetEnvironmentVariable("USE_REMOTE");
            string connStr = useRemote == "true"
                ? _configuration.GetConnectionString("MainStr")
                : _configuration.GetConnectionString("local");

            optionsBuilder.UseSqlServer(connStr, sql => sql.EnableRetryOnFailure());
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoginViewModel>().HasNoKey();
        modelBuilder.Entity<SystemUser>().ToTable("SystemUser");
        base.OnModelCreating(modelBuilder);
    }
}