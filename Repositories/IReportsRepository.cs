// Repositories/Interfaces/IReportsRepository.cs
using backend_trial.Models.Domain;

namespace backend_trial.Repositories.Interfaces
{
    public record IdeaCounts(int Total, int Approved, int Rejected, int UnderReview);
    public record UserCounts(int TotalUsers, int ActiveUsers, int Managers, int Employees, int Admins);
    public record CategoryCounts(int TotalCategories, int ActiveCategories);

    public record CategoryAggregate(
        Guid CategoryId,
        string CategoryName,
        int IdeasSubmitted,
        int ApprovedIdeas,
        int RejectedIdeas,
        int UnderReviewIdeas
    );

    public record DateBucketAggregate(
        DateTime Date,
        int IdeasSubmitted,
        int IdeasApproved,
        int IdeasRejected,
        int IdeasUnderReview
    );

    public record ApprovalTrendBucket(
        int Year,
        int Month,
        int IdeasSubmitted,
        int IdeasApproved
    );

    public record EmployeeContribution(
        Guid UserId,
        string UserName,
        int IdeasSubmitted,
        int IdeasApproved,
        int CommentsPosted,
        int VotesGiven
    );

    public record LatestIdeaProjection(
        Guid IdeaId,
        string Title,
        string? Description,
        string SubmittedBy,
        string CategoryName,
        IdeaStatus Status,
        DateTime SubmittedDate
    );

    public interface IReportsRepository
    {
        Task<IdeaCounts> GetIdeaCountsAsync(CancellationToken ct = default);
        Task<UserCounts> GetUserCountsAsync(CancellationToken ct = default);
        Task<CategoryCounts> GetCategoryCountsAsync(CancellationToken ct = default);

        Task<List<CategoryAggregate>> GetCategoryAggregatesAsync(CancellationToken ct = default);
        Task<CategoryAggregate?> GetSingleCategoryAggregateAsync(Guid categoryId, CancellationToken ct = default);

        Task<List<DateBucketAggregate>> GetIdeasByDateRangeAggregatesAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<List<ApprovalTrendBucket>> GetApprovalTrendsAsync(DateTime startInclusive, CancellationToken ct = default);

        Task<List<EmployeeContribution>> GetEmployeeContributionsAsync(CancellationToken ct = default);
        Task<List<CategoryAggregate>> GetTopCategoryAggregatesAsync(int limit, CancellationToken ct = default);
        Task<List<LatestIdeaProjection>> GetLatestIdeasAsync(int limit, CancellationToken ct = default);

        Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken ct = default);
    }
}
