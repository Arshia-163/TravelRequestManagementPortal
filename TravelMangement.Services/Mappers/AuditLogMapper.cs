using Riok.Mapperly.Abstractions;
using TravelManagement.Data.Entities;
using TravelManagement.Services.Shared.DTOs;

namespace TravelMangement.Services.Mappers;
[Mapper]
internal static partial class AuditLogMapper
{
    [MapProperty(nameof(AuditLog.PerformedByUser), nameof(AuditLogDto.PerformedBy), Use = nameof(GetFullName))]
    public static partial AuditLogDto ToDto(AuditLog source);

    private static string GetFullName(ApplicationUser user) => $"{user.FirstName} {user.LastName}";
}