
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.X86;
using TravelManagement.Services.Interfaces;

namespace TravelRequestManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class CurrentUserController : TravelApiControllerBase
{
    private readonly IUserService users;

    public CurrentUserController(IUserService users)
    {
        this.users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(
            await users.GetCurrentUserAsync(CurrentUserId));
    }
}
