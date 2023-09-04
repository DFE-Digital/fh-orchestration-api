using FamilyHubs.Idam.Data.Entities;
using FamilyHubs.ReferralService.Shared.Dto;

namespace FamilyHubs.Orchestration.Core.Models;

public record ReferralAllDto
{
    public required ReferralDto ReferralDto { get; set; }
    public required ServiceDirectory.Shared.Dto.ServiceDto ServiceDto { get; set; }

    public required Account Account { get; set; }
}
