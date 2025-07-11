using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NetAdminLte.Models;
using System.Diagnostics;

namespace NetAdminLte.Pages.Auth;

public class AuthModel : PageModel
{
    [BindProperty]
    public LoginViewModel LoginViewModel { get; set; } = new LoginViewModel();

    public void OnGet()
    {
    }
}