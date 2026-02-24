// Services/Interfaces/IReportsService.cs
using backend_trial.Models.DTO.Reports;

namespace backend_trial.Services.Interfaces
{
    public interface IReportsService
    {
        Task<SystemOverviewDto> GetSystemOverviewAsync(CancellationToken ct = default);
        Task<IEnumerable<IdeaStatusDistributionItemDto>> GetIdeasStatusDistributionAsync(CancellationToken ct = default);
        Task<IEnumerable<CategoryReportDto>> GetCategoryReportsAsync(CancellationToken ct = default);
        Task<CategoryReportDto> GetCategoryReportAsync(Guid categoryId, CancellationToken ct = default);
        Task<IEnumerable<IdeasByDateDto>> GetIdeasByDateRangeAsync(DateTime? startDate, DateTime? endDate, CancellationToken ct = default);
        Task<UserActivityReportDto> GetUserActivityReportAsync(CancellationToken ct = default);
        Task<IEnumerable<CategoryReportDto>> GetTopCategoriesAsync(int limit, CancellationToken ct = default);
        Task<IEnumerable<ApprovalTrendItemDto>> GetApprovalTrendsAsync(int months, CancellationToken ct = default);
        Task<IEnumerable<LatestIdeaDto>> GetLatestIdeasAsync(int limit, CancellationToken ct = default);

        // Placeholder: Excel export
        Task<(string FileName, string Note)> ExportReportsToExcelAsync(CancellationToken ct = default);
        Task<object?> GetEmployeeContributionsAsync(CancellationToken ct);
    }
}
