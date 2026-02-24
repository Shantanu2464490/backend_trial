using backend_trial.Models.Domain;

namespace backend_trial.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNewIdeaNotificationAsync(Guid ideaId, string ideaTitle, Guid submittedByUserId);
        Task CreateManagerDecisionNotificationAsync(Guid ideaId, string ideaTitle, Guid submittedByUserId, Guid reviewerId, string reviewerName, string decision);
    }
}