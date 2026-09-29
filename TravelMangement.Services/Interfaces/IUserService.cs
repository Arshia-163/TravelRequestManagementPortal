using TravelManagement.Services.Shared.DTOs;

namespace TravelManagement.Services.Interfaces;

/// <summary>User administration and the authenticated user's own profile (spec sections 3, 4, 18).</summary>
public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync();

    Task<IReadOnlyList<UserDto>> GetRecentlyJoinedAsync(int count);

    Task<UserDto> CreateUserAsync(UserInput input);

    Task<UserDto> UpdateUserAsync(int id, UserInput input);

    /// <summary>Locks or unlocks an employee's ability to sign in (spec-adjacent admin feature —
    /// not a hard delete, since TravelRequest/AuditLog history references the user).</summary>
    Task<UserDto> SetActiveAsync(int currentUserId, int targetUserId, bool active);

    Task DeleteUserAsync(int currentUserId, int targetUserId);

    Task<CurrentUserDto> GetCurrentUserAsync(int userId);
}
