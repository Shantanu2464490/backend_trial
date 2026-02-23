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

        // Submit Feedback on an idea
        [HttpPost("feedback/{ideaId}")]
        public async Task<ActionResult> SubmitFeedback(Guid ideaId, [FromBody] ReviewFeedbackDto request)
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

                var managerGuid = GetCurrentUserId();

                // Verify idea exists
                var idea = await _dbContext.Ideas
                    .Include(i => i.SubmittedByUser)
                    .FirstOrDefaultAsync(i => i.IdeaId == ideaId);

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

                // Create review with feedback
                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    IdeaId = ideaId,
                    ReviewerId = managerGuid,
                    Feedback = request.Feedback,
                    ReviewDate = DateTime.UtcNow
                };

                _dbContext.Reviews.Add(review);
                await _dbContext.SaveChangesAsync();

                var response = new ReviewResponseDto
                {
                    ReviewId = review.ReviewId,
                    IdeaId = review.IdeaId,
                    ReviewerId = review.ReviewerId,
                    ReviewerName = manager.Name,
                    Feedback = review.Feedback,
                    ReviewDate = review.ReviewDate
                };

                return CreatedAtAction(nameof(GetReviewById), new { id = review.ReviewId }, 
                    new { Message = "Feedback submitted successfully", Data = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error submitting feedback", Error = ex.Message });
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

        // Change idea status (with review comment for rejection)
        [HttpPut("ideas/{ideaId}/status")]
        public async Task<ActionResult> ChangeIdeaStatus(Guid ideaId, [FromBody] ChangeIdeaStatusRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!Enum.TryParse<IdeaStatus>(request.Status, true, out var newStatus))
                {
                    return BadRequest(new { Message = "Invalid status. Valid values are: Rejected, UnderReview, Approved" });
                }

                // Validate review comment is mandatory for rejection
                if (newStatus == IdeaStatus.Rejected && string.IsNullOrWhiteSpace(request.ReviewComment))
                {
                    return BadRequest(new { Message = "Review comment is mandatory when rejecting an idea" });
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

                // Authorization: Only original reviewer can change status after review
                if (idea.Status != IdeaStatus.UnderReview)
                {
                    if (idea.ReviewedByUserId != currentUserId)
                    {
                        return Forbid("Only the original reviewer can change the status of this idea");
                    }
                }

                var oldStatus = idea.Status;
                idea.Status = newStatus;
                idea.ReviewedByUserId = currentUserId;
                idea.ReviewComment = request.ReviewComment;

                var manager = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (manager != null)
                {
                    idea.ReviewedByUserName = manager.Name;
                }

                _dbContext.Ideas.Update(idea);
                await _dbContext.SaveChangesAsync();

                // Send notification for rejection
                if (newStatus == IdeaStatus.Rejected)
                {
                    await _notificationService.CreateManagerDecisionNotificationAsync(
                        idea.IdeaId,
                        idea.Title,
                        idea.SubmittedByUserId,
                        currentUserId,
                        manager?.Name ?? "Manager",
                        "Rejected"
                    );
                }

                return Ok(new { 
                    Message = $"Status changed from {oldStatus} to {newStatus} successfully",
                    Data = new
                    {
                        IdeaId = idea.IdeaId,
                        Status = idea.Status.ToString(),
                        ReviewedByUserId = idea.ReviewedByUserId,
                        ReviewedByUserName = idea.ReviewedByUserName,
                        ReviewComment = idea.ReviewComment
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error changing idea status", Error = ex.Message });
            }
        }
    }
}