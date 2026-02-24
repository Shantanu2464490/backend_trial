// Services/ReportsService.cs
using backend_trial.Repositories.Interfaces;
using backend_trial.Services.Interfaces;

using backend_trial.Models.DTO.Reports;

namespace backend_trial.Services
{
    public class ReportsService : IReportsService
    {
        private readonly IReportsRepository reportRepository;

        public ReportsService(IReportsRepository reportRepository)
        {
            this.reportRepository = reportRepository;
        }

        public async Task<SystemOverviewDto> GetSystemOverviewAsync(CancellationToken ct = default)
        {
            var ideaCounts = await reportRepository.GetIdeaCountsAsync(ct);
            var userCounts = await reportRepository.GetUserCountsAsync(ct);
            var categoryCounts = await reportRepository.GetCategoryCountsAsync(ct);
            var categoryAggs = await reportRepository.GetCategoryAggregatesAsync(ct);

            decimal approvalRate = ideaCounts.Total > 0
                ? Math.Round((decimal)ideaCounts.Approved / ideaCounts.Total * 100, 2)
                : 0;

            var statusDist = new List<IdeaStatusDistributionItemDto>
            {
                new() { Status = "Approved", Count = ideaCounts.Approved, Percentage = ideaCounts.Total > 0 ? Math.Round((decimal)ideaCounts.Approved / ideaCounts.Total * 100, 2) : 0 },
                new() { Status = "Rejected", Count = ideaCounts.Rejected, Percentage = ideaCounts.Total > 0 ? Math.Round((decimal)ideaCounts.Rejected / ideaCounts.Total * 100, 2) : 0 },
                new() { Status = "UnderReview", Count = ideaCounts.UnderReview, Percentage = ideaCounts.Total > 0 ? Math.Round((decimal)ideaCounts.UnderReview / ideaCounts.Total * 100, 2) : 0 },
            };

            var categoryReports = categoryAggs.Select(a => new CategoryReportDto
            {
                CategoryId = a.CategoryId,
                CategoryName = a.CategoryName,
                IdeasSubmitted = a.IdeasSubmitted,
                ApprovedIdeas = a.ApprovedIdeas,
                RejectedIdeas = a.RejectedIdeas,
                UnderReviewIdeas = a.UnderReviewIdeas,
                ApprovalRate = a.IdeasSubmitted > 0 ? Math.Round((decimal)a.ApprovedIdeas / a.IdeasSubmitted * 100, 2) : 0
            });

            return new SystemOverviewDto
            {
                TotalIdeas = ideaCounts.Total,
                TotalApprovedIdeas = ideaCounts.Approved,
                TotalRejectedIdeas = ideaCounts.Rejected,
                TotalUnderReviewIdeas = ideaCounts.UnderReview,
                TotalUsers = userCounts.TotalUsers,
                TotalManagers = userCounts.Managers,
                TotalEmployees = userCounts.Employees,
                TotalAdmins = userCounts.Admins,
                TotalCategories = categoryCounts.TotalCategories,
                ActiveCategories = categoryCounts.ActiveCategories,
                ApprovalRate = approvalRate,
                IdeaStatusDistribution = statusDist,
                CategoryReports = categoryReports
            };
        }

        public async Task<IEnumerable<IdeaStatusDistributionItemDto>> GetIdeasStatusDistributionAsync(CancellationToken ct = default)
        {
            var ideaCounts = await reportRepository.GetIdeaCountsAsync(ct);
            int total = ideaCounts.Total;

            return new[]
            {
                new IdeaStatusDistributionItemDto { Status = "Approved", Count = ideaCounts.Approved, Percentage = total > 0 ? Math.Round((decimal)ideaCounts.Approved / total * 100, 2) : 0 },
                new IdeaStatusDistributionItemDto { Status = "Rejected", Count = ideaCounts.Rejected, Percentage = total > 0 ? Math.Round((decimal)ideaCounts.Rejected / total * 100, 2) : 0 },
                new IdeaStatusDistributionItemDto { Status = "UnderReview", Count = ideaCounts.UnderReview, Percentage = total > 0 ? Math.Round((decimal)ideaCounts.UnderReview / total * 100, 2) : 0 },
            };
        }

