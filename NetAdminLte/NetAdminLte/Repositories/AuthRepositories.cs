using NetAdminLte.Common;
using NetAdminLte.Models;
using System.Buffers.Text;
using System.Text;
using WebApplicationTrial2.Models;

namespace NetAdminLte.Repositories;
public class AuthRepositories
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _http;
    public AuthRepositories(AppDbContext dbContext, IHttpContextAccessor http)
    {
        _dbContext = dbContext;
        _http = http;
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
        var user = _dbContext.SystemUsers.FirstOrDefault(u => u.Name == loginView.Username && u.Passwd == base64String); 
        if (user != null)
        {
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