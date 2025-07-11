using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetAdminLte.Models;
public class SystemUser
{
    [Key]
    public string UserID { get; set; }
    public string Passwd { get; set; }
    public string SiteCode { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string RoleLevel { get; set; }
    public string SystemUserAccessNo { get; set; }
    [NotMapped]
    public string SystemUser_01 { get; set; }
    [NotMapped]
    public string SystemUser_02 { get; set; }
    [NotMapped]
    public string SystemUser_03 { get; set; }
    [NotMapped]
    public string SystemUser_04 { get; set; }
    [NotMapped]
    public string SystemUser_05 { get; set; }
    [NotMapped]
    public string SystemUser_06 { get; set; }
    [NotMapped]
    public string SystemUser_07 { get; set; }
    [NotMapped]
    public string SystemUser_08 { get; set; }
    [NotMapped]
    public string SystemUser_09 { get; set; }
    [NotMapped]
    public string SystemUser_10 { get; set; }
    [NotMapped]
    public string SystemUser_11 { get; set; }
    [NotMapped]
    public string SystemUser_12 { get; set; }
    [NotMapped]
    public string SystemUser_13 { get; set; }
    [NotMapped]
    public string SystemUser_14 { get; set; }
    [NotMapped]
    public string SystemUser_15 { get; set; }
    public bool isUpdate { get; set; }
}