
namespace lib.DtoModel.LoginDto
{
    public class LoginResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string UserName { get; set; }

        public string LoginType { get; set; }

        public string RememberPSW { get; set; }
        public List<string> Colleges { get; set; }
        public string CollegeName { get; set; }

        public List<MenuItemResponse> MenuItems { get; set; }
    }
}
