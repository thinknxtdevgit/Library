using lib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Controllers
{
    public class LoginController : Controller
    {
        private readonly string _connectionString;

        public LoginController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        // [HttpGet("/") ]
        [HttpGet("/Login")] 
        public IActionResult Login()
        {
            return View();
        } 

        [HttpGet]
        [Route("api/Login/GetLoginTypes")]
       
        public IActionResult GetLoginTypes()
        {
            List<string> loginTypes = new List<string>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = "select distinct LoginType from UserMaster where ApplicationName='Library' and ApplicationType='Windows'";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataSet ds = new DataSet();

                da.Fill(ds, "LoginType");

                if (ds.Tables["LoginType"].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables["LoginType"].Rows.Count; i++)
                    {
                        loginTypes.Add(ds.Tables["LoginType"].Rows[i]["LoginType"].ToString());
                    }
                }
            }

            return Ok(loginTypes);
        }




        [HttpPost("api/Login/EnterOk")]
        public IActionResult EnterOk([FromBody] Usermaster model)
        {
            // -------- VALIDATION --------
            if (string.IsNullOrEmpty(model.UserName))
                return BadRequest("You must enter a user name");

            if (string.IsNullOrEmpty(model.Password))
                return BadRequest("You must enter a password");

            if (string.IsNullOrEmpty(model.LoginType))
                return BadRequest("You must specify a level");

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // -------- LOGIN CHECK --------
                string sql = @"SELECT * FROM UserMaster 
                               WHERE UserName=@UserName 
                               AND Password=@Password 
                               AND LoginType=@LoginType 
                               AND ApplicationName='Library' 
                               AND ApplicationType='Windows'";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                da.SelectCommand.Parameters.AddWithValue("@UserName", model.UserName);
                da.SelectCommand.Parameters.AddWithValue("@Password", model.Password);
                da.SelectCommand.Parameters.AddWithValue("@LoginType", model.LoginType);

                DataSet ds = new DataSet();
                da.Fill(ds, "UserMaster");

                if (ds.Tables["UserMaster"].Rows.Count == 0)
                    return Unauthorized("Wrong UserName/Password");

                var userRow = ds.Tables["UserMaster"].Rows[0];

                string rememberPSW = userRow["RememberPSW"] != DBNull.Value? userRow["RememberPSW"].ToString(): "";

                // -------- MENU ITEMS --------
                string ssql = @"SELECT LibMenuITEMS.ID_ITEM, LibMenuITEMS.NAME,
                                       LibMenuITEMS.HIERAR, LibMenuITEMS.TEXT,
                                       LibMenuITEMS.[DESC], LibMenuITEMS.FUNC
                                FROM LibMenuITEMS
                                INNER JOIN LibMenuPERMS
                                ON LibMenuPERMS.ID_ITEM = LibMenuITEMS.ID_ITEM
                                AND LibMenuPERMS.ApplicationName = LibMenuITEMS.ApplicationName
                                WHERE LibMenuPERMS.ID_USER = @UserName
                                AND LibMenuPERMS.LoginType = @LoginType
                                AND LibMenuITEMS.ApplicationName = 'Library'
                                ORDER BY LibMenuPERMS.ID_ITEM";

                SqlDataAdapter adap = new SqlDataAdapter(ssql, con);
                adap.SelectCommand.Parameters.AddWithValue("@UserName", model.UserName);
                adap.SelectCommand.Parameters.AddWithValue("@LoginType", model.LoginType);

                DataSet dsUser = new DataSet();
                adap.Fill(dsUser, "Items");

                // 🔥 IMPORTANT FIX: DataTable → List (ERROR FIX)
                var menuItems = dsUser.Tables["Items"]
                    .AsEnumerable()
                    .Select(r => new
                    {
                        IdItem = r["ID_ITEM"].ToString(),
                        Name = r["NAME"].ToString(),
                        Hierar = r["HIERAR"].ToString(),
                        Text = r["TEXT"].ToString(),
                        Desc = r["DESC"].ToString(),
                        Func = r["FUNC"].ToString()
                    }).ToList();

                if (menuItems.Count > 0)
                {
                    return Ok(new
                    {
                        message = "Login successful",
                        //user = model.UserName,
                        //loginType = model.LoginType,
                        //rememberPSW = rememberPSW,
                       
                    });
                }
                else
                {
                    return BadRequest("You have no rights. Please contact Admin");
                }
            }
        }



        [HttpGet("api/Login/GetDynamicMenu")]
        public IActionResult GetDynamicMenu()
        {
            List<MenuItemDto> items = new List<MenuItemDto>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = @"SELECT ID_ITEM,NAME,HIERAR,TEXT,[DESC],FUNC 
                       FROM LibMenuITEMS 
                       WHERE ApplicationName='Library' 
                       ORDER BY Id_Item";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataSet ds = new DataSet();
                da.Fill(ds, "Items");

                foreach (DataRow row in ds.Tables["Items"].Rows)
                {
                    items.Add(new MenuItemDto
                    {
                        IdItem = row["ID_ITEM"].ToString(),
                        Name = row["NAME"].ToString(),
                        Hierar = row["HIERAR"].ToString(),
                        Text = row["TEXT"].ToString(),
                        Desc = row["DESC"].ToString(),
                        Func = row["FUNC"].ToString()
                    });
                }
            }

            var tree = BuildMenuTree(items);

            return Ok(tree);
        }

        private List<MenuNode> BuildMenuTree(List<MenuItemDto> items)
        {
            Dictionary<string, MenuNode> dict = new Dictionary<string, MenuNode>();
            List<MenuNode> root = new List<MenuNode>();

            // Step 1: create nodes
            foreach (var item in items)
            {
                dict[item.Hierar] = new MenuNode
                {
                    Id = item.IdItem,
                    Text = item.Text,
                    Func = item.Func,
                    Hierar = item.Hierar
                };
            }

            // Step 2: build hierarchy
            foreach (var item in items)
            {
                var node = dict[item.Hierar];

                if (item.Hierar.Contains("."))
                {
                    string parentKey = item.Hierar.Substring(0, item.Hierar.LastIndexOf('.'));

                    if (dict.ContainsKey(parentKey))
                    {
                        dict[parentKey].Children.Add(node);
                    }
                    else
                    {
                        root.Add(node);
                    }
                }
                else
                {
                    root.Add(node);
                }
            }

            return root;
        }
    }
}
