// Repositories/ReportsRepository.cs
using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_trial.Repositories
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly IdeaBoardDbContext reportsRepository;

        public ReportsRepository(IdeaBoardDbContext context)
        {
            reportsRepository = context;
        }

        public async Task<IdeaCounts> GetIdeaCountsAsync(CancellationToken ct = default)
        {
            var g = await reportsRepository.Ideas
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Approved = g.Count(i => i.Status == IdeaStatus.Approved),
                    Rejected = g.Count(i => i.Status == IdeaStatus.Rejected),
                    UnderReview = g.Count(i => i.Status == IdeaStatus.UnderReview)
                })
                .FirstOrDefaultAsync(ct) ?? new { Total = 0, Approved = 0, Rejected = 0, UnderReview = 0 };

            return new IdeaCounts(g.Total, g.Approved, g.Rejected, g.UnderReview);
        }

        public async Task<UserCounts> GetUserCountsAsync(CancellationToken ct = default)
        {
            var totals = await reportsRepository.Users.AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalUsers = g.Count(),
                    ActiveUsers = g.Count(u => u.Status == UserStatus.Active)
                }).FirstOrDefaultAsync(ct) ?? new { TotalUsers = 0, ActiveUsers = 0 };

            var roleCounts = await reportsRepository.Users.AsNoTracking()
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            int managers = roleCounts.FirstOrDefault(x => x.Role == UserRole.Manager)?.Count ?? 0;
            int employees = roleCounts.FirstOrDefault(x => x.Role == UserRole.Employee)?.Count ?? 0;
            int admins = roleCounts.FirstOrDefault(x => x.Role == UserRole.Admin)?.Count ?? 0;

            return new UserCounts(totals.TotalUsers, totals.ActiveUsers, managers, employees, admins);
        }

        public async Task<CategoryCounts> GetCategoryCountsAsync(CancellationToken ct = default)
        {
            var totals = await reportsRepository.Categories.AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalCategories = g.Count(),
                    ActiveCategories = g.Count(c => c.IsActive)
                })
                .FirstOrDefaultAsync(ct) ?? new { TotalCategories = 0, ActiveCategories = 0 };

            return new CategoryCounts(totals.TotalCategories, totals.ActiveCategories);
        }

        public async Task<List<CategoryAggregate>> GetCategoryAggregatesAsync(CancellationToken ct = default)
        {
            // Pre-aggregate ideas by category
            var ideaAgg = reportsRepository.Ideas.AsNoTracking()
                .GroupBy(i => i.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Total = g.Count(),
                    Approved = g.Count(i => i.Status == IdeaStatus.Approved),
                    Rejected = g.Count(i => i.Status == IdeaStatus.Rejected),
                    UnderReview = g.Count(i => i.Status == IdeaStatus.UnderReview)
                });

            var query =
                from c in reportsRepository.Categories.AsNoTracking()
                join a in ideaAgg on c.CategoryId equals a.CategoryId into agg
                from a in agg.DefaultIfEmpty()
                orderby (a != null ? a.Total : 0) descending
                select new CategoryAggregate(
                    c.CategoryId,
                    c.Name,
                    a != null ? a.Total : 0,
                    a != null ? a.Approved : 0,
                    a != null ? a.Rejected : 0,
                    a != null ? a.UnderReview : 0
                );

            return await query.ToListAsync(ct);
        }

        public async Task<CategoryAggregate?> GetSingleCategoryAggregateAsync(Guid categoryId, CancellationToken ct = default)
        {
            var ideaAggForCategory = await reportsRepository.Ideas.AsNoTracking()
                .Where(i => i.CategoryId == categoryId)
                .GroupBy(i => i.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Total = g.Count(),
                    Approved = g.Count(i => i.Status == IdeaStatus.Approved),
                    Rejected = g.Count(i => i.Status == IdeaStatus.Rejected),
                    UnderReview = g.Count(i => i.Status == IdeaStatus.UnderReview)
                })
                .FirstOrDefaultAsync(ct);

            var cat = await reportsRepository.Categories.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId, ct);

            if (cat == null) return null;

            return new CategoryAggregate(
                cat.CategoryId,
                cat.Name,
                ideaAggForCategory?.Total ?? 0,
                ideaAggForCategory?.Approved ?? 0,
                ideaAggForCategory?.Rejected ?? 0,
                ideaAggForCategory?.UnderReview ?? 0
            );
        }

        public async Task<List<DateBucketAggregate>> GetIdeasByDateRangeAggregatesAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            var query = await reportsRepository.Ideas.AsNoTracking()
                .Where(i => i.SubmittedDate >= start && i.SubmittedDate <= end)
                .GroupBy(i => i.SubmittedDate.Date)
                .Select(g => new DateBucketAggregate(
                    g.Key,
                    g.Count(),
                    g.Count(i => i.Status == IdeaStatus.Approved),
                    g.Count(i => i.Status == IdeaStatus.Rejected),
                    g.Count(i => i.Status == IdeaStatus.UnderReview)
                ))
                .OrderBy(x => x.Date)
                .ToListAsync(ct);

            return query;
        }

        public async Task<List<ApprovalTrendBucket>> GetApprovalTrendsAsync(DateTime startInclusive, CancellationToken ct = default)
        {
            var q = await reportsRepository.Ideas.AsNoTracking()
                .Where(i => i.SubmittedDate >= startInclusive)
                .GroupBy(i => new { i.SubmittedDate.Year, i.SubmittedDate.Month })
                .Select(g => new ApprovalTrendBucket(
                    g.Key.Year,
                    g.Key.Month,
                    g.Count(),
                    g.Count(i => i.Status == IdeaStatus.Approved)
                ))
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync(ct);

            return q;
        }

        public async Task<List<EmployeeContribution>> GetEmployeeContributionsAsync(CancellationToken ct = default)
        {
            var q = await reportsRepository.Users.AsNoTracking()
                .Where(u => u.Status == UserStatus.Active)
                .Select(u => new EmployeeContribution(
                    u.UserId,
                    u.Name,
                    u.SubmittedIdeas.Count(),
                    u.SubmittedIdeas.Count(i => i.Status == IdeaStatus.Approved),
                    u.Comments.Count(),
                    u.Votes.Count()
                ))
                .OrderByDescending(e => e.IdeasSubmitted * 10 + e.CommentsPosted * 5 + e.VotesGiven)
                .ToListAsync(ct);

            return q;
        }

        public async Task<List<CategoryAggregate>> GetTopCategoryAggregatesAsync(int limit, CancellationToken ct = default)
        {
            var all = await GetCategoryAggregatesAsync(ct);
            return all.Take(limit).ToList();
        }

        public async Task<List<LatestIdeaProjection>> GetLatestIdeasAsync(int limit, CancellationToken ct = default)
        {
            var q = await reportsRepository.Ideas.AsNoTracking()
                .OrderByDescending(i => i.SubmittedDate)
                .Take(limit)
                .Select(i => new LatestIdeaProjection(
                    i.IdeaId,
                    i.Title,
                    i.Description,
                    i.SubmittedByUser.Name,
                    i.Category.Name,
                    i.Status,
                    i.SubmittedDate
                ))
                .ToListAsync(ct);

            return q;
        }

        public Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken ct = default)
        {
            return reportsRepository.Categories.AsNoTracking().AnyAsync(c => c.CategoryId == categoryId, ct);
        }
    }
}