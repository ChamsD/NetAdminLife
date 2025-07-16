using NetAdminLte.Models;
using NetAdminLte.Repositories;
using Serilog;
using NetAdminLte.Models;
namespace NetAdminLte.Services;
public class AuthService
{
    private readonly AuthRepositories _authRepositories;
    public AuthService(AuthRepositories authRepositories)
    {
        _authRepositories = authRepositories;
    }
    public async Task<List<ResultResponse>> LoginAsync(LoginViewModel loginView)
    {
        var result = new List<ResultResponse>
        {
            new ResultResponse
            {
                data = _authRepositories.checkUser(loginView),
                msg = "Login successful",
                statusCode = "200"
            }
        };
        return result;
    }
}