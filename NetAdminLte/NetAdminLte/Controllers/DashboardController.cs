using Microsoft.AspNetCore.Mvc;
using NetAdminLte.Models;
using System.Security.Claims;

namespace NetAdminLte.Controllers;

[Route("auth", Name = "Auth")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        var model = new DashboardIndexModel
        {
            PageTitle = "Dashboard Overview",
            NavbarModel = new NavbarViewModel
            {
                UserName = User.Identity?.Name,
                UserRole = User.FindFirst(ClaimTypes.Role)?.Value
            },
            AsideModel = new AsideViewModel
            {
                MenuItems = GetMenuItemsBasedOnRole(User.FindFirst(ClaimTypes.Role)?.Value)
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