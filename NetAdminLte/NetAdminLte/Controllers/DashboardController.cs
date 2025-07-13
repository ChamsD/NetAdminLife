using Microsoft.AspNetCore.Mvc;

namespace NetAdminLte.Controllers;

[Route("auth", Name = "Auth")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View("~/Pages/Dashboard/Index.cshtml");
    }
}