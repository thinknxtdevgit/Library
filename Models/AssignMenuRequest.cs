namespace lib.Models
{
    public class AssignMenuRequest
    {
        public string UserId { get; set; }
        public string LoginType { get; set; }
        public List<MenuItemDto> Menus { get; set; }
    }
}
