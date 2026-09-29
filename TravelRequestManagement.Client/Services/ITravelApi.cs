using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelRequestManagement.Client.Services;


public interface ITravelApi
{
    
    Task<CurrentUserDto> MeAsync();

   
    Task<IReadOnlyList<TravelRequestDto>> MyRequestsAsync();
    Task<TravelRequestDto> GetRequestAsync(int id);
    Task<TravelRequestDto> CreateAsync(TravelRequestInput input);
    Task<TravelRequestDto> UpdateDraftAsync(int id, TravelRequestInput input);
    Task SubmitAsync(int id);
    Task CancelAsync(int id, string reason);
    Task RequestExtensionAsync(int id, ExtensionInput input);
    Task BookAsync(int id, BookingInput input);

    // -- manager approvals --
    Task<IReadOnlyList<TravelRequestDto>> ManagerPendingAsync();
    Task<IReadOnlyList<TravelRequestDto>> ManagerHistoryAsync(bool approved);
    Task<ApprovalRequestPage> ManagerAllRequestsAsync(ApprovalRequestQuery query);
    Task<TravelRequestDto> ManagerRequestDetailsAsync(int id);
    Task DecideManagerAsync(int id, bool approve, string? reason);

    // -- department head approvals --
    Task<IReadOnlyList<TravelRequestDto>> HeadPendingAsync();
    Task<IReadOnlyList<TravelRequestDto>> HeadHistoryAsync(bool approved);
    Task<ApprovalRequestPage> HeadAllRequestsAsync(ApprovalRequestQuery query);
    Task<TravelRequestDto> DeptHeadRequestDetailsAsync(int id);
    Task DecideHeadAsync(int id, bool approve, string? reason);

    // -- admin: dashboard, users, departments --
    Task<DashboardDto> AdminDashboardAsync();
    Task<IReadOnlyList<UserDto>> UsersAsync();
    Task<UserDto> CreateUserAsync(UserInput input);
    Task<UserDto> UpdateUserAsync(int id, UserInput input);
    Task<UserDto> ActivateUserAsync(int id);
    Task<UserDto> DeactivateUserAsync(int id);
    Task DeleteUserAsync(int id);
    Task<IReadOnlyList<DepartmentDto>> DepartmentsAsync();
    Task<DepartmentDto> CreateDepartmentAsync(DepartmentInput input);
    Task<DepartmentDto> UpdateDepartmentAsync(int id, DepartmentInput input);
    Task DeleteDepartmentAsync(int id);
    Task<TravelRequestDto> AdminRequestHistoryAsync(int id);
    Task<IReadOnlyList<TravelRequestDto>> AdminAllRequestsAsync();

    // -- admin: bookings --
    Task<IReadOnlyList<TravelRequestDto>> ApprovedForBookingAsync();
    Task<IReadOnlyList<AdminBookingDto>> AllBookingsAsync();
    Task<IReadOnlyList<BookingDto>> GetBookingsAsync(int requestId);
    Task<BookingDto> AddBookingAsync(int requestId, BookingCreateInput input);
    Task<BookingDto> UpdateBookingStatusAsync(int requestId, int bookingId, BookingStatus status);
}
