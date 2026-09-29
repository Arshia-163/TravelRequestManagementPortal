
using Microsoft.AspNetCore.Identity;
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Implementations;

public sealed class TravelRequestService : ITravelRequestService
{
    private readonly ITravelRequestRepository requests;
    private readonly UserManager<ApplicationUser> users;
    private readonly IAuditLogService auditLog;

    public TravelRequestService(
        ITravelRequestRepository requests,
        UserManager<ApplicationUser> users,
        IAuditLogService auditLog)
    {
        this.requests = requests;
        this.users = users;
        this.auditLog = auditLog;
    }

    public async Task<TravelRequestDto> CreateDraftAsync(
        int userId,
        TravelRequestInput input)
    {
        var user = await FindUserAsync(userId);

       
        if (user.DepartmentId is null)
        {
            throw new InvalidOperationException(
                "No department is assigned to your account.");
        }

        var existing = await requests.GetForUserAsync(userId);

        RequestWorkflowRules.EnsureNoOverlappingActiveRequest(
            existing,
            input.StartDate,
            input.EndDate);

       
        var item = new TravelRequest
        {
            UserId = userId,
            DepartmentId = user.DepartmentId.Value,
            TravelType = input.TravelType,
            Source = (input.Source ?? string.Empty).Trim(),
            Destination = (input.Destination ?? string.Empty).Trim(),
            BusinessJustification = (input.BusinessJustification ?? string.Empty).Trim(),
            StartDate = input.StartDate.Date,
            EndDate = input.EndDate.Date,
            EstimatedCost = input.EstimatedCost,
            Currency = (input.Currency ?? string.Empty).Trim().ToUpperInvariant(),
            AccommodationRequired = input.AccommodationRequired,
            HotelName = input.AccommodationRequired
                ? (input.HotelName ?? string.Empty).Trim()
                : null,
            HotelCity = input.AccommodationRequired
                ? (input.HotelCity ?? string.Empty).Trim()
                : null,
            CheckInDate = input.AccommodationRequired
                ? input.CheckInDate
                : null,
            CheckOutDate = input.AccommodationRequired
                ? input.CheckOutDate
                : null
        };

        auditLog.Record(
            item,
            userId,
            AuditActionType.Created,
            "Draft created");

        await requests.AddAsync(item);
        await requests.SaveAsync();

      
        var saved = await requests.GetAsync(item.Id)
            ?? throw new InvalidOperationException(
                "The draft was saved but could not be reloaded.");

        return TravelRequestMapper.ToDto(saved);
    }

    public async Task<TravelRequestDto> UpdateDraftAsync(
        int userId,
        int requestId,
        TravelRequestInput input)
    {
        var item = await RequestWorkflowRules.GetOwnedAsync(
            requests,
            requestId,
            userId);

        if (item.Status != TravelRequestStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft requests can be edited.");
        }

        var user = await FindUserAsync(userId);

        if (user.DepartmentId is null ||
            user.DepartmentId != item.DepartmentId)
        {
            throw new InvalidOperationException(
                "Your department is not valid for this request.");
        }

   
        var existing = await requests.GetForUserAsync(userId);

        RequestWorkflowRules.EnsureNoOverlappingActiveRequest(
            existing,
            input.StartDate,
            input.EndDate,
            excludeRequestId: item.Id);

        item.TravelType = input.TravelType;
        item.Source = (input.Source ?? string.Empty).Trim();
        item.Destination = (input.Destination ?? string.Empty).Trim();
        item.BusinessJustification = (input.BusinessJustification ?? string.Empty).Trim();
        item.StartDate = input.StartDate.Date;
        item.EndDate = input.EndDate.Date;
        item.EstimatedCost = input.EstimatedCost;
        item.Currency = (input.Currency ?? string.Empty).Trim().ToUpperInvariant();
        item.AccommodationRequired = input.AccommodationRequired;
        item.HotelName = input.AccommodationRequired
            ? (input.HotelName ?? string.Empty).Trim()
            : null;
        item.HotelCity = input.AccommodationRequired
            ? (input.HotelCity ?? string.Empty).Trim()
            : null;
        item.CheckInDate = input.AccommodationRequired
            ? input.CheckInDate
            : null;
        item.CheckOutDate = input.AccommodationRequired
            ? input.CheckOutDate
            : null;
        item.UpdatedAt = DateTime.UtcNow;

        auditLog.Record(
            item,
            userId,
            AuditActionType.DraftUpdated,
            "Draft updated");

        await requests.SaveAsync();

        return TravelRequestMapper.ToDto(item);
    }

    public async Task<TravelRequestDto> SubmitAsync(
        int userId,
        int requestId)
    {
        var item = await RequestWorkflowRules.GetOwnedAsync(
            requests,
            requestId,
            userId);

        if (item.Status != TravelRequestStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft requests can be submitted.");
        }

        RequestWorkflowRules.ValidateForSubmission(
            new TravelRequestInput
            {
                TravelType = item.TravelType,
                Source = item.Source,
                Destination = item.Destination,
                BusinessJustification = item.BusinessJustification,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                EstimatedCost = item.EstimatedCost,
                Currency = item.Currency,
                AccommodationRequired = item.AccommodationRequired,
                HotelName = item.HotelName,
                HotelCity = item.HotelCity,
                CheckInDate = item.CheckInDate,
                CheckOutDate = item.CheckOutDate
            });

        var user = await FindUserAsync(userId);

        if (user.DepartmentId is null ||
            user.DepartmentId != item.DepartmentId)
        {
            throw new InvalidOperationException(
                "Your department is not valid for this request.");
        }

     
        if (item.Department.ManagerId == userId)
        {
            if (item.Department.DepartmentHeadId is null ||
                item.Department.DepartmentHeadId == userId)
            {
                throw new InvalidOperationException(
                    "A separate Department Head must be assigned before this request can proceed.");
            }

            item.Status = TravelRequestStatus.PendingDeptHead;
        }
        else
        {
            if (item.Department.ManagerId is null)
            {
                throw new InvalidOperationException(
                    "A Manager must be assigned before this request can be submitted.");
            }

            item.Status = TravelRequestStatus.PendingManager;
        }

        item.UpdatedAt = DateTime.UtcNow;

        auditLog.Record(
            item,
            userId,
            AuditActionType.Submitted);

        await requests.SaveAsync();

        return TravelRequestMapper.ToDto(item);
    }

    public async Task<IReadOnlyList<TravelRequestDto>> GetMyAsync(
        int userId)
    {
        var items = await requests.GetForUserAsync(userId);

        await RequestWorkflowRules.CompleteElapsedAsync(
            requests,
            items);

        return items
            .Select(TravelRequestMapper.ToDto)
            .ToList();
    }

    public async Task<IReadOnlyList<TravelRequestDto>> GetAllAsync()
    {
        var items = await requests.GetAllAsync();

        await RequestWorkflowRules.CompleteElapsedAsync(
            requests,
            items);

        return items
            .Select(TravelRequestMapper.ToDto)
            .ToList();
    }

    public async Task<TravelRequestDto?> GetAsync(
        int id,
        int userId,
        bool canViewAll = false)
    {
        var item = await requests.GetAsync(id);

        if (item is null)
        {
            return null;
        }

        var isOwner = item.UserId == userId;

        return !canViewAll && !isOwner
            ? null
            : TravelRequestMapper.ToDto(item);
    }

    private async Task<ApplicationUser> FindUserAsync(int userId)
    {
        return await users.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException(
                "User was not found.");
    }
}

