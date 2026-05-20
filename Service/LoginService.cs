using lib.DtoModel.LoginDto;
using lib.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Service
{
    public class LoginService: ILoginService
    {
        private readonly string _connectionString;

        public LoginService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<string>> GetLoginTypesAsync()
        {
            List<string> loginTypes = new();

            using SqlConnection con = new(_connectionString);

            string sql = @"select distinct LoginType 
                           from UserMaster
                           where ApplicationName='Library'
                           and ApplicationType='Windows'";

            SqlDataAdapter da = new(sql, con);

            DataSet ds = new();

            da.Fill(ds, "LoginType");

            if (ds.Tables["LoginType"].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables["LoginType"].Rows)
                {
                    loginTypes.Add(row["LoginType"].ToString());
                }
            }

            return await Task.FromResult(loginTypes);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest model)
        {
            using SqlConnection con = new(_connectionString);

            await con.OpenAsync();

            string sql = @"SELECT * FROM UserMaster
                           WHERE UserName=@UserName
                           AND Password=@Password
                           AND LoginType=@LoginType
                           AND ApplicationName='Library'
                           AND ApplicationType='Windows'";

            SqlDataAdapter da = new(sql, con);

            da.SelectCommand.Parameters.AddWithValue("@UserName", model.UserName);

            da.SelectCommand.Parameters.AddWithValue("@Password", model.Password);

            da.SelectCommand.Parameters.AddWithValue("@LoginType", model.LoginType);

            DataSet ds = new();

            da.Fill(ds, "UserMaster");

            if (ds.Tables["UserMaster"].Rows.Count == 0)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Wrong Username/Password"
                };
            }

            var userRow = ds.Tables["UserMaster"].Rows[0];

            string rememberPSW = userRow["RememberPSW"] != DBNull.Value
                ? userRow["RememberPSW"].ToString()
                : "";

            string menuSql = @"SELECT LibMenuITEMS.ID_ITEM,
                                      LibMenuITEMS.NAME,
                                      LibMenuITEMS.HIERAR,
                                      LibMenuITEMS.TEXT,
                                      LibMenuITEMS.[DESC],
                                      LibMenuITEMS.FUNC
                               FROM LibMenuITEMS
                               INNER JOIN LibMenuPERMS
                               ON LibMenuPERMS.ID_ITEM = LibMenuITEMS.ID_ITEM
                               AND LibMenuPERMS.ApplicationName = LibMenuITEMS.ApplicationName
                               WHERE LibMenuPERMS.ID_USER = @UserName
                               AND LibMenuPERMS.LoginType = @LoginType
                               AND LibMenuITEMS.ApplicationName = 'Library'
                               ORDER BY LibMenuPERMS.ID_ITEM";

            SqlDataAdapter adap = new(menuSql, con);

            adap.SelectCommand.Parameters.AddWithValue("@UserName", model.UserName);

            adap.SelectCommand.Parameters.AddWithValue("@LoginType", model.LoginType);

            DataSet dsUser = new();

            adap.Fill(dsUser, "Items");

            var menuItems = dsUser.Tables["Items"]
                .AsEnumerable()
                .Select(r => new MenuItemResponse
                {
                    IdItem = r["ID_ITEM"].ToString(),
                    Name = r["NAME"].ToString(),
                    Hierar = r["HIERAR"].ToString(),
                    Text = r["TEXT"].ToString(),
                    Desc = r["DESC"].ToString(),
                    Func = r["FUNC"].ToString()
                }).ToList();

            return new LoginResponse
            {
                Success = true,
                Message = "Login Successful",
                UserName = model.UserName,
                LoginType = model.LoginType,
                RememberPSW = rememberPSW,
                MenuItems = menuItems
            };
        }

        public async Task<List<MenuNode>> GetDynamicMenuAsync()
        {
            List<MenuItemDto> items = new();

            using SqlConnection con = new(_connectionString);

            string sql = @"SELECT ID_ITEM,NAME,HIERAR,TEXT,[DESC],FUNC
                           FROM LibMenuITEMS
                           WHERE ApplicationName='Library'
                           ORDER BY ID_ITEM";

            SqlDataAdapter da = new(sql, con);

            DataSet ds = new();

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

            return await Task.FromResult(BuildMenuTree(items));
        }

        private List<MenuNode> BuildMenuTree(List<MenuItemDto> items)
        {
            Dictionary<string, MenuNode> dict = new();

            List<MenuNode> root = new();

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

            foreach (var item in items)
            {
                var node = dict[item.Hierar];

                if (item.Hierar.Contains("."))
                {
                    string parentKey =
                        item.Hierar.Substring(0, item.Hierar.LastIndexOf('.'));

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

