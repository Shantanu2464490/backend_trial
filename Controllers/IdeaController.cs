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
    public class IdeaController : ControllerBase
    {
        private readonly IdeaBoardDbContext _dbContext;
        private readonly INotificationService _notificationService;

        public IdeaController(IdeaBoardDbContext dbContext, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetAllIdeas()
        {
            try
            {
                var ideas = await _dbContext.Ideas
                    .Include(i => i.Category)
                    .Include(i => i.SubmittedByUser)
                    .Include(i => i.Comments)
                        .ThenInclude(c => c.User)
                    .Include(i => i.Votes)
                    .Select(i => new IdeaResponseDto
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
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(ideas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving ideas", Error = ex.Message });
            }
        }

        // Get all ideas submitted by current employee
        [HttpGet("my-ideas")]
        public async Task<ActionResult> GetMyIdeas()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var ideas = await _dbContext.Ideas
                    .Where(i => i.SubmittedByUserId == userGuid)
                    .Include(i => i.Category)
                    .Include(i => i.Comments)
                        .ThenInclude(c => c.User)
                    .Include(i => i.Votes)
                    .Select(i => new IdeaResponseDto
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
                        }).ToList()
                    })
                    .OrderByDescending(i => i.SubmittedDate)
                    .ToListAsync();

                return Ok(ideas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error retrieving your ideas", Error = ex.Message });
            }
        }

        // Get idea by ID
        [HttpGet("{id}")]
        public async Task<ActionResult> GetIdeaById(Guid id)
        {
            try
            {
                var idea = await _dbContext.Ideas
                    .Where(i => i.IdeaId == id)
                    .Include(i => i.Category)
                    .Include(i => i.SubmittedByUser)
                    .Include(i => i.Comments)
                        .ThenInclude(c => c.User)
                    .Include(i => i.Votes)
                    .Select(i => new IdeaResponseDto
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
                return BadRequest(new { Message = "Error retrieving idea", Error = ex.Message });
            }
        }
        // Submit new idea (Employee role)
        [HttpPost("submit")]
        public async Task<ActionResult> SubmitIdea([FromBody] IdeaRequestDto ideaRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                // Verify category exists and is active
                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == ideaRequestDto.CategoryId);
                if (category == null)
                {
                    return NotFound(new { Message = "Category not found" });
                }

                if (!category.IsActive)
                {
                    return BadRequest(new { Message = "Selected category is inactive" });
                }

                // Verify user exists
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userGuid);
                if (user == null)
                {
                    return Unauthorized(new { Message = "User not found" });
                }

                var newIdea = new Idea
                {
                    IdeaId = Guid.NewGuid(),
                    Title = ideaRequestDto.Title,
                    Description = ideaRequestDto.Description,
                    CategoryId = ideaRequestDto.CategoryId,
                    SubmittedByUserId = userGuid,
                    SubmittedDate = DateTime.UtcNow,
                    Status = IdeaStatus.UnderReview
                };

                _dbContext.Ideas.Add(newIdea);
                await _dbContext.SaveChangesAsync();

                // Notify admins about the new idea
                await _notificationService.CreateNewIdeaNotificationAsync(newIdea.IdeaId, newIdea.Title, newIdea.SubmittedByUserId);

                var responseIdea = new IdeaResponseDto
                {
                    IdeaId = newIdea.IdeaId,
                    Title = newIdea.Title,
                    Description = newIdea.Description,
                    CategoryId = newIdea.CategoryId,
                    CategoryName = category.Name,
                    SubmittedByUserId = newIdea.SubmittedByUserId,
                    SubmittedByUserName = user.Name,
                    SubmittedDate = newIdea.SubmittedDate,
                    Status = newIdea.Status.ToString(),
                    Upvotes = 0,
                    Downvotes = 0,
                    Comments = new List<CommentResponseDto>()
                };

                return CreatedAtAction(nameof(GetIdeaById), new { id = newIdea.IdeaId }, responseIdea);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error submitting idea", Error = ex.Message });
            }
        }

        // Update idea (Employee can only update their own ideas)
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateIdea(Guid id, [FromBody] IdeaRequestDto ideaRequestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var idea = await _dbContext.Ideas
                    .Include(i => i.Category)
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(i => i.IdeaId == id);

                if (idea == null)
                {
                    return NotFound(new { Message = "Idea not found" });
                }

                // Check if user is the owner of the idea
                if (idea.SubmittedByUserId != userGuid)
                {
                    return Forbid("You can only update your own ideas");
                }

                // Verify category exists and is active
                var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == ideaRequestDto.CategoryId);
                if (category == null)
                {
                    return NotFound(new { Message = "Category not found" });
                }

                if (!category.IsActive)
                {
                    return BadRequest(new { Message = "Selected category is inactive" });
                }

                idea.Title = ideaRequestDto.Title;
                idea.Description = ideaRequestDto.Description;
                idea.CategoryId = ideaRequestDto.CategoryId;

                _dbContext.Ideas.Update(idea);
                await _dbContext.SaveChangesAsync();

                var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userGuid);

                var responseIdea = new IdeaResponseDto
                {
                    IdeaId = idea.IdeaId,
                    Title = idea.Title,
                    Description = idea.Description,
                    CategoryId = idea.CategoryId,
                    CategoryName = category.Name,
                    SubmittedByUserId = idea.SubmittedByUserId,
                    SubmittedByUserName = updatedUser?.Name ?? "Unknown",
                    SubmittedDate = idea.SubmittedDate,
                    Status = idea.Status.ToString(),
                    Upvotes = idea.Votes.Count(v => v.VoteType == VoteType.Upvote),
                    Downvotes = idea.Votes.Count(v => v.VoteType == VoteType.Downvote),
                    Comments = idea.Comments.Select(c => new CommentResponseDto
                    {
                        CommentId = c.CommentId,
                        UserId = c.UserId,
                        UserName = c.User.Name,
                        Text = c.Text,
                        CreatedDate = c.CreatedDate
                    }).ToList()
                };

                return Ok(new { Message = "Idea updated successfully", Idea = responseIdea });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error updating idea", Error = ex.Message });
            }
        }

        // Delete idea (Employee can only delete their own ideas in Draft status)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteIdea(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { Message = "User ID not found in token" });
                }

                var idea = await _dbContext.Ideas.FirstOrDefaultAsync(i => i.IdeaId == id);
                if (idea == null)
                {
                    return NotFound(new { Message = "Idea not found" });
                }

                // Check if user is the owner of the idea
                if (idea.SubmittedByUserId != userGuid)
                {
                    return Forbid("You can only delete your own ideas");
                }

                // Only allow deletion of draft ideas
                if (idea.Status != IdeaStatus.Draft)
                {
                    return BadRequest(new { Message = "You can only delete ideas in Draft status" });
                }

                _dbContext.Ideas.Remove(idea);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Idea deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error deleting idea", Error = ex.Message });
            }
        }
    }
}

