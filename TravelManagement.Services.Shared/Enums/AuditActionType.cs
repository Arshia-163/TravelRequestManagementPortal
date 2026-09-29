namespace TravelManagement.Services.Shared.Enums;

public enum AuditActionType
{
    Created, DraftUpdated, Submitted, ManagerApproved, ManagerRejected,
    DepartmentHeadApproved, DepartmentHeadRejected, ExtensionRequested,
    Cancelled, Booked, Completed, BookingStatusChanged
}
