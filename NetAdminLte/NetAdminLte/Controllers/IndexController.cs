using Microsoft.AspNetCore.Mvc;
using NetAdminLte.Common;
namespace NetAdminLte.Controllers;

[Route("[controller]")]
public class IndexController : Controller
{
    private const string AuthCookieName = "X-CRF-COOKIE";
    private readonly AppDbContext _dbContext;

    public IndexController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        if (Request.Cookies.ContainsKey(AuthCookieName))
        {
            ViewData["Title"] = "Home page";
            return View("~/Pages/Index.cshtml");
        }
        else
        {
            return Redirect("Auth/Login");
        }
    }
}