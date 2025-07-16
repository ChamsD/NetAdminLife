using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NetAdminLte.Common;
using NetAdminLte.Models;
using NetAdminLte.Models;
using NetAdminLte.Repositories;
using NetAdminLte.Services;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace NetAdminLte.Controllers;

[Route("auth", Name = "Auth")]
public class AuthController : Controller
{
    private readonly MenusHirarkiServices _menusHirarki;
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _http;
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext dbContext, IHttpContextAccessor http, ILogger<AuthController> logger, AuthService authService, MenusHirarkiServices menusHirarki)
    {
        _dbContext = dbContext;
        _http = http;
        _logger = logger;
        _authService = authService;
        _menusHirarki = menusHirarki;
    }

    [HttpGet("login", Name = "Login")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Login";
        return View("~/Pages/Auth/Index.cshtml");
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        var result = await _authService.LoginAsync(loginViewModel);
        //var list_menu = await _menusHirarki.GetMenu();
        if (result != null && result.Any())
        {

            //HttpContext.Response.Cookies.Append("UserMenuCookie", JsonSerializer.Serialize(dataList), new CookieOptions
            //{
            //    HttpOnly = true,
            //    Secure = true, // Set to true in production (HTTPS)
            //    SameSite = SameSiteMode.Strict,
            //    Expires = DateTimeOffset.UtcNow.AddHours(1) // Expiry as needed
            //});

            var response = result[0];
            _logger.LogInformation(JsonSerializer.Serialize(response.data));
            Debug.Print(JsonSerializer.Serialize(response.data));
            //var listMenu = _menusHirarki.GetMenu(response.data[0]);
            var statusCode = int.Parse(response.statusCode);
            var msg = response.msg;
            if (statusCode == 200)
            {
                if (response.data is IEnumerable<ResultResponse> dataList)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginViewModel.Username),
                        new Claim(ClaimTypes.Role, "0") // You can change based on your model
                    };
                    ViewData["title"] = "Auth Security | Vault";
                    var checkData = dataList.ToList();
                    // return Ok(checkData);
                    return Redirect("/Dashboard");
                }
                // return Ok(response.data);
            }
            else
            {
                // return StatusCode(statusCode, msg);
                ViewData["Msg"] = "Invalid password username not recorded!";
                return View("~/Pages/Auth/Index.cshtml");
            }
        }
        return BadRequest("Login failed or no data returned.");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LoginViewModel loginViewModel)
    { 
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("AuthToken");
        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
        } 
        return RedirectToAction("Index", "Auth");
    }

}
