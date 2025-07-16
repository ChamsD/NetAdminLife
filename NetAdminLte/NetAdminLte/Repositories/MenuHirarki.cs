// NetAdminLte.Repositories.MenuHirarki.cs
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetAdminLte.Common;
using NetAdminLte.Models;
using Serilog;
using System.Diagnostics;
using System.Text.Json;
using NetAdminLte.Models;

namespace NetAdminLte.Repositories;

public class MenuHirarki
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<MenuHirarki> _logger;
    private readonly IConfiguration _configuration;

    public MenuHirarki(ILogger<MenuHirarki> logger, IConfiguration configuration, AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
    }
    public IList<ListMenu> GetMenu()
    {
        try
        {
            var allMenus = _dbContext.masterFunction
            .Select(m => new ListMenu
            {
                ID = m.id,
                ID_MENU = m.ID_MENU,
                NAME_MENU = m.NAME_MENU,
                SHOWING_LABEL = m.SHOWING_LABEL,
                TYPE_MENU = m.TYPE_MENU,
                CHILD_MENU = m.CHILD_MENU,
                IS_ACTIVATED = m.IS_ACTIVATED,
                Level = m.CHILD_MENU == 0 ? 0 : 1
            })
            .ToList();

            // Group into nested structure
            var nestedMenus = allMenus
                .Where(m => m.CHILD_MENU == 0)
                .Select(parent => new ListMenu
                {
                    ID = parent.ID,
                    ID_MENU = parent.ID_MENU,
                    NAME_MENU = parent.NAME_MENU,
                    SHOWING_LABEL = parent.SHOWING_LABEL,
                    TYPE_MENU = parent.TYPE_MENU,
                    //CHILD_MENU = parent.CHILD_MENU,
                    IS_ACTIVATED = parent.IS_ACTIVATED,
                    //Level = parent.Level,
                    sub_menus = allMenus
                        .Where(child => child.CHILD_MENU == parent.ID)
                        .ToList()
                })
                .ToList();

            // Serialize to JSON
            var json = JsonSerializer.Serialize(nestedMenus, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Console.WriteLine(json);

            //_logger.LogInformation($"REPO 35    {menuList[0].ID_MENU}");
            //Debug.Print($"{menuList[0].ID_MENU}");
            //_logger.LogInformation($"REPO 35    {JsonSerializer.Serialize(menuList)}");
            return nestedMenus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu");
            return new List<ListMenu>();
        }
    }



    public IList<ListMenu> GetMenuByRole(string role)
    {
        try
        {
            string sqlQuery = File.ReadAllText(@"Sql/DetailMenu.sql");
            //_logger.LogInformation(sqlQuery);
            //Debug.Print(sqlQuery);
            var parameters = new SqlParameter[]
            {
                //new SqlParameter("@V_TAGNUMBER", null),
            };
            var json = _dbContext.Database.SqlQueryRaw<ResultResponse2>(sqlQuery, parameters).ToList();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var menuList = JsonSerializer.Deserialize<List<ListMenu>>(json[0].data, options) ?? new List<ListMenu>();
            List<ListMenu> filteredMenus = menuList
                .Where(m => m.ID > 1)
                .Select(m =>
                {
                    if (m.sub_menus != null)
                    {
                        m.sub_menus = m.sub_menus.Where(sm => sm.ID > 1).ToList();
                    }
                    return m;
                })
                .ToList();

            // Now log the filtered result
            _logger.LogInformation(JsonSerializer.Serialize(filteredMenus));
            Debug.Print(JsonSerializer.Serialize(filteredMenus));


            return menuList;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get raw menu data.");
            return new List<ListMenu>();
        }
    }




}
//