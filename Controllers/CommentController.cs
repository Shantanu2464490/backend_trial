using backend_trial.Models.DTO;
using backend_trial.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Employee")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService commentService;

        public CommentController(ICommentService commentService)
        {
            this.commentService = commentService;
        }

        // Add comment to an idea
        [HttpPost("{ideaId}")]
        public async Task<ActionResult> AddComment(Guid ideaId, [FromBody] CommentRequestDto request)
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

            var response = await commentService.AddCommentAsync(ideaId, userGuid, request);

            return CreatedAtAction(nameof(GetCommentById), new { id = response.CommentId }, response);
        }

        // Get all comments for an idea
        [HttpGet("{ideaId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetCommentsForIdea(Guid ideaId)
        {
            var comments = await commentService.GetCommentsByIdeaIdAsync(ideaId);
            return Ok(comments);
        }

        // Get comment by ID
        [HttpGet("comment/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetCommentById(Guid id)
        {
            var response = await commentService.GetCommentByIdAsync(id);
            return Ok(response);
        }

        // Update comment (only own comments)
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateComment(Guid id, [FromBody] CommentRequestDto request)
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

            var response = await commentService.UpdateCommentAsync(id, userGuid, request);

            return Ok(new { Message = "Comment updated successfully", Comment = response });
        }

        // Delete comment (only own comments)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteComment(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            await commentService.DeleteCommentAsync(id, userGuid);

            return Ok(new { Message = "Comment deleted successfully" });
        }
    }
}
