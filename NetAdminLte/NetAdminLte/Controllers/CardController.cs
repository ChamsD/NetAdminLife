using Microsoft.AspNetCore.Mvc;

namespace NetAdminLte.Controllers;

[Route("Card", Name ="Card")]
public class CardController : Controller
{
    [Route("CardUser", Name ="CardUser")]
    public IActionResult Index()
    {
        return View("~/Pages/Card/Index.cshtml");
    }
}

