using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetAdminLte.Common;
using NetAdminLte.Models;
using NetAdminLte.Services;
using System.Security.Claims;
using WebApplicationTrial2.Models;

namespace NetAdminLte.Controllers;

[Route("auth", Name = "Auth")]
public class AuthController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _http;
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext dbContext, IHttpContextAccessor http, ILogger<AuthController> logger, AuthService authService)
    {
        _dbContext = dbContext;
        _http = http;
        _logger = logger;
        _authService = authService;
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
        if (result != null && result.Any())
        {
            var user = result.First(); 
            var claimsIdentity = new ClaimsIdentity(
                Enumerable.Empty<Claim>(),
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Dashboard");
        }
        else
        {
            TempData["Error"] = "Invalid username or password";
            return RedirectToAction("Index", "Auth");
        }
    }
}
