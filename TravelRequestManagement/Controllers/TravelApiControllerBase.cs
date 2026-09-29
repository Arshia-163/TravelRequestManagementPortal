
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TravelRequestManagement.Controllers;


public abstract class TravelApiControllerBase : ControllerBase
{
    protected int CurrentUserId
    {
        get
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var id)
                ? id
                : throw new UnauthorizedAccessException();
        }
    }
}
