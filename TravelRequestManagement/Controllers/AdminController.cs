
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.Constants;
using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Controllers;

[ApiController]
[Authorize(Policy = AppRoles.TravelAdminPolicy)]
[Route("api/admin")]
public sealed class AdminController : TravelApiControllerBase
{
    private readonly IAdminService administration;
    private readonly IUserService users;
    private readonly IBookingService bookings;

    public AdminController(
        IAdminService administration,
        IUserService users,
        IBookingService bookings)
    {
        this.administration = administration;
        this.users = users;
        this.bookings = bookings;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        return Ok(
            await administration.GetDashboardAsync());
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        return Ok(
            await users.GetUsersAsync());
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        [FromBody] UserInput input)
    {
        return Ok(
            await users.CreateUserAsync(input));
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] UserInput input)
    {
        return Ok(
            await users.UpdateUserAsync(id, input));
    }

    [HttpPost("users/{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        return Ok(
            await users.SetActiveAsync(
                CurrentUserId,
                id,
                true));
    }

    [HttpPost("users/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        return Ok(
            await users.SetActiveAsync(
                CurrentUserId,
                id,
                false));
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            await users.DeleteUserAsync(
                CurrentUserId,
                id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
         
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("departments")]
    public async Task<IActionResult> Departments()
    {
        return Ok(
            await administration.GetDepartmentsAsync());
    }

    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment(
        [FromBody] DepartmentInput input)
    {
        return Ok(
            await administration.CreateDepartmentAsync(input));
    }

    [HttpPut("departments/{id:int}")]
    public async Task<IActionResult> UpdateDepartment(
        int id,
        [FromBody] DepartmentInput input)
    {
        return Ok(
            await administration.UpdateDepartmentAsync(
                id,
                input));
    }

    [HttpDelete("departments/{id:int}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        await administration.DeleteDepartmentAsync(id);

        return NoContent();
    }

    [HttpGet("approved-requests")]
    public async Task<IActionResult> ApprovedRequests()
    {
        return Ok(
            await bookings.GetApprovedForBookingAsync());
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> AllBookings()
    {
        return Ok(
            await bookings.GetAllBookingsAsync());
    }

    [HttpGet("requests")]
    public async Task<IActionResult> AllRequests(
        [FromServices] ITravelRequestService requests)
    {
        return Ok(
            await requests.GetAllAsync());
    }

    [HttpGet("requests/{id:int}")]
    public async Task<IActionResult> RequestHistory(
        int id,
        [FromServices] ITravelRequestService requests)
    {
        var request = await requests.GetAsync(
            id,
            CurrentUserId,
            canViewAll: true);

        return request is null
            ? NotFound(
                new
                {
                    message = "No request was found with that request number."
                })
            : Ok(request);
    }

    [HttpGet("requests/{id:int}/bookings")]
    public async Task<IActionResult> GetBookings(int id)
    {
        return Ok(
            await bookings.GetBookingsForRequestAsync(id));
    }

    [HttpPost("requests/{id:int}/bookings")]
    public async Task<IActionResult> AddBooking(
        int id,
        [FromBody] BookingCreateInput input)
    {
        return Ok(
            await bookings.AddBookingAsync(
                CurrentUserId,
                id,
                input));
    }

    [HttpPut("requests/{id:int}/bookings/{bookingId:int}/status")]
    public async Task<IActionResult> UpdateBookingStatus(
        int id,
        int bookingId,
        [FromBody] BookingStatusUpdateInput input)
    {
        return Ok(
            await bookings.UpdateBookingStatusAsync(
                CurrentUserId,
                id,
                bookingId,
                input.Status));
    }
}
