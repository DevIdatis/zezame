namespace Zezame.Api.Services.Interfaces {
    public interface IAuthorizationService {
        Task EnsureCanCreateProjectsAsync(Guid userId);
        Task EnsureCanManageProjectAsync(Guid userId, Guid projectId);
        Task EnsureCanAssignActivitiesAsync(Guid userId, Guid projectId);
    }
}
