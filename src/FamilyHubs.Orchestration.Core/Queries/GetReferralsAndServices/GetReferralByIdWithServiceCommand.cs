using Ardalis.GuardClauses;
using FamilyHubs.Orchestration.Core.ClientServices;
using FamilyHubs.Orchestration.Core.Models;
using FamilyHubs.ReferralService.Shared.Dto;
using MediatR;

namespace FamilyHubs.Orchestration.Core.Queries.GetReferralsAndServices;

public class GetReferralByIdWithServiceCommand : IRequest<ReferralAllDto>
{
    public GetReferralByIdWithServiceCommand(long id)
    {
        Id = id;
    }

    public long Id { get; set; }

}

public class GetReferralByIdWithServiceCommandHandler : IRequestHandler<GetReferralByIdWithServiceCommand, ReferralAllDto>
{
    private readonly IClientService _clientService;
    public GetReferralByIdWithServiceCommandHandler(IClientService clientService)
    {
        _clientService = clientService;
    }
    public async Task<ReferralAllDto> Handle(GetReferralByIdWithServiceCommand request, CancellationToken cancellationToken)
    {
        var referral = await _clientService.GetReferralById(request.Id);

        if (referral == null)
        {
            throw new NotFoundException(nameof(ReferralDto), request.Id.ToString());
        }

        var serviceCall = _clientService.GetServiceById(referral.ReferralServiceDto.Id);
        var accountCall = _clientService.GetAccountById(referral.ReferralUserAccountDto.Id);

        await Task.WhenAll(serviceCall, accountCall);

        return new ReferralAllDto
        {
            ReferralDto = referral,
            Account = accountCall.Result!,
            ServiceDto = serviceCall.Result!
        };
    }
}
