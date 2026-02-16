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
                    return BadRequest(new { Message = "Invalid status. Valid values are: Draft, UnderReview, Approved" });
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

        // Change idea status (Draft -> UnderReview -> Approved)
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
                    return BadRequest(new { Message = "Invalid status. Valid values are: Draft, UnderReview, Approved" });
                }

                var idea = await _dbContext.Ideas
                    .Include(i => i.Category)
                    .Include(i => i.SubmittedByUser)
                    .FirstOrDefaultAsync(i => i.IdeaId == ideaId);

                if (idea == null)
                {
                    return NotFound(new { Message = "Idea not found" });
                }

                var oldStatus = idea.Status;
                idea.Status = newStatus;

                _dbContext.Ideas.Update(idea);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = $"Idea status changed from {oldStatus} to {newStatus}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error changing idea status", Error = ex.Message });
            }
        }

        // Submit a review on an idea
        [HttpPost("submit")]
        public async Task<ActionResult> SubmitReview([FromBody] ReviewWithIdeaRequestDto request)
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

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var managerGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                // Verify idea exists
                var idea = await _dbContext.Ideas.FirstOrDefaultAsync(i => i.IdeaId == request.IdeaId);
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
                    ReviewDate = DateTime.UtcNow
                };

                _dbContext.Reviews.Add(review);
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var managerGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

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
                        ReviewDate = r.ReviewDate
                    })
                    .ToListAsync();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving your reviews", Error = ex.Message });
            }
        }

        // Update a review (only own reviews)
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateReview(Guid id, [FromBody] ReviewRequestDto request)
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

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var managerGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var review = await _dbContext.Reviews
                    .Include(r => r.Reviewer)
                    .FirstOrDefaultAsync(r => r.ReviewId == id);

                if (review == null)
                {
                    return NotFound(new { Message = "Review not found" });
                }

                // Check if user is the reviewer
                if (review.ReviewerId != managerGuid)
                {
                    return Forbid("You can only update your own reviews");
                }

                review.Feedback = request.Feedback;
                review.Decision = reviewDecision;
                review.ReviewDate = DateTime.UtcNow;

                _dbContext.Reviews.Update(review);
                await _dbContext.SaveChangesAsync();

                var response = new ReviewResponseDto
                {
                    ReviewId = review.ReviewId,
                    IdeaId = review.IdeaId,
                    ReviewerId = review.ReviewerId,
                    ReviewerName = review.Reviewer.Name,
                    Feedback = review.Feedback,
                    Decision = review.Decision.ToString(),
                    ReviewDate = review.ReviewDate
                };

                return Ok(new { Message = "Review updated successfully", Review = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error updating review", Error = ex.Message });
            }
        }

        // Delete a review (only own reviews)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReview(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var managerGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.ReviewId == id);
                if (review == null)
                {
                    return NotFound(new { Message = "Review not found" });
                }

                // Check if user is the reviewer
                if (review.ReviewerId != managerGuid)
                {
                    return Forbid("You can only delete your own reviews");
                }

                _dbContext.Reviews.Remove(review);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Review deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error deleting review", Error = ex.Message });
            }
        }
    }
}