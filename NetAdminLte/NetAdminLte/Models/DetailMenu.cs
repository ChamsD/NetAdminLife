// NetAdminLte.Models.MenuModel.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace NetAdminLte.Models;

public class ListMenu
{
    public int ID { get; set; }
    public int ID_MENU { get; set; }
    public string NAME_MENU { get; set; }
    public string SHOWING_LABEL { get; set; }
    public string TYPE_MENU { get; set; }
    public bool IS_ACTIVATED { get; set; }
    public int CHILD_MENU { get; set; }
    public DateTime CREATED_DATE { get; set; }
    [NotMapped]
    public DateTime UPDATED_DATE { get; set; }
    [NotMapped]
    public DateTime DELETE_DATE { get; set; }
    [NotMapped]
    public int Level { get; set; }
    [NotMapped]
    public List<ListMenu>? sub_menus { get; set; }
}

public class DetailMenu
{
    public int ID { get; set; }
    public int ID_MENU { get; set; }
    public string NAME_MENU { get; set; }
    public string SHOWING_LABEL { get; set; }
    public string TYPE_MENU { get; set; }
    public int CHILD_MENU { get; set; }
    public bool IS_ACTIVATED { get; set; }
    public DateTime CREATED_DATE { get; set; }
    [NotMapped]
	public DateTime UPDATED_DATE {  get; set; }
    [NotMapped]
	public DateTime DELETE_DATE { get; set; }
    [NotMapped]
    public string? sub_menus { get; set; }
}