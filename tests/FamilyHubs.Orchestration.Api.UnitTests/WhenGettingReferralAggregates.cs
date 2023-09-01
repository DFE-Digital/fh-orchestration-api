using FamilyHubs.Orchestration.Core.ClientServices;
using FamilyHubs.Orchestration.Core.Queries.GetReferralsAndServices;
using FamilyHubs.ReferralService.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FluentAssertions;
using Moq;

namespace FamilyHubs.Orchestration.Api.UnitTests
{
    public class WhenGettingReferralAggregates
    {

        private readonly Mock<IClientService> _mockClientService;
        private readonly ReferralDto _referral;

        public WhenGettingReferralAggregates() 
        {
            _mockClientService = new Mock<IClientService>();
            _referral = GetReferralDto();
            _mockClientService.Setup(x => x.GetServiceById(It.IsAny<long>())).ReturnsAsync(GetTestCountyCouncilServicesDto(1));
            _mockClientService.Setup(x => x.GetAccountByEmail(It.IsAny<string>())).ReturnsAsync(new Idam.Data.Entities.Account
            {
                Id = 1,
                Email = _referral.ReferralUserAccountDto.EmailAddress,
                Name = _referral.ReferralUserAccountDto.Name ?? "Joe Bloggs",
                Status = Idam.Data.Entities.AccountStatus.Active
            });
        }


        [Fact]
        public async Task ThenGettingReferral_ReturnsReferralAllRecord()
        {
            //Arrange
            _mockClientService.Setup(x => x.GetReferralById(It.IsAny<long>())).ReturnsAsync(GetReferralDto());

            GetReferralByIdWithServiceCommand command = new(1);
            GetReferralByIdWithServiceCommandHandler handler = new GetReferralByIdWithServiceCommandHandler(_mockClientService.Object);

            //Act
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            result.Should().NotBeNull();
            result.ReferralDto.Should().BeEquivalentTo(GetReferralDto());
            result.ServiceDto.Should().BeEquivalentTo(GetTestCountyCouncilServicesDto(1));
            result.Account.Email.Should().Be(_referral.ReferralUserAccountDto.EmailAddress);
        }

        public static ReferralDto GetReferralDto()
        {
            return new ReferralDto
            {
                Id = 1,
                ReasonForSupport = "Reason For Support",
                EngageWithFamily = "Engage With Family",
                RecipientDto = new RecipientDto
                {
                    Id = 1,
                    Name = "Joe Blogs",
                    Email = "JoeBlog@email.com",
                    Telephone = "078123456",
                    TextPhone = "078123456",
                    AddressLine1 = "Address Line 1",
                    AddressLine2 = "Address Line 2",
                    TownOrCity = "Town or City",
                    County = "County",
                    PostCode = "B30 2TV"
                },
                ReferralUserAccountDto = new UserAccountDto
                {
                    Id = 1,
                    EmailAddress = "Bob.Referrer@email.com",
                    Name = "Bob Referrer",
                    PhoneNumber = "1234567890",
                    Team = "Team",
                    UserAccountRoles = new List<UserAccountRoleDto>()
                    {
                        new UserAccountRoleDto
                        {
                            UserAccount = new UserAccountDto
                            {
                                EmailAddress = "Bob.Referrer@email.com",
                            },
                            Role = new RoleDto
                            {
                                Name = "VcsProfessional"
                            }
                        }
                    },
                    ServiceUserAccounts = new List<UserAccountServiceDto>(),
                    OrganisationUserAccounts = new List<UserAccountOrganisationDto>(),
                },
                Status = new ReferralStatusDto
                {
                    Id = 1,
                    Name = "New",
                    SortOrder = 0
                },
                ReferralServiceDto = new ReferralServiceDto
                {
                    Id = 1,
                    Name = "Unit Test Service",
                    Description = "Unit Test Service Description",
                    Url = "www.service.com",
                    OrganisationDto = new ReferralService.Shared.Dto.OrganisationDto
                    {
                        Id = 1,
                        ReferralServiceId = 2,
                        Name = "Unit Test Organisation",
                        Description = "Unit Test Organisation Description",
                    }
                }

            };
        }

