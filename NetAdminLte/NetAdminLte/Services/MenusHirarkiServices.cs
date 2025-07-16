// NetAdminLte.Services.MenusHirarkiServices.cs
using NetAdminLte.Models;
using NetAdminLte.Repositories;
using System.Text.Json;

namespace NetAdminLte.Services;

public class MenusHirarkiServices
{
    private readonly MenuHirarki _menuHirarki;

    public MenusHirarkiServices(MenuHirarki menuHirarki)
    {
        _menuHirarki = menuHirarki;
    }

    public List<ListMenu> GetMenu()
    {
        var rawMenus = _menuHirarki.GetMenu();
        return (List<ListMenu>)rawMenus;
    }

    public List<ListMenu> GetMenuByRole(string role)
    {
        var rawMenus = _menuHirarki.GetMenuByRole(role); 
        return (List<ListMenu>)rawMenus;
    }

}