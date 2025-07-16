namespace NetAdminLte.Models;

//
public class BaseLayoutModel
{
    public string? PageTitle { get; set; }
    public NavbarViewModel NavbarModel { get; set; }
    public AsideViewModel AsideModel { get; set; }
}

//
public class DashboardIndexModel : BaseLayoutModel
{
}

//
public class NavbarViewModel
{
    public string? UserName { get; set; }
    public string? UserRole { get; set; }
    public List<ListMenu>? navbarMenu { get; set; }
}

//
public class AsideViewModel
{
    public List<MenuItem> MenuItems { get; set; } = new();
}

//
public class MenuItem
{
    public string Text { get; set; }
    public string Icon { get; set; }
    public string Url { get; set; }
}

