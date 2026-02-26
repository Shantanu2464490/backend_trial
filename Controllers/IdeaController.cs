using backend_trial.Models.DTO.Idea;
using backend_trial.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdeaController : ControllerBase
    {
        private readonly IIdeaService ideaService;

        public IdeaController(IIdeaService ideaService)
        {
            this.ideaService = ideaService;
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetAllIdeas()
        {
            var ideas = await ideaService.GetAllIdeasAsync();
            return Ok(ideas);
        }

        // Get all ideas submitted by current employee
        [HttpGet("my-ideas")]
        public async Task<ActionResult> GetMyIdeas()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var ideas = await ideaService.GetMyIdeasAsync(userGuid);
            return Ok(ideas);
        }

        // Get idea by ID
        [HttpGet("{id}")]
        public async Task<ActionResult> GetIdeaById(Guid id)
        {
            var idea = await ideaService.GetIdeaByIdAsync(id);
            return Ok(idea);
        }

        // Submit new idea (Employee role)
        [HttpPost("submit")]
        public async Task<ActionResult> SubmitIdea([FromBody] IdeaRequestDto ideaRequestDto)
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

            var responseIdea = await ideaService.SubmitIdeaAsync(userGuid, ideaRequestDto);

            return CreatedAtAction(nameof(GetIdeaById), new { id = responseIdea.IdeaId }, responseIdea);
        }

        // Update idea (Employee can only update their own ideas)
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateIdea(Guid id, [FromBody] IdeaRequestDto ideaRequestDto)
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

            var responseIdea = await ideaService.UpdateIdeaAsync(id, userGuid, ideaRequestDto);

            return Ok(new { Message = "Idea updated successfully", Idea = responseIdea });
        }

        // Delete idea (Employee can only delete their own ideas that are not Approved)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteIdea(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            await ideaService.DeleteIdeaAsync(id, userGuid);

            return Ok(new { Message = "Idea deleted successfully" });
        }
    }
}
