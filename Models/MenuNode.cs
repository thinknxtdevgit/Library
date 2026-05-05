namespace lib.Models
{
    public class MenuNode
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Func { get; set; }
        public string Hierar { get; set; }
        public List<MenuNode> Children { get; set; } = new List<MenuNode>();
    }
}
