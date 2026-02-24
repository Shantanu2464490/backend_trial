using System.Security.Claims;
using backend_trial.Models.DTO;
using backend_trial.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoteController : ControllerBase
    {
        private readonly IVoteService voteService;

        public VoteController(IVoteService voteService)
        {
            this.voteService = voteService;
        }

        // GET: /api/vote/{ideaId}/user-vote
        [HttpGet("{ideaId:guid}/user-vote")]
        [Authorize]
        public async Task<ActionResult<UserVoteResponseDto>> GetUserVoteStatus(Guid ideaId, CancellationToken ct)
        {
            var userId = GetCurrentUserIdOrThrow();
            var dto = await voteService.GetUserVoteStatusAsync(ideaId, userId, ct);
            return Ok(dto);
        }

        // POST: /api/vote/{ideaId}/upvote
        [HttpPost("{ideaId:guid}/upvote")]
        [Authorize]
        public async Task<IActionResult> AddUpvote(Guid ideaId, CancellationToken ct)
        {
            var userId = GetCurrentUserIdOrThrow();
            var vote = await voteService.AddUpvoteAsync(ideaId, userId, ct);
            return Ok(new { Message = "Upvote added successfully", Vote = vote });
        }

        // POST: /api/vote/{ideaId}/downvote
        [HttpPost("{ideaId:guid}/downvote")]
        [Authorize]
        public async Task<IActionResult> AddDownvote(Guid ideaId, [FromBody] VoteWithCommentRequestDto request, CancellationToken ct)
        {
            // [ApiController] automatically returns 400 if ModelState is invalid
            var userId = GetCurrentUserIdOrThrow();
            var vote = await voteService.AddDownvoteAsync(ideaId, userId, request.CommentText, ct);
            return Ok(new { Message = "Downvote added successfully with comment", Vote = vote });
        }

        // DELETE: /api/vote/{ideaId}
        [HttpDelete("{ideaId:guid}")]
        [Authorize]
        public async Task<IActionResult> RemoveVote(Guid ideaId, CancellationToken ct)
        {
            var userId = GetCurrentUserIdOrThrow();
            await voteService.RemoveVoteAsync(ideaId, userId, ct);
            return Ok(new { Message = "Vote removed successfully" });
        }

        // GET: /api/vote/{ideaId}
        [HttpGet("{ideaId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVotesForIdea(Guid ideaId, CancellationToken ct)
        {
            var votes = await voteService.GetVotesForIdeaAsync(ideaId, ct);
            return Ok(votes);
        }

        private Guid GetCurrentUserIdOrThrow()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
                throw new ("User ID not found in token");
            return userGuid;
        }
    }
}