        public async Task<IEnumerable<CategoryReportDto>> GetCategoryReportsAsync(CancellationToken ct = default)
        {
            var aggs = await reportRepository.GetCategoryAggregatesAsync(ct);
            return aggs.Select(a => new CategoryReportDto
            {
                CategoryId = a.CategoryId,
                CategoryName = a.CategoryName,
                IdeasSubmitted = a.IdeasSubmitted,
                ApprovedIdeas = a.ApprovedIdeas,
                RejectedIdeas = a.RejectedIdeas,
                UnderReviewIdeas = a.UnderReviewIdeas,
                ApprovalRate = a.IdeasSubmitted > 0 ? Math.Round((decimal)a.ApprovedIdeas / a.IdeasSubmitted * 100, 2) : 0
            });
        }

        public async Task<CategoryReportDto> GetCategoryReportAsync(Guid categoryId, CancellationToken ct = default)
        {
            var agg = await reportRepository.GetSingleCategoryAggregateAsync(categoryId, ct);
            if (agg is null)
                throw new("Category not found");

            return new CategoryReportDto
            {
                CategoryId = agg.CategoryId,
                CategoryName = agg.CategoryName,
                IdeasSubmitted = agg.IdeasSubmitted,
                ApprovedIdeas = agg.ApprovedIdeas,
                RejectedIdeas = agg.RejectedIdeas,
                UnderReviewIdeas = agg.UnderReviewIdeas,
                ApprovalRate = agg.IdeasSubmitted > 0 ? Math.Round((decimal)agg.ApprovedIdeas / agg.IdeasSubmitted * 100, 2) : 0
            };
        }

        public async Task<IEnumerable<IdeasByDateDto>> GetIdeasByDateRangeAsync(DateTime? startDate, DateTime? endDate, CancellationToken ct = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var buckets = await reportRepository.GetIdeasByDateRangeAggregatesAsync(start, end, ct);
            return buckets.Select(b => new IdeasByDateDto
            {
                Date = b.Date.ToString("yyyy-MM-dd"),
                IdeasSubmitted = b.IdeasSubmitted,
                IdeasApproved = b.IdeasApproved,
                IdeasRejected = b.IdeasRejected,
                IdeasUnderReview = b.IdeasUnderReview
            });
        }

        public async Task<UserActivityReportDto> GetUserActivityReportAsync(CancellationToken ct = default)
        {
            var userCounts = await reportRepository.GetUserCountsAsync(ct);

            // For averages and engagement we need some totals:
            var ideaCounts = await reportRepository.GetIdeaCountsAsync(ct);
            var employeeContribs = await reportRepository.GetEmployeeContributionsAsync(ct);
            var totalIdeas = ideaCounts.Total;
            var usersWithIdeas = employeeContribs.Count(ec => ec.IdeasSubmitted > 0);
            var usersWithComments = employeeContribs.Count(ec => ec.CommentsPosted > 0);
            var usersWithVotes = employeeContribs.Count(ec => ec.VotesGiven > 0);
            var usersWithReviews = 0; // Not tracked in this repository; set if you add a Reviews aggregation

            var totalComments = employeeContribs.Sum(ec => ec.CommentsPosted);

            var avgIdeasPerUser = usersWithIdeas > 0 ? Math.Round((decimal)totalIdeas / usersWithIdeas, 2) : 0;
            var avgCommentsPerUser = usersWithComments > 0 ? Math.Round((decimal)totalComments / usersWithComments, 2) : 0;

            var engagementRate = userCounts.TotalUsers > 0
                ? Math.Round((decimal)(usersWithIdeas + usersWithComments + usersWithVotes) / (userCounts.TotalUsers * 3) * 100, 2)
                : 0;

            return new UserActivityReportDto
            {
                TotalUsers = userCounts.TotalUsers,
                ActiveUsers = userCounts.ActiveUsers,
                InactiveUsers = userCounts.TotalUsers - userCounts.ActiveUsers,
                UsersWithIdeas = usersWithIdeas,
                UsersWithReviews = usersWithReviews,
                UsersWithComments = usersWithComments,
                UsersWithVotes = usersWithVotes,
                AverageIdeasPerUser = avgIdeasPerUser,
                AverageCommentsPerUser = avgCommentsPerUser,
                EngagementRate = engagementRate
            };
        }

