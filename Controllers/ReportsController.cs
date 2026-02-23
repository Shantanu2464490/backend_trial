using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend_trial.Data;
using backend_trial.Models.Domain;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IdeaBoardDbContext _dbContext;

        public ReportsController(IdeaBoardDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        [HttpGet("system-overview")]
        public async Task<ActionResult> GetSystemOverview()
        {
            try
            {
                var totalIdeas = await _dbContext.Ideas.CountAsync();
                var totalApprovedIdeas = await _dbContext.Ideas.CountAsync(i => i.Status == IdeaStatus.Approved);
                var totalRejectedIdeas = await _dbContext.Ideas.CountAsync(i => i.Status == IdeaStatus.Rejected);
                var totalUnderReviewIdeas = await _dbContext.Ideas.CountAsync(i => i.Status == IdeaStatus.UnderReview);

                var totalUsers = await _dbContext.Users.CountAsync();
                var totalManagers = await _dbContext.Users.CountAsync(u => u.Role == UserRole.Manager);
                var totalEmployees = await _dbContext.Users.CountAsync(u => u.Role == UserRole.Employee);
                var totalAdmins = await _dbContext.Users.CountAsync(u => u.Role == UserRole.Admin);

                var totalCategories = await _dbContext.Categories.CountAsync();
                var activeCategories = await _dbContext.Categories.CountAsync(c => c.IsActive);

                var approvalRate = totalIdeas > 0 ? Math.Round((decimal)totalApprovedIdeas / totalIdeas * 100, 2) : 0;

                var ideaStatusDistribution = new List<object>
                {
                    new
                    {
                        status = "Approved",
                        count = totalApprovedIdeas,
                        percentage = totalIdeas > 0 ? Math.Round((decimal)totalApprovedIdeas / totalIdeas * 100, 2) : 0
                    },
                    new
                    {
                        status = "Rejected",
                        count = totalRejectedIdeas,
                        percentage = totalIdeas > 0 ? Math.Round((decimal)totalRejectedIdeas / totalIdeas * 100, 2) : 0
                    },
                    new
                    {
                        status = "UnderReview",
                        count = totalUnderReviewIdeas,
                        percentage = totalIdeas > 0 ? Math.Round((decimal)totalUnderReviewIdeas / totalIdeas * 100, 2) : 0
                    }
                };

                var categoryReports = await _dbContext.Categories
                    .Select(c => new
                    {
                        categoryId = c.CategoryId,
                        categoryName = c.Name,
                        ideasSubmitted = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId),
                        approvedIdeas = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId && i.Status == IdeaStatus.Approved),
                        rejectedIdeas = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId && i.Status == IdeaStatus.Rejected),
                        underReviewIdeas = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId && i.Status == IdeaStatus.UnderReview)
                    })
                    .ToListAsync();

                // Calculate approval rate for each category
                var categoryReportsWithApprovalRate = categoryReports.Select(cr => new
                {
                    cr.categoryId,
                    cr.categoryName,
                    cr.ideasSubmitted,
                    cr.approvedIdeas,
                    cr.rejectedIdeas,
                    cr.underReviewIdeas,
                    approvalRate = cr.ideasSubmitted > 0 ? Math.Round((decimal)cr.approvedIdeas / cr.ideasSubmitted * 100, 2) : 0
                }).ToList();

                return Ok(new
                {
                    totalIdeas,
                    totalApprovedIdeas,
                    totalRejectedIdeas,
                    totalUnderReviewIdeas,
                    totalUsers,
                    totalManagers,
                    totalEmployees,
                    totalAdmins,
                    totalCategories,
                    activeCategories,
                    approvalRate,
                    ideaStatusDistribution,
                    categoryReports = categoryReportsWithApprovalRate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving system overview", error = ex.Message });
            }
        }

        [HttpGet("ideas/status-distribution")]
        public async Task<ActionResult> GetIdeasStatusDistribution()
        {
            try
            {
                var totalIdeas = await _dbContext.Ideas.CountAsync();
                var statusDistribution = new List<object>();

                var approved = await _dbContext.Ideas.CountAsync(i => i.Status == IdeaStatus.Approved);
                var rejected = await _dbContext.Ideas.CountAsync(i => i.Status == IdeaStatus.Rejected);
                var underReview = await _dbContext.Ideas.CountAsync(i => i.Status == IdeaStatus.UnderReview);

                statusDistribution.Add(new
                {
                    status = "Approved",
                    count = approved,
                    percentage = totalIdeas > 0 ? Math.Round((decimal)approved / totalIdeas * 100, 2) : 0
                });

                statusDistribution.Add(new
                {
                    status = "Rejected",
                    count = rejected,
                    percentage = totalIdeas > 0 ? Math.Round((decimal)rejected / totalIdeas * 100, 2) : 0
                });

                statusDistribution.Add(new
                {
                    status = "UnderReview",
                    count = underReview,
                    percentage = totalIdeas > 0 ? Math.Round((decimal)underReview / totalIdeas * 100, 2) : 0
                });

                return Ok(statusDistribution);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving status distribution", error = ex.Message });
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult> GetCategoryReports()
        {
            try
            {
                var categoryReports = await _dbContext.Categories
                    .Select(c => new
                    {
                        categoryId = c.CategoryId,
                        categoryName = c.Name,
                        ideasSubmitted = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId),
                        approvedIdeas = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId && i.Status == IdeaStatus.Approved),
                        rejectedIdeas = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId && i.Status == IdeaStatus.Rejected),
                        underReviewIdeas = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId && i.Status == IdeaStatus.UnderReview)
                    })
                    .OrderByDescending(c => c.ideasSubmitted)
                    .ToListAsync();

                return Ok(categoryReports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving category reports", error = ex.Message });
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult> GetCategoryReport(Guid categoryId)
        {
            try
            {
                var category = await _dbContext.Categories.FindAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }

                var ideasSubmitted = await _dbContext.Ideas.CountAsync(i => i.CategoryId == categoryId);
                var approvedIdeas = await _dbContext.Ideas.CountAsync(i => i.CategoryId == categoryId && i.Status == IdeaStatus.Approved);
                var rejectedIdeas = await _dbContext.Ideas.CountAsync(i => i.CategoryId == categoryId && i.Status == IdeaStatus.Rejected);
                var underReviewIdeas = await _dbContext.Ideas.CountAsync(i => i.CategoryId == categoryId && i.Status == IdeaStatus.UnderReview);

                return Ok(new
                {
                    categoryId = category.CategoryId,
                    categoryName = category.Name,
                    ideasSubmitted,
                    approvedIdeas,
                    rejectedIdeas,
                    underReviewIdeas,
                    lastUpdated = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving category report", error = ex.Message });
            }
        }

        [HttpGet("ideas/by-date")]
        public async Task<ActionResult> GetIdeasByDateRange([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                var ideasByDate = await _dbContext.Ideas
                    .Where(i => i.SubmittedDate >= start && i.SubmittedDate <= end)
                    .GroupBy(i => i.SubmittedDate.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        ideasSubmitted = g.Count(),
                        ideasApproved = g.Count(i => i.Status == IdeaStatus.Approved),
                        ideasRejected = g.Count(i => i.Status == IdeaStatus.Rejected),
                        ideasUnderReview = g.Count(i => i.Status == IdeaStatus.UnderReview)
                    })
                    .OrderBy(x => x.date)
                    .ToListAsync();

                return Ok(ideasByDate);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving ideas by date", error = ex.Message });
            }
        }

        [HttpGet("users/activity")]
        public async Task<ActionResult> GetUserActivityReport()
        {
            try
            {
                var totalUsers = await _dbContext.Users.CountAsync();
                var activeUsers = await _dbContext.Users.CountAsync(u => u.Status == UserStatus.Active);
                var inactiveUsers = totalUsers - activeUsers;

                var usersWithIdeas = await _dbContext.Users
                    .Where(u => u.SubmittedIdeas.Any())
                    .CountAsync();

                var usersWithReviews = await _dbContext.Users
                    .Where(u => u.ReviewsAuthored.Any())
                    .CountAsync();

                var usersWithComments = await _dbContext.Users
                    .Where(u => u.Comments.Any())
                    .CountAsync();

                var usersWithVotes = await _dbContext.Users
                    .Where(u => u.Votes.Any())
                    .CountAsync();

                var totalIdeas = await _dbContext.Ideas.CountAsync();
                var averageIdeasPerUser = usersWithIdeas > 0 ? Math.Round((decimal)totalIdeas / usersWithIdeas, 2) : 0;

                var totalComments = await _dbContext.Comments.CountAsync();
                var averageCommentsPerUser = usersWithComments > 0 ? Math.Round((decimal)totalComments / usersWithComments, 2) : 0;

                var engagementRate = totalUsers > 0 ? Math.Round((decimal)(usersWithIdeas + usersWithComments + usersWithVotes) / (totalUsers * 3) * 100, 2) : 0;

                return Ok(new
                {
                    totalUsers,
                    activeUsers,
                    inactiveUsers,
                    usersWithIdeas,
                    usersWithReviews,
                    usersWithComments,
                    usersWithVotes,
                    averageIdeasPerUser,
                    averageCommentsPerUser,
                    engagementRate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving user activity report", error = ex.Message });
            }
        }

        [HttpGet("top-categories")]
        public async Task<ActionResult> GetTopCategories([FromQuery] int limit = 10)
        {
            try
            {
                var topCategories = await _dbContext.Categories
                    .Select(c => new
                    {
                        categoryId = c.CategoryId,
                        categoryName = c.Name,
                        ideasSubmitted = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId),
                        approvedIdeas = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId && i.Status == IdeaStatus.Approved),
                        rejectedIdeas = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId && i.Status == IdeaStatus.Rejected),
                        underReviewIdeas = _dbContext.Ideas.Count(i => i.CategoryId == c.CategoryId && i.Status == IdeaStatus.UnderReview)
                    })
                    .OrderByDescending(c => c.ideasSubmitted)
                    .Take(limit)
                    .ToListAsync();

                return Ok(topCategories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving top categories", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/reports/approval-trends
        /// Get approval rate trends over time.
        /// </summary>
        [HttpGet("approval-trends")]
        public async Task<ActionResult> GetApprovalTrends([FromQuery] int months = 6)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                var approvalTrends = await _dbContext.Ideas
                    .Where(i => i.SubmittedDate >= startDate)
                    .GroupBy(i => new { Year = i.SubmittedDate.Year, Month = i.SubmittedDate.Month })
                    .Select(g => new
                    {
                        month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        ideasSubmitted = g.Count(),
                        ideasApproved = g.Count(i => i.Status == IdeaStatus.Approved),
                        approvalRate = g.Count() > 0 ? Math.Round((decimal)g.Count(i => i.Status == IdeaStatus.Approved) / g.Count() * 100, 2) : 0
                    })
                    .OrderBy(x => x.month)
                    .ToListAsync();

                return Ok(approvalTrends);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving approval trends", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/reports/employee-contributions
        /// Get statistics on employee contributions and activity.
        /// </summary>
        [HttpGet("employee-contributions")]
        public async Task<ActionResult> GetEmployeeContributions()
        {
            try
            {
                var employeeContributions = await _dbContext.Users
                    .Where(u => u.Status == UserStatus.Active)
                    .Select(u => new
                    {
                        userId = u.UserId,
                        userName = u.Name,
                        department = "N/A", // Department not in model, placeholder
                        ideasSubmitted = u.SubmittedIdeas.Count(),
                        ideasApproved = u.SubmittedIdeas.Count(i => i.Status == IdeaStatus.Approved),
                        commentsPosted = u.Comments.Count(),
                        votesGiven = u.Votes.Count(),
                        totalEngagementScore = u.SubmittedIdeas.Count() * 10 + u.Comments.Count() * 5 + u.Votes.Count()
                    })
                    .OrderByDescending(e => e.totalEngagementScore)
                    .ToListAsync();

                return Ok(employeeContributions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving employee contributions", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/reports/export/excel
        /// Export all reports data to Excel format.
        /// </summary>
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportReportsToExcel()
        {
            try
            {
                // For now, return a simple message indicating Excel export would be generated
                // In production, you would use a library like EPPlus or ClosedXML to generate actual Excel files

                var systemOverviewData = await GetSystemOverviewData();
                var fileName = $"reports-{DateTime.UtcNow:yyyy-MM-dd}.xlsx";

                // Placeholder response - in production, generate actual Excel file
                return Ok(new
                {
                    message = "Excel export functionality - implement with EPPlus or ClosedXML",
                    fileName,
                    note = "This endpoint would return a binary Excel file with all report data"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating Excel file", error = ex.Message });
            }
        }

        private async Task<object> GetSystemOverviewData()
        {
            var totalIdeas = await _dbContext.Ideas.CountAsync();
            var totalApprovedIdeas = await _dbContext.Ideas.CountAsync(i => i.Status == IdeaStatus.Approved);
            var totalRejectedIdeas = await _dbContext.Ideas.CountAsync(i => i.Status == IdeaStatus.Rejected);
            var totalUnderReviewIdeas = await _dbContext.Ideas.CountAsync(i => i.Status == IdeaStatus.UnderReview);

            var totalUsers = await _dbContext.Users.CountAsync();
            var totalManagers = await _dbContext.Users.CountAsync(u => u.Role == UserRole.Manager);
            var totalEmployees = await _dbContext.Users.CountAsync(u => u.Role == UserRole.Employee);
            var totalAdmins = await _dbContext.Users.CountAsync(u => u.Role == UserRole.Admin);

            var totalCategories = await _dbContext.Categories.CountAsync();
            var activeCategories = await _dbContext.Categories.CountAsync(c => c.IsActive);

            var approvalRate = totalIdeas > 0 ? Math.Round((decimal)totalApprovedIdeas / totalIdeas * 100, 2) : 0;

            return new
            {
                totalIdeas,
                totalApprovedIdeas,
                totalRejectedIdeas,
                totalUnderReviewIdeas,
                totalUsers,
                totalManagers,
                totalEmployees,
                totalAdmins,
                totalCategories,
                activeCategories,
                approvalRate
            };
        }

        [HttpGet("ideas/latest")]
        public async Task<ActionResult> GetLatestIdeas()
        {
            try
            {
                var latestIdeas = await _dbContext.Ideas
                    .OrderByDescending(i => i.SubmittedDate)
                    .Take(5)
                    .Select(i => new
                    {
                        ideaId = i.IdeaId,
                        title = i.Title,
                        description = i.Description,
                        submittedBy = i.SubmittedByUser.Name,
                        categoryName = i.Category.Name,
                        status = i.Status.ToString(),
                        submittedDate = i.SubmittedDate
                    })
                    .ToListAsync();

                return Ok(latestIdeas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error retrieving latest ideas", error = ex.Message });
            }
        }
    }
}
