namespace TravelManagement.Services.Shared.DTOs;

public sealed record DepartmentDto(int Id, string Name, decimal TravelApprovalLimit, int? ManagerId, int? DepartmentHeadId);
