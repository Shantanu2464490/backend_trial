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
            try
            {
                var result = await reportsService.GetSystemOverviewAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("ideas/status-distribution")]
        public async Task<IActionResult> GetIdeasStatusDistribution(CancellationToken ct)
        {
            try
            {
                var result = await reportsService.GetIdeasStatusDistributionAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoryReports(CancellationToken ct)
        {
            try
            {
                var result = await reportsService.GetCategoryReportsAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("category/{categoryId:guid}")]
        public async Task<IActionResult> GetCategoryReport(Guid categoryId, CancellationToken ct)
        {
            try
            {
                var result = await reportsService.GetCategoryReportAsync(categoryId, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("users/activity")]
        public async Task<IActionResult> GetUserActivity(CancellationToken ct)
        {
            try
            {
                var result = await reportsService.GetUserActivityReportAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("top-categories")]
        public async Task<IActionResult> GetTopCategories([FromQuery] int limit = 10, CancellationToken ct = default)
        {
            try
            {
                var result = await reportsService.GetTopCategoriesAsync(limit, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("approval-trends")]
        public async Task<IActionResult> GetApprovalTrends([FromQuery] int months = 6, CancellationToken ct = default)
        {
            try
            {
                var result = await reportsService.GetApprovalTrendsAsync(months, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("employee-contributions")]
        public async Task<IActionResult> GetEmployeeContributions(CancellationToken ct)
        {
            try
            {
                var result = await reportsService.GetEmployeeContributionsAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("ideas/latest")]
        public async Task<IActionResult> GetLatestIdeas([FromQuery] int limit = 5, CancellationToken ct = default)
        {
            try
            {
                var result = await reportsService.GetLatestIdeasAsync(limit, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportReportsToExcel(CancellationToken ct)
        {
            try
            {
                var (fileName, note) = await reportsService.ExportReportsToExcelAsync(ct);
                return Ok(new { fileName, note });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }
    }
}