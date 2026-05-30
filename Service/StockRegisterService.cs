using lib.DtoModel.AddStockBookDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class StockRegisterService:BaseService,IStockRegisterService
    {
        private readonly string _connectionString;

        public StockRegisterService(IConfiguration configuration,IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection");
        }

        private object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using SqlConnection con = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddRange(parameters);

            con.Open();

            return cmd.ExecuteScalar();
        }

        private async Task ExecuteNonQueryAsync(
            string query,
            List<SqlParameter> parameters)
        {
            using SqlConnection con = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddRange(parameters.ToArray());

            await con.OpenAsync();

            await cmd.ExecuteNonQueryAsync();
        }

        private List<string> ExecuteList(
            string query,
            params SqlParameter[] parameters)
        {
            List<string> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddRange(parameters);

            con.Open();

            using var dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                list.Add(dr[0].ToString() ?? "");
            }

            return list;
        }

        private Dictionary<string, object> ExecuteSingleRow(
            string query,
            params SqlParameter[] parameters)
        {
            var result = new Dictionary<string, object>();

            using SqlConnection con = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddRange(parameters);

            con.Open();

            using var dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    result[dr.GetName(i)] =
                        dr[i] == DBNull.Value ? null! : dr[i];
                }
            }

            return result;
        }

        public object GetInitialData(string collegeName)
        {
            var collegeData = ExecuteSingleRow(
                @"SELECT Id, CollegeCode
                  FROM MasterCollege
                  WHERE CollegeName=@CollegeName",

                new SqlParameter("@CollegeName", collegeName)
            );

            if (collegeData.Count == 0)
            {
                throw new Exception("College not found");
            }

            long collegeId =
                Convert.ToInt64(collegeData["Id"]);

            string collegeCode =
                Convert.ToString(collegeData["CollegeCode"]) ?? "";
            long maxId = Convert.ToInt64(
                ExecuteScalar(
                    @"SELECT ISNULL(MAX(AccessionId),100)
          FROM StockRegister
          WHERE CollegeName=@CollegeName
          AND AccessionId > 0",

                    new SqlParameter("@CollegeName", collegeName)
                )
            );

            long nextId = maxId + 1;

            string accessionNo = $"{collegeCode}-{nextId}";

            return new
            {
                CollegeId = collegeId,
                AccessionId = nextId,
                AccessionNo = accessionNo,

                Publishers = ExecuteList(
                    "SELECT DISTINCT Publisher FROM Publishers WHERE CollegeName=@CollegeName",
                    new SqlParameter("@CollegeName", collegeName)
                ),

                Sources = ExecuteList(
                    "SELECT DISTINCT Source FROM SourceBooks WHERE CollegeName=@CollegeName",
                    new SqlParameter("@CollegeName", collegeName)
                ),

                Categories = ExecuteList(
                    "SELECT DISTINCT Category FROM Categories WHERE CollegeName=@CollegeName",
                    new SqlParameter("@CollegeName", collegeName)
                ),

                Titles = ExecuteList(
                    "SELECT DISTINCT Title FROM StockRegister WHERE CollegeName=@CollegeName",
                    new SqlParameter("@CollegeName", collegeName)
                )
            };
        }

        public Dictionary<string, object> GetByAccession(
            string collegeName,
            string accessionNo)
        {
            return ExecuteSingleRow(
                @"SELECT *
                  FROM StockRegister
                  WHERE CollegeName=@CollegeName
                  AND AccessionNo=@AccessionNo",

                new SqlParameter("@CollegeName", collegeName),

                new SqlParameter("@AccessionNo", accessionNo)
            );
        }

        public Dictionary<string, object> GetBookDetail(
            string collegeName,
            string title)
        {
            return ExecuteSingleRow(
                @"SELECT TOP 1
                    Author,
                    Publisher,
                    Source,
                    Edition,
                    Price,
                    Category
                  FROM StockRegister
                  WHERE CollegeName=@CollegeName
                  AND Title=@Title",

                new SqlParameter("@CollegeName", collegeName),

                new SqlParameter("@Title", title)
            );
        }

        public List<string> AutoComplete(
            string collegeName,
            string field,
            string search)
        {
            string column = field switch
            {
                "first" => "FirstAuthorForeName",
                "second" => "SecondAuthorForeName",
                "third" => "ThirdAuthorForeName",
                "surname1" => "FirstAuthorSirName",
                "surname2" => "SecondAuthorSirName",
                "surname3" => "ThirdAuthorSirName",
                "place" => "Place",
                _ => "Title"
            };

            return ExecuteList(
                $@"SELECT DISTINCT {column}
                   FROM StockRegister
                   WHERE CollegeName=@CollegeName
                   AND {column} LIKE @Search",

                new SqlParameter("@CollegeName", collegeName),

                new SqlParameter("@Search", "%" + search + "%")
            );
        }

        private List<SqlParameter> GetBookParameters(RequestDto req)
        {
            return new List<SqlParameter>
            {
                new("@CollegeName", req.CollegeName ?? ""),
                new("@DateEntry", req.DateEntry ?? DateTime.Now),
                new("@AccessionNo", req.AccessionNo ?? ""),
                new("@Author", req.Author ?? "None"),
                new("@Title", req.Title ?? (object)DBNull.Value),
                new("@Edition", req.Edition ?? (object)DBNull.Value),
                new("@Publisher", req.Publisher ?? (object)DBNull.Value),
                new("@Source", req.Source ?? (object)DBNull.Value),
                new("@Year", req.Year ?? (object)DBNull.Value),
                new("@Pages", req.Pages ?? (object)DBNull.Value),
                new("@Volume", req.Volume ?? (object)DBNull.Value),
                new("@Price", req.Price ?? 0),
                new("@Discount", req.Discount ?? (object)DBNull.Value),
                new("@NetPrice", req.NetPrice ?? 0),
                new("@Type", req.Type ?? (object)DBNull.Value), 
                new("@Category", req.Category ?? "None"),
                new("@BillNo", req.BillNo ?? (object)DBNull.Value),
                new("@BillDate", req.BillDate ?? (object)DBNull.Value),
                new("@ClassNo", req.ClassNo ?? (object)DBNull.Value),
                new("@BookNo", req.BookNo ?? (object)DBNull.Value),
                new("@Remarks", req.Remarks ?? (object)DBNull.Value),
                new("@Location", req.Location ?? (object)DBNull.Value),
                new("@FirstAuthorForeName", req.FirstAuthorForename ?? ""),
                new("@FirstAuthorSirName", req.FirstAuthorSirName ?? ""),
                new("@SecondAuthorForeName", req.SecondAuthorForename ?? ""),
                new("@SecondAuthorSirName", req.SecondAuthorSurname ?? ""),
                new("@ThirdAuthorForeName", req.ThirdAuthorForename ?? ""),
                new("@ThirdAuthorSirName", req.ThirdAuthorSurname ?? ""),
                new("@MoreThanThreeAuthors", req.MoreThanThreeAuthors ?? "False"),
                new("@SubTitle", req.Subtitle ?? (object)DBNull.Value),
                new("@ISBN", req.ISBN ?? (object)DBNull.Value),
                new("@Place", req.Place ?? (object)DBNull.Value),
                new("@Series", req.Series ?? (object)DBNull.Value),
                new("@BookSize", req.Size ?? (object)DBNull.Value),
                new("@Subject1", req.Subject1 ?? (object)DBNull.Value),
                new("@Subject2", req.Subject2 ?? (object)DBNull.Value),
                new("@BindingBook", "Normal"),
                new("@Attachment", DBNull.Value),
                new("@CollegeId", req.CollegeId ?? 0),
              new("@AccessionId", req.AccessionId ?? 0),
            };
        }

        public async Task<string> AddBookAsync(RequestDto req)
        {
            var exists = ExecuteScalar(
                @"SELECT COUNT(1)
                  FROM StockRegister
                  WHERE CollegeName=@CollegeName
                  AND AccessionNo=@AccessionNo",

                new SqlParameter("@CollegeName", req.CollegeName),

                new SqlParameter("@AccessionNo", req.AccessionNo)
            );

            if (Convert.ToInt32(exists) > 0)
            {
                return "Accession No already exists";
            }

            string query = @"INSERT INTO StockRegister
            (
                CollegeId, AccessionId,
                CollegeName, DateEntry, AccessionNo, Author,
                Title, Edition, Publisher, Year, Pages,
                Source, Price, Discount, NetPrice,
                Type, Category, BillNo, BillDate,
                ClassNo, BookNo, Remarks, Location,
                FirstAuthorForeName, FirstAuthorSirName,
                SecondAuthorForeName, SecondAuthorSirName,
                ThirdAuthorForeName, ThirdAuthorSirName,
                MoreThanThreeAuthors, SubTitle, ISBN,
                Place, Series, BookSize, Subject1,
                Subject2, BindingBook, Attachment, Volume
            )
            VALUES
            (
               @CollegeId, @AccessionId,
                @CollegeName, @DateEntry, @AccessionNo, @Author,
                @Title, @Edition, @Publisher, @Year, @Pages,
                @Source, @Price, @Discount, @NetPrice,
                @Type, @Category, @BillNo, @BillDate,
                @ClassNo, @BookNo, @Remarks, @Location,
                @FirstAuthorForeName, @FirstAuthorSirName,
                @SecondAuthorForeName, @SecondAuthorSirName,
                @ThirdAuthorForeName, @ThirdAuthorSirName,
                @MoreThanThreeAuthors, @SubTitle, @ISBN,
                @Place, @Series, @BookSize, @Subject1,
                @Subject2, @BindingBook, @Attachment, @Volume
            )";

            await ExecuteNonQueryAsync(query, GetBookParameters(req));

            return "Added Successfully";
        }

        public async Task<string> UpdateBookAsync(RequestDto req)
        {
            string query = @"UPDATE StockRegister SET
                Author=@Author,
                Title=@Title,
                Edition=@Edition,
                Publisher=@Publisher,
                Year=@Year,
                Pages=@Pages,
                Source=@Source,
                Price=@Price,
                Discount=@Discount,
                NetPrice=@NetPrice,
                Type=@Type,
                Category=@Category,
                BillNo=@BillNo,
                BillDate=@BillDate,
                ClassNo=@ClassNo,
                BookNo=@BookNo,
                Remarks=@Remarks,
                Location=@Location,
                FirstAuthorForeName=@FirstAuthorForeName,
                FirstAuthorSirName=@FirstAuthorSirName,
                SecondAuthorForeName=@SecondAuthorForeName,
                SecondAuthorSirName=@SecondAuthorSirName,
                ThirdAuthorForeName=@ThirdAuthorForeName,
                ThirdAuthorSirName=@ThirdAuthorSirName,
                MoreThanThreeAuthors=@MoreThanThreeAuthors,
                SubTitle=@SubTitle,
                ISBN=@ISBN,
                Place=@Place,
                Series=@Series,
                BookSize=@BookSize,
                Subject1=@Subject1,
                Subject2=@Subject2
            WHERE CollegeName=@CollegeName
            AND AccessionNo=@AccessionNo";

            await ExecuteNonQueryAsync(query, GetBookParameters(req));

            return "Updated Successfully";
        }
    }
}
