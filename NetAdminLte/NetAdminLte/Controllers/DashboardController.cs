// NetAdminLte.Controllers.DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using NetAdminLte.Models;
using NetAdminLte.Services;
using System.Security.Claims;

namespace NetAdminLte.Controllers;

[Route("Dashboard")]
public class DashboardController : Controller
{
    private readonly MenusHirarkiServices _menusHirarki;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(MenusHirarkiServices menusHirarki, ILogger<DashboardController> logger)
    {
        _menusHirarki = menusHirarki;
        _logger = logger;
    } 

    [HttpGet("")]
    public IActionResult Index()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "00";
        var listMenu = _menusHirarki.GetMenu();
        // var listMenu = _menusHirarki.GetMenuByRole(role);
        _logger.LogInformation($"DASH CONTROL 27 {listMenu?.Count}");
        var model = new DashboardIndexModel
        {
            PageTitle = "Dashboard Overview",
            NavbarModel = new NavbarViewModel
            {
                UserName = User.Identity?.Name,
                UserRole = role,
                navbarMenu = null
            },
            AsideModel = new AsideViewModel
            {
                MenuItems = GetMenuItemsBasedOnRole(role)
            }
        };

        return View("~/Pages/Dashboard/Index.cshtml", model);
    }

    [HttpGet("Master", Name = "Master")]
    public IActionResult Master()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "00";
        var listMenu = _menusHirarki.GetMenu(); 
        _logger.LogInformation($"DASH CONTROL 51 {listMenu?.Count}");
        var model = new DashboardIndexModel
        {
            PageTitle = "Dashboard Overview",
            NavbarModel = new NavbarViewModel
            {
                UserName = User.Identity?.Name,
                UserRole = role,
                navbarMenu = null
            },
            AsideModel = new AsideViewModel
            {
                MenuItems = GetMenuItemsBasedOnRole(role)
            }
        };

        return View("~/Pages/Dashboard/Index.cshtml", model);
    }



    private List<MenuItem> GetMenuItemsBasedOnRole(string? role)
    {
        var menuItems = new List<MenuItem>();

        if (string.IsNullOrEmpty(role)) return menuItems;

        switch (role.ToLower())
        {
            case "admin":
                menuItems.AddRange(new List<MenuItem>
                {
                    new MenuItem { Text = "Dashboard", Icon = "bi bi-speedometer2", Url = "/dashboard" },
                });
                break;
        }

        return menuItems;
    }
}

//