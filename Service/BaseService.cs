using System.Text.Json;

namespace lib.Service
{
    public class BaseService
    {
        protected readonly IHttpContextAccessor
            _httpContextAccessor;

        public BaseService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor =
                httpContextAccessor;
        }

        // =====================================
        // USERNAME
        // =====================================

        protected string UserName =>
            _httpContextAccessor
            .HttpContext?
            .Session?
            .GetString("UserName") ?? "";

        // =====================================
        // LOGIN TYPE
        // =====================================

        protected string LoginType =>
            _httpContextAccessor
            .HttpContext?
            .Session?
            .GetString("LoginType") ?? "";

        // =====================================
        // ALL COLLEGES
        // =====================================

        protected List<string> Colleges
        {
            get
            {
                var data =
                    _httpContextAccessor
                    .HttpContext?
                    .Session?
                    .GetString("Colleges");

                if (string.IsNullOrEmpty(data))
                    return new List<string>();

                return JsonSerializer
                    .Deserialize<List<string>>(data)
                    ?? new List<string>();
            }
        }

        // =====================================
        // COLLEGE FILTER
        // =====================================

        protected string GetCollegeFilter()
        {
            if (Colleges == null || Colleges.Count == 0)
                return "''";

            return string.Join(",",
                Colleges.Select(x =>
                    $"'{x.Replace("'", "''")}'"));
        }
    }
}