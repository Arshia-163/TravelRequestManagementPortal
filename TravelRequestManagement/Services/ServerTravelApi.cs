using System.Net;
using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.Constants;
using TravelManagement.Services.Shared.DTOs;

using TravelRequestManagement.Client.Services;

namespace TravelRequestManagement.Services;

public sealed partial class ServerTravelApi(
    IServiceScopeFactory scopes,
    AuthenticationStateProvider authenticationState,
    ILogger<ServerTravelApi> logger) : ITravelApi
{
    public Task<CurrentUserDto> MeAsync() =>
        Run(
            null,
            (sp, userId) =>
                sp.GetRequiredService<IUserService>()
                    .GetCurrentUserAsync(userId));

    private static readonly string[] TravelAdminOnly =
        [RoleNames.TravelAdmin];

    private static readonly string[] ManagerOnly =
        [RoleNames.Manager];

    private static readonly string[] DepartmentHeadOnly =
        [RoleNames.DepartmentHead];

    private async Task<T> Run<T>(
        string[]? anyOfRoles,
        Func<IServiceProvider, int, Task<T>> work)
    {
        var userId = await RequireUserAsync(anyOfRoles);

        using var scope = scopes.CreateScope();

        try
        {
            return await work(scope.ServiceProvider, userId);
        }
        catch (Exception ex)
        {
            throw Translate(ex);
        }
    }

    private async Task RunVoid(
        string[]? anyOfRoles,
        Func<IServiceProvider, int, Task> work)
    {
        var userId = await RequireUserAsync(anyOfRoles);

        using var scope = scopes.CreateScope();

        try
        {
            await work(scope.ServiceProvider, userId);
        }
        catch (Exception ex)
        {
            throw Translate(ex);
        }
    }

    private async Task<int> RequireUserAsync(string[]? anyOfRoles)
    {
        var state =
            await authenticationState.GetAuthenticationStateAsync();

        var principal = state.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new ApiException(
                "Your session has expired. Please sign in again.",
                HttpStatusCode.Unauthorized);
        }

        if (!int.TryParse(
                principal.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            throw new ApiException(
                "Your session has expired. Please sign in again.",
                HttpStatusCode.Unauthorized);
        }

        if (anyOfRoles is { Length: > 0 }
            && !anyOfRoles.Any(principal.IsInRole))
        {
            throw new ApiException(
                "You don't have permission to do that.",
                HttpStatusCode.Forbidden);
        }

        return userId;
    }

    private ApiException Translate(Exception ex) => ex switch
    {
        ApiException api => api,

        InvalidOperationException =>
            new ApiException(
                ex.Message,
                HttpStatusCode.BadRequest),

        UnauthorizedAccessException =>
            new ApiException(
                string.IsNullOrWhiteSpace(ex.Message)
                    ? "You are not allowed to do this."
                    : ex.Message,
                HttpStatusCode.Forbidden),

        KeyNotFoundException =>
            new ApiException(
                string.IsNullOrWhiteSpace(ex.Message)
                    ? "The requested item was not found."
                    : ex.Message,
                HttpStatusCode.NotFound),

        _ => LogAndWrap(ex)
    };

    private ApiException LogAndWrap(Exception ex)
    {
        logger.LogError(
            ex,
            "Unhandled exception in a server-side travel API operation.");

        return new ApiException(
            "Something went wrong while processing your request. Please try again, and contact support if the problem continues.",
            HttpStatusCode.InternalServerError);
    }
}
