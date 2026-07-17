using lib.DtoModel.DashboardDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lib.Interface
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(List<string> authorizedColleges);
        Task<List<QuickSearchResultDto>> QuickSearchAsync(List<string> authorizedColleges, string query);
    }
}
