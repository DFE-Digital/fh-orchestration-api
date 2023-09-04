using Ardalis.GuardClauses;
using Azure;
using FamilyHubs.Orchestration.Core.ClientServices;
using FamilyHubs.Orchestration.Core.Queries.GetReferralsAndServices;
using FamilyHubs.ReferralService.Shared.Dto;
using FluentAssertions;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FamilyHubs.Orchestration.Api.UnitTests;

public class WhenUsingClientServices : BaseClientService
{
    [Fact]
    public async Task ThenGetServiceById()
    {
        //Arrange
        var service = WhenGettingReferralAggregates.GetTestCountyCouncilServicesDto(1);
        var json = JsonConvert.SerializeObject(service);
        var mockClient = GetMockClient(json);
        var clientService = new ClientService(mockClient);

        //Act
        var result = await clientService.GetServiceById(service.Id);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(service);
    }

    [Fact]
    public async Task ThenGetServiceById_ReturnsArgumentException()
    {
        //Arrange
        var mockClient = GetMockClient(default!);
        var clientService = new ClientService(mockClient);

        //Act
        Func<Task> act = async () => await clientService.GetServiceById(-1);


        //Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid response from ServiceDirectory Api");
    }

    [Fact]
    public async Task ThenGetReferralById()
    {
        //Arrange
        ReferralDto referral = WhenGettingReferralAggregates.GetReferralDto();
        var json = JsonConvert.SerializeObject(referral);
        var mockClient = GetMockClient(json);
        var clientService = new ClientService(mockClient);

        //Act
        var result = await clientService.GetReferralById(referral.Id);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(referral);
    }

    [Fact]
    public async Task ThenGetReferralById_ReturnsArgumentException()
    {
        //Arrange
        var mockClient = GetMockClient(default!);
        var clientService = new ClientService(mockClient);

        //Act
        Func<Task> act = async () => await clientService.GetReferralById(-1);


        //Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid response from Referral Api");
    }

    [Fact]
    public async Task ThenGetAccountById()
    {
        //Arrange
        ReferralDto referral = WhenGettingReferralAggregates.GetReferralDto();
        var account = new Idam.Data.Entities.Account
        {
            Id = 1,
            Email = referral.ReferralUserAccountDto.EmailAddress,
            Name = referral.ReferralUserAccountDto.Name ?? "Joe Bloggs",
            Status = Idam.Data.Entities.AccountStatus.Active
        };
        
        var json = JsonConvert.SerializeObject(account);
        var mockClient = GetMockClient(json);
        var clientService = new ClientService(mockClient);

        //Act
        var result = await clientService.GetAccountById(1);

        //Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account);
    }

    [Fact]
    public async Task ThenGetAccountById_ReturnsArgumentException()
    {
        //Arrange
        var mockClient = GetMockClient(default!);
        var clientService = new ClientService(mockClient);

        //Act
        Func<Task> act = async () => await clientService.GetAccountById(-1);


        //Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid response from Idams Api");
    }
}
