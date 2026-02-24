using backend_trial.Models.DTO;
using backend_trial.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService reviewService;

        public ReviewController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new ("User ID missing"));

        [HttpGet("ideas")]
        public async Task<IActionResult> GetIdeas(CancellationToken ct)
        {
            return Ok(await reviewService.GetAllIdeasForReviewAsync(ct));
        }

        [HttpGet("ideas/status/{status}")]
        public async Task<IActionResult> GetIdeasByStatus(string status, CancellationToken ct)
        {
            return Ok(await reviewService.GetIdeasByStatusAsync(status, ct));
        }

        [HttpGet("ideas/{ideaId}")]
        public async Task<IActionResult> GetIdea(Guid ideaId, CancellationToken ct)
        {
            return Ok(await reviewService.GetIdeaForReviewAsync(ideaId, ct));
        }

        [HttpPost("feedback/{ideaId}")]
        public async Task<IActionResult> SubmitFeedback(Guid ideaId, [FromBody] ReviewFeedbackDto dto, CancellationToken ct)
        {
            var result = await reviewService.SubmitFeedbackAsync(ideaId, CurrentUserId, dto.Feedback, ct);
            return CreatedAtAction(nameof(GetReview), new { id = result.ReviewId }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(Guid id, CancellationToken ct)
        {
            return Ok(await reviewService.GetReviewByIdAsync(id, ct));
        }

        [HttpGet("idea/{ideaId}")]
        public async Task<IActionResult> GetReviewsForIdea(Guid ideaId, CancellationToken ct)
        {
            return Ok(await reviewService.GetReviewsForIdeaAsync(ideaId, ct));
        }

        [HttpGet("manager/my-reviews")]
        public async Task<IActionResult> GetMyReviews(CancellationToken ct)
        {
            return Ok(await reviewService.GetMyReviewsAsync(CurrentUserId, ct));
        }

        [HttpPut("ideas/{ideaId}/status")]
        public async Task<IActionResult> ChangeStatus(Guid ideaId, [FromBody] ChangeIdeaStatusRequestDto dto, CancellationToken ct)
        {
            await reviewService.ChangeIdeaStatusAsync(ideaId, dto.Status, dto.ReviewComment, CurrentUserId, ct);
            return Ok(new { Message = "Status changed successfully" });
        }
    }
}