        public async Task<IEnumerable<CategoryReportDto>> GetTopCategoriesAsync(int limit, CancellationToken ct = default)
        {
            var aggs = await reportRepository.GetTopCategoryAggregatesAsync(limit, ct);
            return aggs.Select(a => new CategoryReportDto
            {
                CategoryId = a.CategoryId,
                CategoryName = a.CategoryName,
                IdeasSubmitted = a.IdeasSubmitted,
                ApprovedIdeas = a.ApprovedIdeas,
                RejectedIdeas = a.RejectedIdeas,
                UnderReviewIdeas = a.UnderReviewIdeas,
                ApprovalRate = a.IdeasSubmitted > 0 ? Math.Round((decimal)a.ApprovedIdeas / a.IdeasSubmitted * 100, 2) : 0
            });
        }

        public async Task<IEnumerable<ApprovalTrendItemDto>> GetApprovalTrendsAsync(int months, CancellationToken ct = default)
        {
            var start = DateTime.UtcNow.AddMonths(-months);
            var buckets = await reportRepository.GetApprovalTrendsAsync(start, ct);

            return buckets.Select(b => new ApprovalTrendItemDto
            {
                Month = $"{b.Year}-{b.Month:D2}",
                IdeasSubmitted = b.IdeasSubmitted,
                IdeasApproved = b.IdeasApproved,
                ApprovalRate = b.IdeasSubmitted > 0 ? Math.Round((decimal)b.IdeasApproved / b.IdeasSubmitted * 100, 2) : 0
            });
        }

        public async Task<IEnumerable<LatestIdeaDto>> GetLatestIdeasAsync(int limit, CancellationToken ct = default)
        {
            var latest = await reportRepository.GetLatestIdeasAsync(limit, ct);
            return latest.Select(i => new LatestIdeaDto
            {
                IdeaId = i.IdeaId,
                Title = i.Title,
                Description = i.Description,
                SubmittedBy = i.SubmittedBy,
                CategoryName = i.CategoryName,
                Status = i.Status.ToString(),
                SubmittedDate = i.SubmittedDate
            });
        }

        public Task<(string FileName, string Note)> ExportReportsToExcelAsync(CancellationToken ct = default)
        {
            var fileName = $"reports-{DateTime.UtcNow:yyyy-MM-dd}.xlsx";
            var note = "Excel export placeholder — implement with EPPlus or ClosedXML in production.";
            return Task.FromResult((fileName, note));
        }

        public async Task<IEnumerable<EmployeeContributionDto>> GetEmployeeContributionsAsync(CancellationToken ct = default)
        {
            var contributions = await reportRepository.GetEmployeeContributionsAsync(ct);
            return contributions.Select(c => new EmployeeContributionDto
            {
                UserId = c.UserId,
                UserName = c.UserName,
                IdeasSubmitted = c.IdeasSubmitted,
                IdeasApproved = c.IdeasApproved,
                CommentsPosted = c.CommentsPosted,
                VotesGiven = c.VotesGiven,
                ApprovalRate = c.IdeasSubmitted > 0 ? Math.Round((decimal)c.IdeasApproved / c.IdeasSubmitted * 100, 2) : 0
            });
        }
    }
}