        public static ServiceDto GetTestCountyCouncilServicesDto(long organisationId, string serviceId = "5059a0b2-ad5d-4288-b7c1-e30d35345b0e")
        {
            var service = new ServiceDto
            {
                Id = 1,
                ServiceOwnerReferenceId = serviceId,
                OrganisationId = organisationId,
                ServiceType = ServiceType.InformationSharing,
                Status = ServiceStatusType.Active,
                Name = "Unit Test Service",
                Description = @"Unit Test Service Description",
                ServiceDeliveries = new List<ServiceDeliveryDto>
            {
                new ServiceDeliveryDto
                {
                    Name = ServiceDeliveryType.Online,
                }
            },
                Eligibilities = new List<EligibilityDto>
            {
                new EligibilityDto
                {
                    EligibilityType = EligibilityType.NotSet,
                    MinimumAge = 0,
                    MaximumAge = 13,
                }
            },
                Contacts = new List<ContactDto>
            {
                new ContactDto
                {
                    Name = "Contact",
                    Title = string.Empty,
                    Telephone = "01827 65777",
                    TextPhone = "01827 65777",
                    Url = "https://www.unittestservice.com",
                    Email = "support@unittestservice.com"
                }
            },
                CostOptions = new List<CostOptionDto>
            {
                new CostOptionDto
                {
                    Amount = 1,
                    Option = "paid",
                    AmountDescription = "£1 a session",
                }
            },
                Languages = new List<LanguageDto>
            {
                new LanguageDto
                {
                    Name = "English",
                }
            },
                ServiceAreas = new List<ServiceAreaDto>
            {
                new ServiceAreaDto
                {
                    ServiceAreaName = "National",
                    Extent = null,
                    Uri = "http://statistics.data.gov.uk/id/statistical-geography/K02000001",
                }
            },
                Locations = new List<LocationDto>
            {
                new LocationDto
                {
                    Name = "Test Location",
                    Description = "",
                    Latitude = 52.6312,
                    Longitude = -1.66526,
                    Address1 = "Some Lane",
                    City = ", Stathe, Tamworth, Staffordshire, ",
                    PostCode = "B77 3JN",
                    Country = "England",
                    StateProvince = "null",
                    LocationType = LocationType.FamilyHub,
                    Contacts = new List<ContactDto>
                    {
                        new ContactDto
                        {
                            Name = "Contact",
                            Title = string.Empty,
                            TextPhone = "01827 65777",
                            Telephone = "01827 65777",
                            Url = "https://www.unittestservice.com",
                            Email = "support@unittestservice.com"
                        }
                    },
                    RegularSchedules = new List<RegularScheduleDto>
                    {
                        new RegularScheduleDto
                        {
                            Description = "Description",
                            ValidFrom = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            ValidTo = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(8),
                            ByDay = "byDay",
                            ByMonthDay = "byMonth",
                            DtStart = "dtStart",
                            Freq = FrequencyType.NotSet,
                            Interval = "interval",
                            OpensAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            ClosesAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(6)
                        }
                    },
                    HolidaySchedules = new List<HolidayScheduleDto>
                    {
                        new HolidayScheduleDto
                        {
                            Closed = false,
                            ClosesAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            OpensAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            StartDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(5) ,
                            EndDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        }
                    }
                }
            },
                Taxonomies = new List<TaxonomyDto>
            {
                new TaxonomyDto
                {
                    Name = "Organisation",
                    TaxonomyType = TaxonomyType.ServiceCategory,
                    ParentId = null
                },
                new TaxonomyDto
                {
                    Name = "Support",
                    TaxonomyType = TaxonomyType.ServiceCategory,
                    ParentId = null
                },
                new TaxonomyDto
                {
                    Name = "Children",
                    TaxonomyType = TaxonomyType.ServiceCategory,
                    ParentId = null
                },
                new TaxonomyDto
                {
                    Name = "Long Term Health Conditions",
                    TaxonomyType = TaxonomyType.ServiceCategory,
                    ParentId = null
                }
            },
                RegularSchedules = new List<RegularScheduleDto>
            {
                new RegularScheduleDto
                {
                    Description = "Description",
                    OpensAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ClosesAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(8),
                    ByDay = "byDay1",
                    ByMonthDay = "byMonth",
                    DtStart = "dtStart",
                    Freq = FrequencyType.NotSet,
                    Interval = "interval",
                    ValidTo = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ValidFrom = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(6)
                }
            },
                HolidaySchedules = new List<HolidayScheduleDto>
            {
                new HolidayScheduleDto
                {
                    Closed = true,
                    ClosesAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    OpensAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    StartDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(5),
                    EndDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            }
            };

            return service;
        }
    }
}