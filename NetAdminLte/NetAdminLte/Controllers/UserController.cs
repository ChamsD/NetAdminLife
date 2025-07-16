using Microsoft.AspNetCore.Mvc;

namespace NetAdminLte.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
