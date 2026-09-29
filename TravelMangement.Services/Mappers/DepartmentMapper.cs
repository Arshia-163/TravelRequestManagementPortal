using Riok.Mapperly.Abstractions;

using TravelManagement.Data.Entities;
using TravelManagement.Services.Shared.DTOs;

namespace TravelMangement.Services.Mappers;

[Mapper]
internal static partial class DepartmentMapper
{
    public static partial DepartmentDto ToDto(Department source);
}