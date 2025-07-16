using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetAdminLte.Models;
public class MasterFunction
{
    [Key]
    public int id { get; set; } = 0;
    public int ID_MENU { get; set; } = 0;
    public string NAME_MENU { get; set; } = string.Empty;
    public string SHOWING_LABEL { get; set; } = string.Empty;
    public string TYPE_MENU { get; set; } = string.Empty;
    public int CHILD_MENU { get; set; }
    public bool IS_ACTIVATED { get; set; }
    [NotMapped]
	public DateTime CREATED_DATE { get; set; }
    [NotMapped]
    public DateTime UPDATED_DATE { get; set; }
    [NotMapped]
	public DateTime DELETE_DATE { get; set; }

}