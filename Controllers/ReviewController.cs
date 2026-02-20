using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Models.DTO;
using backend_trial.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class ReviewController : ControllerBase
    {
        private readonly IdeaBoardDbContext _dbContext;
        private readonly INotificationService _notificationService;

        public ReviewController(IdeaBoardDbContext dbContext, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userGuid;
        }

        // Get all ideas with votes, comments and reviews (for manager review)
        [HttpGet("ideas")]
        public async Task<ActionResult> GetAllIdeasForReview()
        {
            try
            {
                var ideas = await _dbContext.Ideas
                    .Include(i => i.Category)
                    .Include(i => i.SubmittedByUser)
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                        .ThenInclude(c => c.User)
                    .Include(i => i.Reviews)
                        .ThenInclude(r => r.Reviewer)
                    .Select(i => new IdeaWithDetailsResponseDto
                    {
                        IdeaId = i.IdeaId,
                        Title = i.Title,
                        Description = i.Description,
                        CategoryId = i.CategoryId,
                        CategoryName = i.Category.Name,
                        SubmittedByUserId = i.SubmittedByUserId,
                        SubmittedByUserName = i.SubmittedByUser.Name,
                        SubmittedDate = i.SubmittedDate,
                        Status = i.Status.ToString(),
                        Upvotes = i.Votes.Count(v => v.VoteType == VoteType.Upvote),
                        Downvotes = i.Votes.Count(v => v.VoteType == VoteType.Downvote),
                        ReviewedByUserId = (i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Rejected) ? i.ReviewedByUserId : null,
                        ReviewedByUserName = (i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Rejected) ? i.ReviewedByUserName : null,
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
                            Decision = r.Decision.ToString(),
                            RejectionReason = r.RejectionReason,
                            ReviewDate = r.ReviewDate
                        }).ToList()
                    })
                    .OrderByDescending(i => i.SubmittedDate)
                    .ToListAsync();

                return Ok(ideas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving ideas for review", Error = ex.Message });
            }
        }

        // Get ideas by specific status
        [HttpGet("ideas/status/{status}")]
        public async Task<ActionResult> GetIdeasByStatus(string status)
        {
            try
            {
                // Validate status
                if (!Enum.TryParse<IdeaStatus>(status, true, out var ideaStatus))
                {
                    return BadRequest(new { Message = "Invalid status. Valid values are: Rejected, UnderReview, Approved" });
                }

                var ideas = await _dbContext.Ideas
                    .Where(i => i.Status == ideaStatus)
                    .Include(i => i.Category)
                    .Include(i => i.SubmittedByUser)
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                        .ThenInclude(c => c.User)
                    .Include(i => i.Reviews)
                        .ThenInclude(r => r.Reviewer)
                    .Select(i => new IdeaWithDetailsResponseDto
                    {
                        IdeaId = i.IdeaId,
                        Title = i.Title,
                        Description = i.Description,
                        CategoryId = i.CategoryId,
                        CategoryName = i.Category.Name,
                        SubmittedByUserId = i.SubmittedByUserId,
                        SubmittedByUserName = i.SubmittedByUser.Name,
                        SubmittedDate = i.SubmittedDate,
                        Status = i.Status.ToString(),
                        Upvotes = i.Votes.Count(v => v.VoteType == VoteType.Upvote),
                        Downvotes = i.Votes.Count(v => v.VoteType == VoteType.Downvote),
                        ReviewedByUserId = (i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Rejected) ? i.ReviewedByUserId : null,
                        ReviewedByUserName = (i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Rejected) ? i.ReviewedByUserName : null,
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
                            Decision = r.Decision.ToString(),
                            RejectionReason = r.RejectionReason,
                            ReviewDate = r.ReviewDate
                        }).ToList()
                    })
                    .OrderByDescending(i => i.SubmittedDate)
                    .ToListAsync();

                return Ok(ideas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving ideas by status", Error = ex.Message });
            }
        }

        // Get single idea with all details
        [HttpGet("ideas/{ideaId}")]
        public async Task<ActionResult> GetIdeaForReview(Guid ideaId)
        {
            try
            {
                var idea = await _dbContext.Ideas
                    .Where(i => i.IdeaId == ideaId)
                    .Include(i => i.Category)
                    .Include(i => i.SubmittedByUser)
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                        .ThenInclude(c => c.User)
                    .Include(i => i.Reviews)
                        .ThenInclude(r => r.Reviewer)
                    .Select(i => new IdeaWithDetailsResponseDto
                    {
                        IdeaId = i.IdeaId,
                        Title = i.Title,
                        Description = i.Description,
                        CategoryId = i.CategoryId,
                        CategoryName = i.Category.Name,
                        SubmittedByUserId = i.SubmittedByUserId,
                        SubmittedByUserName = i.SubmittedByUser.Name,
                        SubmittedDate = i.SubmittedDate,
                        Status = i.Status.ToString(),
                        Upvotes = i.Votes.Count(v => v.VoteType == VoteType.Upvote),
                        Downvotes = i.Votes.Count(v => v.VoteType == VoteType.Downvote),
                        ReviewedByUserId = (i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Rejected) ? i.ReviewedByUserId : null,
                        ReviewedByUserName = (i.Status == IdeaStatus.Approved || i.Status == IdeaStatus.Rejected) ? i.ReviewedByUserName : null,
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
                            Decision = r.Decision.ToString(),
                            RejectionReason = r.RejectionReason,
                            ReviewDate = r.ReviewDate
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (idea == null)
                {
                    return NotFound(new { Message = "Idea not found" });
                }

                return Ok(idea);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving idea for review", Error = ex.Message });
            }
        }

        // Change idea status (Rejected -> UnderReview -> Approved)
        [HttpPut("ideas/{ideaId}/status")]
        public async Task<ActionResult> ChangeIdeaStatus(Guid ideaId, [FromBody] ChangeIdeaStatusRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate status
                if (!Enum.TryParse<IdeaStatus>(request.Status, true, out var newStatus))
                {
                    return BadRequest(new { Message = "Invalid status. Valid values are: Rejected, UnderReview, Approved" });
                }

                var currentUserId = GetCurrentUserId();

                var idea = await _dbContext.Ideas
                    .Include(i => i.Category)
                    .Include(i => i.SubmittedByUser)
                    .Include(i => i.Reviews)
                    .FirstOrDefaultAsync(i => i.IdeaId == ideaId);

                if (idea == null)
                {
                    return NotFound(new { Message = "Idea not found" });
                }

                // Authorization Logic:
                // 1. If idea is not reviewed yet (status is UnderReview), any manager can change it
                // 2. If idea is already reviewed (Approved/Rejected), only the original reviewer can change it
                if (idea.Status != IdeaStatus.UnderReview)
                {
                    if (idea.ReviewedByUserId != currentUserId)
                    {
                        return Forbid("Only the original reviewer can change the status of this idea");
                    }
                }

                var oldStatus = idea.Status;
                idea.Status = newStatus;

                _dbContext.Ideas.Update(idea);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = $"Status changed from {oldStatus} to {newStatus} successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error changing idea status", Error = ex.Message });
            }
        }

        // Submit a review on an idea
        [HttpPost("submit")]
        public async Task<ActionResult> SubmitReview([FromBody] ReviewSubmitDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(request.Feedback))
                {
                    return BadRequest(new { Message = "Feedback is required" });
                }

                // Validate decision
                if (!Enum.TryParse<ReviewDecision>(request.Decision, true, out var reviewDecision))
                {
                    return BadRequest(new { Message = "Invalid decision. Valid values are: Approve, Reject" });
                }

                // Validate rejection reason when decision is Reject
                if (reviewDecision == ReviewDecision.Rejected && string.IsNullOrWhiteSpace(request.RejectionReason))
                {
                    return BadRequest(new { Message = "Rejection reason is mandatory when rejecting an idea", ErrorCode = "REJECTION_REASON_REQUIRED" });
                }

                var managerGuid = GetCurrentUserId();

                // Verify idea exists
                var idea = await _dbContext.Ideas
                    .Include(i => i.SubmittedByUser)
                    .FirstOrDefaultAsync(i => i.IdeaId == request.IdeaId);
                
                if (idea == null)
                {
                    return NotFound(new { Message = "Idea not found" });
                }

                // Verify manager exists
                var manager = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == managerGuid);
                if (manager == null)
                {
                    return Unauthorized(new { Message = "Manager not found" });
                }

                // Check if manager has already reviewed this idea
                var existingReview = await _dbContext.Reviews
                    .FirstOrDefaultAsync(r => r.IdeaId == request.IdeaId && r.ReviewerId == managerGuid);

                if (existingReview != null)
                {
                    return BadRequest(new { Message = "You have already submitted a review for this idea" });
                }

                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    IdeaId = request.IdeaId,
                    ReviewerId = managerGuid,
                    Feedback = request.Feedback,
                    Decision = reviewDecision,
                    RejectionReason = reviewDecision == ReviewDecision.Rejected ? request.RejectionReason : null,
                    ReviewDate = DateTime.UtcNow
                };

                _dbContext.Reviews.Add(review);

                // Update Idea with reviewer info
                idea.ReviewedByUserId = managerGuid;
                idea.ReviewedByUserName = manager.Name;
                idea.ReviewedDate = DateTime.UtcNow;
                idea.Status = reviewDecision == ReviewDecision.Approved ? IdeaStatus.Approved : IdeaStatus.Rejected;

                _dbContext.Ideas.Update(idea);
                await _dbContext.SaveChangesAsync();

                // Send notification
                await _notificationService.CreateManagerDecisionNotificationAsync(
                    review.IdeaId,
                    idea.Title,
                    idea.SubmittedByUserId,
                    review.ReviewerId,
                    manager.Name,
                    review.Decision.ToString()
                );

                var response = new ReviewResponseDto
                {
                    ReviewId = review.ReviewId,
                    IdeaId = review.IdeaId,
                    ReviewerId = review.ReviewerId,
                    ReviewerName = manager.Name,
                    Feedback = review.Feedback,
                    Decision = review.Decision.ToString(),
                    RejectionReason = review.RejectionReason,
                    ReviewDate = review.ReviewDate
                };

                return CreatedAtAction(nameof(GetReviewById), new { id = review.ReviewId }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error submitting review", Error = ex.Message });
            }
        }

        // Get review by ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetReviewById(Guid id)
        {
            try
            {
                var review = await _dbContext.Reviews
                    .Include(r => r.Reviewer)
                    .FirstOrDefaultAsync(r => r.ReviewId == id);

                if (review == null)
                {
                    return NotFound(new { Message = "Review not found" });
                }

                var response = new ReviewResponseDto
                {
                    ReviewId = review.ReviewId,
                    IdeaId = review.IdeaId,
                    ReviewerId = review.ReviewerId,
                    ReviewerName = review.Reviewer.Name,
                    Feedback = review.Feedback,
                    Decision = review.Decision.ToString(),
                    RejectionReason = review.RejectionReason,
                    ReviewDate = review.ReviewDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving review", Error = ex.Message });
            }
        }

        // Get all reviews for an idea
        [HttpGet("idea/{ideaId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetReviewsForIdea(Guid ideaId)
        {
            try
            {
                var idea = await _dbContext.Ideas.FirstOrDefaultAsync(i => i.IdeaId == ideaId);
                if (idea == null)
                {
                    return NotFound(new { Message = "Idea not found" });
                }

                var reviews = await _dbContext.Reviews
                    .Where(r => r.IdeaId == ideaId)
                    .Include(r => r.Reviewer)
                    .OrderByDescending(r => r.ReviewDate)
                    .Select(r => new ReviewResponseDto
                    {
                        ReviewId = r.ReviewId,
                        IdeaId = r.IdeaId,
                        ReviewerId = r.ReviewerId,
                        ReviewerName = r.Reviewer.Name,
                        Feedback = r.Feedback,
                        Decision = r.Decision.ToString(),
                        RejectionReason = r.RejectionReason,
                        ReviewDate = r.ReviewDate
                    })
                    .ToListAsync();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving reviews for idea", Error = ex.Message });
            }
        }

        // Get all reviews submitted by current manager
        [HttpGet("manager/my-reviews")]
        public async Task<ActionResult> GetMyReviews()
        {
            try
            {
                var managerGuid = GetCurrentUserId();

                var reviews = await _dbContext.Reviews
                    .Where(r => r.ReviewerId == managerGuid)
                    .Include(r => r.Idea)
                    .Include(r => r.Reviewer)
                    .OrderByDescending(r => r.ReviewDate)
                    .Select(r => new ReviewResponseDto
                    {
                        ReviewId = r.ReviewId,
                        IdeaId = r.IdeaId,
                        ReviewerId = r.ReviewerId,
                        ReviewerName = r.Reviewer.Name,
                        Feedback = r.Feedback,
                        Decision = r.Decision.ToString(),
                        RejectionReason = r.RejectionReason,
                        ReviewDate = r.ReviewDate
                    })
                    .ToListAsync();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving manager reviews", Error = ex.Message });
            }
        }
    }
}