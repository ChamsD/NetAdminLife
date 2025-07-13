using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetAdminLte.Common;
using NetAdminLte.Models;
using NetAdminLte.Services;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
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
            var response = result[0];
            var statusCode = int.Parse(response.statusCode);
            var msg = response.msg;
            if (statusCode == 200)
            {
                if (response.data is IEnumerable<ResultResponse> dataList)
                {
                    var checkData = dataList.ToList();
                    return Ok(checkData);
                }
                return Ok(response.data);
            }
            else
            {
                return StatusCode(statusCode, msg);
            }
        }

        return BadRequest("Login failed or no data returned.");
    }


}
