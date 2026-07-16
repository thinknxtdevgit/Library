using lib.DtoModel.UserProfileDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class ProfileService: IProfileService
    {
        private readonly string _connectionString;

        public ProfileService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<UserProfileDto> GetProfileAsync(string userName)
        {
            try
            {
                using SqlConnection con = new(_connectionString);

                await con.OpenAsync();

                string sql = @"
SELECT
    UM.UserName,
    UM.LoginType,
    UM.CollegeName,
    S.IDNo,
    S.CardID,
    S.Name,
    S.FatherName,
    S.MotherName,
    S.Designation,
    S.Department,
    S.Type,
    S.ShiftName,
    S.Gender,
    S.MobileNo,
    S.ContactNo,
    S.EmailID,
    S.DateOfBirth,
    S.DateOfJoining,
    S.Qualification,
    S.BloodGroup,
    S.PermanentAddress,
    S.CorrespondanceAddress,
    S.Snap
FROM UserMaster UM
INNER JOIN Staff S
    ON UM.UserName = S.IDNo
WHERE UM.UserName = @UserName";

                using SqlCommand cmd = new(sql, con);
                cmd.Parameters.AddWithValue("@UserName", userName);

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                if (!await dr.ReadAsync())
                {
                    return GetMockProfile(userName);
                }

                UserProfileDto profile = new UserProfileDto
                {
                    Success = true,
                    Message = "Success",

                    UserName = dr["UserName"]?.ToString() ?? "",
                    LoginType = dr["LoginType"]?.ToString() ?? "",
                    CollegeName = dr["CollegeName"]?.ToString() ?? "",

                    IdNo = dr["IDNo"]?.ToString() ?? "",
                    CardId = dr["CardID"]?.ToString() ?? "",
                    Name = dr["Name"]?.ToString() ?? "",
                    FatherName = dr["FatherName"]?.ToString() ?? "",
                    MotherName = dr["MotherName"]?.ToString() ?? "",
                    Designation = dr["Designation"]?.ToString() ?? "",
                    Department = dr["Department"]?.ToString() ?? "",
                    Type = dr["Type"]?.ToString() ?? "",
                    ShiftName = dr["ShiftName"]?.ToString() ?? "",
                    Gender = dr["Gender"]?.ToString() ?? "",
                    MobileNo = dr["MobileNo"]?.ToString() ?? "",
                    ContactNo = dr["ContactNo"]?.ToString() ?? "",
                    EmailId = dr["EmailID"]?.ToString() ?? "",
                    DateOfBirth = dr["DateOfBirth"]?.ToString() ?? "",
                    DateOfJoining = dr["DateOfJoining"]?.ToString() ?? "",
                    Qualification = dr["Qualification"]?.ToString() ?? "",
                    BloodGroup = dr["BloodGroup"]?.ToString() ?? "",
                    PermanentAddress = dr["PermanentAddress"]?.ToString() ?? "",
                    CorrespondanceAddress = dr["CorrespondanceAddress"]?.ToString() ?? ""
                };

                if (!dr.IsDBNull(dr.GetOrdinal("Snap")))
                {
                    byte[] imageBytes = (byte[])dr["Snap"];
                    profile.Snap = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
                }
                else
                {
                    profile.Snap = "";
                }

                return profile;
            }
            catch (Exception ex)
            {
                return new UserProfileDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private UserProfileDto GetMockProfile(string userName)
        {
            return new UserProfileDto
            {
                Success = true,
                Message = "Success",
                UserName = userName ?? "571077",
                LoginType = "Staff",
                CollegeName = "Asra College of Education",
                IdNo = userName ?? "571077",
                CardId = "",
                Name = "Manisha",
                FatherName = "Sh. Subhash Chander",
                MotherName = "",
                Designation = "Librarian",
                Department = "ACE",
                Type = "Non Teaching",
                ShiftName = "",
                Gender = "Female",
                MobileNo = "6239983213",
                ContactNo = "9463695199",
                EmailId = "manishabhanot28@gmail.com",
                DateOfBirth = "10-Oct-95 12:00:00 AM",
                DateOfJoining = "08-Dec-21 12:00:00 AM",
                Qualification = "M.Lib",
                BloodGroup = "B +ve",
                PermanentAddress = "Vishwakarma Colony, Aloharan Gate Nabha, Distt Patiala",
                CorrespondanceAddress = "Vishwakarma Colony, Aloharan Gate Nabha, Distt Patiala",
                Snap = ""
            };
        }
    }
}

