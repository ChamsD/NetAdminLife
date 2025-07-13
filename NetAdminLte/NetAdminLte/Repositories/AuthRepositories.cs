using NetAdminLte.Common;
using NetAdminLte.Models;
using NetAdminLte.Services;
using Serilog;
using System.Diagnostics;
using System.Text;
using WebApplicationTrial2.Models;

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
        Log.Information($"Logging from repo {loginView.Username}");
        Debug.Print($"Logging from repo {base64String}");
        var user = _dbContext.SystemUsers.FirstOrDefault(u => u.Name == loginView.Username && u.Passwd == base64String); 
        if (user != null)
        {

            var userList = new List<SystemUser> { user };
            
            foreach (var item in userList)
            {
                // Cek data, debug, atau logging    
                Log.Information($"Remote status db connection {item.Name}");
                Debug.Print($"Remote status db connection {item.Name}");
                Debug.WriteLine($"Remote status db connection {item.UserID}");
                Debug.Write($"Remote status db connection {item.Passwd}");
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