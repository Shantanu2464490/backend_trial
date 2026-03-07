using backend_trial.Models.Domain;
using backend_trial.Models.DTO.Comment;
using backend_trial.Models.DTO.Idea;
using backend_trial.Models.DTO.Review;
using backend_trial.Repositories.Interfaces;
using backend_trial.Services.Interfaces;

namespace backend_trial.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository reviewRepository;
        private readonly INotificationService notificationService
            ;

        public ReviewService(IReviewRepository reviewRepository, INotificationService notificationService)
        {
           this. reviewRepository = reviewRepository;
           this. notificationService = notificationService;
        }

        public async Task<IEnumerable<IdeaWithDetailsResponseDto>> GetAllIdeasForReviewAsync(CancellationToken ct)
        {
            // Only return ideas that are under review or have been reviewed by the manager
            var ideas = await reviewRepository.GetAllIdeasForReviewAsync(ct);
            return ideas.Select(MapIdea);
        }

        public async Task<IEnumerable<IdeaWithDetailsResponseDto>> GetIdeasByStatusAsync(string status, CancellationToken ct)
        {
            // Only return ideas that match the specified status and are under review or have been reviewed by the manager
            if (!Enum.TryParse<IdeaStatus>(status, true, out var parsedStatus))
                throw new("Invalid status value");

            var ideas = await reviewRepository.GetIdeasByStatusAsync(parsedStatus, ct);
            return ideas.Select(MapIdea);
        }

        public async Task<IdeaWithDetailsResponseDto> GetIdeaForReviewAsync(Guid ideaId, CancellationToken ct)
        {
            // Return the idea details only if it's under review or has been reviewed by the manager
            var idea = await reviewRepository.GetIdeaForReviewAsync(ideaId, ct)
                ?? throw new ("Idea not found");

            return MapIdea(idea);
        }

        public async Task<ReviewResponseDto> SubmitFeedbackAsync(Guid ideaId, Guid managerId, string feedback, CancellationToken ct)
        {
            // Validate that the idea exists and is under review or has been reviewed by the manager
            if (!await reviewRepository.IdeaExistsAsync(ideaId, ct))
                throw new ("Idea not found");

            var manager = await reviewRepository.GetUserAsync(managerId, ct)
                ?? throw new ("Manager not found");
            // map the review to the domain model and save it
            var review = new Review
            {
                ReviewId = Guid.NewGuid(),
                IdeaId = ideaId,
                ReviewerId = managerId,
                Feedback = feedback,
                ReviewDate = DateTime.UtcNow
            };
            // Save the review
            await reviewRepository.AddReviewAsync(review, ct);
            await reviewRepository.SaveChangesAsync(ct);
            // Return the saved review details
            return new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                IdeaId = review.IdeaId,
                ReviewerId = review.ReviewerId,
                ReviewerName = manager.Name,
                Feedback = review.Feedback,
                ReviewDate = review.ReviewDate
            };
        }

        public async Task<ReviewResponseDto> GetReviewByIdAsync(Guid id, CancellationToken ct)
        {
            // Validate that the review exists
            var review = await reviewRepository.GetReviewByIdAsync(id, ct)
                ?? throw new ("Review not found");
            // Return the review details
            return new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                IdeaId = review.IdeaId,
                ReviewerId = review.ReviewerId,
                ReviewerName = review.Reviewer.Name,
                Feedback = review.Feedback,
                ReviewDate = review.ReviewDate
            };
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsForIdeaAsync(Guid ideaId, CancellationToken ct)
        {
            // Validate that the idea exists and is under review or has been reviewed by the manager
            if (!await reviewRepository.IdeaExistsAsync(ideaId, ct))
                throw new ("Idea not found");

            var reviews = await reviewRepository.GetReviewsForIdeaAsync(ideaId, ct);
            return reviews.Select(r => new ReviewResponseDto
            {
                ReviewId = r.ReviewId,
                IdeaId = r.IdeaId,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.Reviewer.Name,
                Feedback = r.Feedback,
                ReviewDate = r.ReviewDate
            });
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetMyReviewsAsync(Guid managerId, CancellationToken ct)
        {
            // get the review details
            var reviews = await reviewRepository.GetReviewsByManagerAsync(managerId, ct);
            return reviews.Select(r => new ReviewResponseDto
            {
                ReviewId = r.ReviewId,
                IdeaId = r.IdeaId,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.Reviewer.Name,
                Feedback = r.Feedback,
                ReviewDate = r.ReviewDate
            });
        }

        public async Task ChangeIdeaStatusAsync(Guid ideaId, string newStatusStr, string? comment, Guid managerId, CancellationToken ct)
        {
            // checking if idea exists
            var idea = await reviewRepository.GetIdeaWithReviewerAsync(ideaId, ct)
                ?? throw new ("Idea not found");

            // checking idea status
            if (!Enum.TryParse<IdeaStatus>(newStatusStr, true, out var newStatus))
                throw new ("Invalid status");

            // making sure comment is provided when rejecting an idea
            if (newStatus == IdeaStatus.Rejected && string.IsNullOrWhiteSpace(comment))
                throw new ("Comment is required when rejecting");

            // only allow status change if the idea is currently under review or if the manager is the original reviewer
            if (idea.Status != IdeaStatus.UnderReview &&
                idea.ReviewedByUserId != managerId)
                throw new ("Only original reviewer can change status");
            // get manager details for notification
            var manager = await reviewRepository.GetUserAsync(managerId, ct);

            idea.Status = newStatus;
            idea.ReviewComment = comment;
            idea.ReviewedByUserId = managerId;
            idea.ReviewedByUserName = manager?.Name ?? "Manager";

            // save the changes
            await reviewRepository.UpdateIdeaAsync(idea, ct);
            await reviewRepository.SaveChangesAsync(ct);

            // send notification to the idea submitter about the decision
            if (newStatus == IdeaStatus.Rejected || newStatus == IdeaStatus.Approved)
            {
                var decision = newStatus == IdeaStatus.Approved ? "Approved" : "Rejected";
                await notificationService.CreateManagerDecisionNotificationAsync(
                    idea.IdeaId, idea.Title, idea.SubmittedByUserId, managerId, manager?.Name ?? "Manager", decision);
            }
        }

        // Helper mapping
        private IdeaWithDetailsResponseDto MapIdea(Idea i)
        {
            return new IdeaWithDetailsResponseDto
            {
                IdeaId = i.IdeaId,
                Title = i.Title,
                Description = i.Description,
                CategoryId = i.CategoryId,
                CategoryName = i.Category?.Name,
                SubmittedByUserId = i.SubmittedByUserId,
                SubmittedByUserName = i.SubmittedByUser?.Name,
                SubmittedDate = i.SubmittedDate,
                Status = i.Status.ToString(),
                Upvotes = i.Votes.Count(v => v.VoteType == VoteType.Upvote),
                Downvotes = i.Votes.Count(v => v.VoteType == VoteType.Downvote),
                ReviewedByUserId = i.ReviewedByUserId,
                ReviewedByUserName = i.ReviewedByUserName,
                ReviewComment = i.ReviewComment,
                Comments = i.Comments.Select(c => new CommentResponseDto
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    Text = c.Text,
                    CreatedDate = c.CreatedDate
                }).ToList(),
                Reviews = i.Reviews.Select(r => new ReviewResponseDto
                {
                    ReviewId = r.ReviewId,
                    IdeaId = r.IdeaId,
                    ReviewerId = r.ReviewerId,
                    ReviewerName = r.Reviewer.Name,
                    Feedback = r.Feedback,
                    ReviewDate = r.ReviewDate
                }).ToList()
            };
        }
    }
}