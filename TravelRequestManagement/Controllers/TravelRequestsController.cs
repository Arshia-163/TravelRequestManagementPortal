using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.Constants;
using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/requests")]
public sealed class TravelRequestsController(
    ITravelRequestService requests,
    ICancellationService cancellations,
    ITripExtensionService extensions,
    IBookingService bookings) : TravelApiControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> Mine() => Ok(await requests.GetMyAsync(CurrentUserId));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var request = await requests.GetAsync(id, CurrentUserId);
        return request is null ? NotFound(new { message = "Request was not found or you do not have access to it." }) : Ok(request);
    }

    [HttpPost]
    [Authorize(Policy = AppRoles.CanCreateRequestPolicy)]
    public async Task<IActionResult> Create([FromBody] TravelRequestInput input) =>
        Ok(await requests.CreateDraftAsync(CurrentUserId, input));

    [HttpPut("{id:int}")]
    [Authorize(Policy = AppRoles.CanCreateRequestPolicy)]
    public async Task<IActionResult> UpdateDraft(int id, [FromBody] TravelRequestInput input) =>
        Ok(await requests.UpdateDraftAsync(CurrentUserId, id, input));

    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = AppRoles.CanCreateRequestPolicy)]
    public async Task<IActionResult> Submit(int id)
    {
        await requests.SubmitAsync(CurrentUserId, id);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = AppRoles.CanCreateRequestPolicy)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelInput input)
    {
        await cancellations.CancelAsync(CurrentUserId, id, input.Reason);
        return NoContent();
    }

    [HttpPost("{id:int}/extension")]
    [Authorize(Policy = AppRoles.CanCreateRequestPolicy)]
    public async Task<IActionResult> RequestExtension(int id, [FromBody] ExtensionInput input)
    {
        await extensions.RequestExtensionAsync(CurrentUserId, id, input);
        return NoContent();
    }

    [HttpPost("{id:int}/book")]
    [Authorize(Policy = AppRoles.TravelAdminPolicy)]
    public async Task<IActionResult> Book(int id, [FromBody] BookingInput input)
    {
        await bookings.BookAsync(CurrentUserId, id, input);
        return NoContent();
    }
}
