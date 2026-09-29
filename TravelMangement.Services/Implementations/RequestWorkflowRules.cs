
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Implementations;

internal static class RequestWorkflowRules
{
    public static async Task<TravelRequest> GetOwnedAsync(
        ITravelRequestRepository requests,
        int id,
        int userId)
    {
        var item = await requests.GetAsync(id)
            ?? throw new InvalidOperationException("Request was not found.");

        if (item.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You do not own this request.");
        }

        return item;
    }

    public static void ValidateForSubmission(TravelRequestInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Source) ||
            string.IsNullOrWhiteSpace(input.Destination) ||
            string.IsNullOrWhiteSpace(input.BusinessJustification) ||
            string.IsNullOrWhiteSpace(input.Currency) ||
            input.EstimatedCost <= 0 ||
            input.EndDate.Date < input.StartDate.Date)
        {
            throw new InvalidOperationException(
                "Complete all required travel details with valid dates and cost.");
        }

       
        if (input.AccommodationRequired)
        {
            if (string.IsNullOrWhiteSpace(input.HotelName) ||
                string.IsNullOrWhiteSpace(input.HotelCity) ||
                input.CheckInDate is null ||
                input.CheckOutDate is null)
            {
                throw new InvalidOperationException(
                    "Complete all accommodation details (hotel name, hotel city, check-in and check-out dates).");
            }

          
            if (input.CheckInDate.Value.Date < input.StartDate.Date ||
                input.CheckOutDate.Value.Date > input.EndDate.Date)
            {
                throw new InvalidOperationException(
                    "Check-in date must be on or after the start date, and check-out date must be on or before the end date.");
            }
        }
    }

    public static void RequireReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("A reason is required.");
        }
    }

    public static void EnsureNoOverlappingActiveRequest(
        IEnumerable<TravelRequest> existingRequests,
        DateTime newStart,
        DateTime newEnd,
        int? excludeRequestId = null)
    {
        var start = newStart.Date;
        var end = newEnd.Date;

        var conflict = existingRequests.Any(x =>
            x.Id != excludeRequestId &&
            x.Status is not (
                TravelRequestStatus.Rejected
                or TravelRequestStatus.Cancelled
                or TravelRequestStatus.Completed) &&
            x.StartDate.Date <= end &&
            x.EndDate.Date >= start);

        if (conflict)
        {
            throw new InvalidOperationException(
                "You already have a travel request in progress for these dates. Cancel or wait for it to be resolved before creating an overlapping request.");
        }
    }

    public static bool IsSelfApproval(
        TravelRequest request,
        int actingUserId)
    {
        return request.UserId == actingUserId;
    }

    public static TripExtension GetPendingExtension(
        TravelRequest request,
        ExtensionStatus status)
    {
        return request.TripExtensions
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => x.Status == status)
            ?? throw new InvalidOperationException(
                "No pending extension was found.");
    }

    public static void ApplyApprovedExtension(
        TravelRequest request,
        TripExtension extension)
    {
        extension.Status = ExtensionStatus.Approved;
        extension.UpdatedAt = DateTime.UtcNow;

        request.EndDate = extension.NewEndDate;
        request.EstimatedCost = extension.TotalEstimatedCost;
        request.Status = TravelRequestStatus.Approved;
        request.UpdatedAt = DateTime.UtcNow;
    }

    public static async Task CompleteElapsedAsync(
        ITravelRequestRepository requests,
        IReadOnlyList<TravelRequest> items)
    {
        var today = DateTime.UtcNow.Date;

        var elapsed = items
            .Where(x =>
                x.Status == TravelRequestStatus.Approved &&
                x.EndDate.Date < today)
            .ToList();

        if (elapsed.Count == 0)
        {
            return;
        }

        foreach (var item in elapsed)
        {
            item.Status = TravelRequestStatus.Completed;
            item.UpdatedAt = DateTime.UtcNow;

            item.AuditLogs.Add(new AuditLog
            {
                PerformedByUserId = item.UserId,
                Action = AuditActionType.Completed,
                Comment = "Travel end date has passed."
            });
        }

        await requests.SaveAsync();
    }
}
