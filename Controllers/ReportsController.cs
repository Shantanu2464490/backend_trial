using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_trial.Services.Interfaces;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsService reportsService;

        public ReportsController(IReportsService reportsService)
        {
            this.reportsService = reportsService;
        }

        [HttpGet("system-overview")]
        public async Task<IActionResult> GetSystemOverview(CancellationToken ct)
        {
            return Ok(await reportsService.GetSystemOverviewAsync(ct));
        }

        [HttpGet("ideas/status-distribution")]
        public async Task<IActionResult> GetIdeasStatusDistribution(CancellationToken ct)
        {
            return Ok(await reportsService.GetIdeasStatusDistributionAsync(ct));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoryReports(CancellationToken ct)
        {
            return Ok(await reportsService.GetCategoryReportsAsync(ct));
        }

        [HttpGet("category/{categoryId:guid}")]
        public async Task<IActionResult> GetCategoryReport(Guid categoryId, CancellationToken ct)
        {
            return Ok(await reportsService.GetCategoryReportAsync(categoryId, ct));
        }

        [HttpGet("ideas/by-date")]
        public async Task<IActionResult> GetIdeasByDateRange(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken ct)
        {
            return Ok(await reportsService.GetIdeasByDateRangeAsync(startDate, endDate, ct));
        }

        [HttpGet("users/activity")]
        public async Task<IActionResult> GetUserActivity(CancellationToken ct)
        {
            return Ok(await reportsService.GetUserActivityReportAsync(ct));
        }

        [HttpGet("top-categories")]
        public async Task<IActionResult> GetTopCategories([FromQuery] int limit = 10, CancellationToken ct = default)
        {
            return Ok(await reportsService.GetTopCategoriesAsync(limit, ct));
        }

        [HttpGet("approval-trends")]
        public async Task<IActionResult> GetApprovalTrends([FromQuery] int months = 6, CancellationToken ct = default)
        {
            return Ok(await reportsService.GetApprovalTrendsAsync(months, ct));
        }

        [HttpGet("employee-contributions")]
        public async Task<IActionResult> GetEmployeeContributions(CancellationToken ct)
        {
            // Optional endpoint if needed in service
            return Ok(await reportsService.GetEmployeeContributionsAsync(ct));
        }

        [HttpGet("ideas/latest")]
        public async Task<IActionResult> GetLatestIdeas([FromQuery] int limit = 5, CancellationToken ct = default)
        {
            return Ok(await reportsService.GetLatestIdeasAsync(limit, ct));
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportReportsToExcel(CancellationToken ct)
        {
            var (fileName, note) = await reportsService.ExportReportsToExcelAsync(ct);
            return Ok(new { fileName, note });
        }
    }
}