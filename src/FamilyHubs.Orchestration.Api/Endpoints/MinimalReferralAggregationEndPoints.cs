using FamilyHubs.Orchestration.Core.Models;
using FamilyHubs.Orchestration.Core.Queries.GetReferralsAndServices;
using FamilyHubs.SharedKernel.Identity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace FamilyHubs.Orchestration.Api.Endpoints;

public class MinimalReferralAggregationEndPoints
{
    public void RegisterReferralAggregationEndPoints(WebApplication app)
    {
        app.MapGet("api/fullreferral/{id}", /*[Authorize(Roles = RoleGroups.LaProfessionalOrDualRole+","+RoleGroups.VcsProfessionalOrDualRole)]*/ async (long id, CancellationToken cancellationToken, ISender _mediator, HttpContext httpContext) =>
        {
            (long accountId, string role, long organisationId) = GetUserDetailsFromClaims(httpContext);

            GetReferralByIdWithServiceCommand request = new(id);
            var result = await _mediator.Send(request, cancellationToken);

            //If this is a VCS User make sure they can only see their own organisation details
            // VcsManagers will be blocked at the endpoint, but the check still makes sense here
            if (role is RoleTypes.VcsManager or RoleTypes.VcsProfessional or RoleTypes.VcsDualRole
                && result.ServiceDto.OrganisationId != organisationId)
            {
                return await SetForbidden<ReferralAllDto>(httpContext);
            }

            if (role is RoleTypes.LaManager or RoleTypes.LaProfessional or RoleTypes.LaDualRole
                && accountId != result.Account.Id)
            {
                return await SetForbidden<ReferralAllDto>(httpContext);
            }

            return result;

        }).WithMetadata(new SwaggerOperationAttribute("Get Referrals", "Get Referral By Id") { Tags = new[] { "Referrals" } });
    }

    private async Task<T> SetForbidden<T>(HttpContext httpContext)
    {
        var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());
        var statusCodeResult = new StatusCodeResult(StatusCodes.Status403Forbidden);
        await statusCodeResult.ExecuteResultAsync(actionContext);
        return default!;
    }

    private (long accountId, string role, long organisationId) GetUserDetailsFromClaims(HttpContext httpContext)
    {
        long accountId = -1;
        var accountIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == FamilyHubsClaimTypes.AccountId);
        if (accountIdClaim != null)
        {
            long.TryParse(accountIdClaim.Value, out accountId);
        }

        long organisationId = -1;
        var organisationIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == FamilyHubsClaimTypes.OrganisationId);
        if (organisationIdClaim != null)
        {
            long.TryParse(organisationIdClaim.Value, out organisationId);
        }

        string role = string.Empty;
        var roleClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        if (roleClaim != null)
        {
            role = roleClaim.Value;
        }

        return (accountId, role, organisationId);
    }
}

