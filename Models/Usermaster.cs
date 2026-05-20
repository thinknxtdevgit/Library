using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lib.Models
{
    [Table("UserMaster")]
    public class UserMaster
    {
        [Key]
    
        public long UserName { get; set; }

        public string Password { get; set; }

        public string LoginType { get; set; }

        public string ApplicationType { get; set; }

        public string ApplicationName { get; set; }

        public string CollegeName { get; set; }

        public string RightsLevel { get; set; }

        public string RememberPSW { get; set; }
        public long UserMasterID { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

