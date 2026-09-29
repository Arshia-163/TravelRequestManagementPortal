
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.Constants;
using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Controllers;

[ApiController]
[Authorize(Policy = AppRoles.ManagerPolicy)]
[Route("api/manager")]
public sealed class ManagerApprovalsController : TravelApiControllerBase
{
    private readonly ITravelApprovalService approvals;

    public ManagerApprovalsController(
        ITravelApprovalService approvals)
    {
        this.approvals = approvals;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        return Ok(
            await approvals.GetPendingManagerAsync(
                CurrentUserId));
    }

    [HttpGet("approved")]
    public async Task<IActionResult> Approved()
    {
        return Ok(
            await approvals.GetManagerHistoryAsync(
                CurrentUserId,
                true));
    }

    [HttpGet("rejected")]
    public async Task<IActionResult> Rejected()
    {
        return Ok(
            await approvals.GetManagerHistoryAsync(
                CurrentUserId,
                false));
    }

    [HttpGet("all")]
    public async Task<IActionResult> All(
        [FromQuery] ApprovalRequestQuery query)
    {
        return Ok(
            await approvals.GetAllManagerRequestsAsync(
                CurrentUserId,
                query));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await approvals.GetForManagerViewAsync(
            CurrentUserId,
            id);

        return dto is null
            ? NotFound()
            : Ok(dto);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(
        int id,
        [FromBody] DecisionInput input)
    {
        await approvals.ApproveAsManagerAsync(
            CurrentUserId,
            id,
            input.Reason);

        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(
        int id,
        [FromBody] DecisionInput input)
    {
        await approvals.RejectAsManagerAsync(
            CurrentUserId,
            id,
            input.Reason ?? string.Empty);

        return NoContent();
    }
}
