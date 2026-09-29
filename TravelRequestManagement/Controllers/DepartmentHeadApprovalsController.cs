
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.Constants;
using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Controllers;

[ApiController]
[Authorize(Policy = AppRoles.DepartmentHeadPolicy)]
[Route("api/department-head")]
public sealed class DepartmentHeadApprovalsController : TravelApiControllerBase
{
    private readonly ITravelApprovalService approvals;

    public DepartmentHeadApprovalsController(
        ITravelApprovalService approvals)
    {
        this.approvals = approvals;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        return Ok(
            await approvals.GetPendingDepartmentHeadAsync(
                CurrentUserId));
    }

    [HttpGet("approved")]
    public async Task<IActionResult> Approved()
    {
        return Ok(
            await approvals.GetDepartmentHeadHistoryAsync(
                CurrentUserId,
                true));
    }

    [HttpGet("rejected")]
    public async Task<IActionResult> Rejected()
    {
        return Ok(
            await approvals.GetDepartmentHeadHistoryAsync(
                CurrentUserId,
                false));
    }

    [HttpGet("all")]
    public async Task<IActionResult> All(
        [FromQuery] ApprovalRequestQuery query)
    {
        return Ok(
            await approvals.GetAllDepartmentHeadRequestsAsync(
                CurrentUserId,
                query));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await approvals.GetForDepartmentHeadViewAsync(
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
        await approvals.ApproveAsDepartmentHeadAsync(
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
        await approvals.RejectAsDepartmentHeadAsync(
            CurrentUserId,
            id,
            input.Reason ?? string.Empty);

        return NoContent();
    }
}
