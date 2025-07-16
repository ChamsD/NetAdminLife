using NetAdminLte.Common;
using NetAdminLte.Models;
using NetAdminLte.Services;
using Serilog;
using System.Diagnostics;
using System.Text;
using NetAdminLte.Models;

namespace NetAdminLte.Repositories;
public class AuthRepositories
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<AuthRepositories> _logger;
    public AuthRepositories(AppDbContext dbContext, IHttpContextAccessor http, ILogger<AuthRepositories> logger)
    {
        _dbContext = dbContext;
        _http = http;
        _logger = logger;
    }
    private string GenerateAuthToken(SystemUser user)
    {
        // In production, use JWT or similar secure token
        return Guid.NewGuid().ToString() + user.UserID;
    }

    public List<ResultResponse> checkUser(LoginViewModel loginView)
    {
        var result = new List<ResultResponse>();
        string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(loginView.Password));
        _logger.LogInformation($"Logging from repo {base64String}");
        Log.Information($"Logging from repo {loginView.Username}");
        Debug.Print($"Logging from repo {base64String}");

        //  
        var user = _dbContext.SystemUsers
            .Where(u => u.UserID == loginView.Username && u.Passwd == base64String)
            .Select(u => new SystemUser
            {
                UserID = u.UserID,
                Name = u.Name ?? string.Empty,
                Passwd = u.Passwd ?? string.Empty,
                SiteCode = u.SiteCode ?? string.Empty,
                isUpdate = u.isUpdate,
                Role = u.Role ?? string.Empty,
                RoleLevel = u.RoleLevel ?? string.Empty,
                SystemUserAccessNo = u.SystemUserAccessNo ?? string.Empty,
                SystemUser_01 = u.SystemUser_01 ?? string.Empty,
                SystemUser_02 = u.SystemUser_02 ?? string.Empty
            })
            .FirstOrDefault(); 


        if (user != null)
        {
            _logger.LogInformation($"Remote status db connection {user.Name}");

            var userList = new List<SystemUser> { user };
            
            foreach (var item in userList)
            {
                // Cek data, debug, atau logging    
                Log.Information($"FOREACH status db connection {item.UserID}");
                Log.Information($"FOREACH status db connection {item.Passwd}");
                Log.Information($"FOREACH status db connection {item.RoleLevel}");
                Debug.Print($"FOREACH status db connection {item.Passwd}");
                Debug.Print($"FOREACH status db connection {item.UserID}");
                Debug.Print($"FOREACH status db connection {item.RoleLevel}");
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Prevent JavaScript access
                Secure = true, // Only send over HTTPS
                SameSite = SameSiteMode.Strict, // Prevent CSRF
                Expires = DateTime.UtcNow.AddDays(7) // Set expiration
            };

            // Set authentication cookie
            _http.HttpContext.Response.Cookies.Append(
                "AuthToken",
                GenerateAuthToken(user),
                cookieOptions);
            result.Add(new ResultResponse
            {
                data = user,
                msg = "Login successful",
                statusCode = "200"
            });
        }
        else
        {
            result.Add(new ResultResponse
            {
                data = null,
                msg = "Invalid username or password",
                statusCode = "401"
            });
        }
        
        return result;
    }


}