 namespace FamilyHubs.Idam.Data.Entities;

public class Account : EntityBase<long>
{
    public string? OpenId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public required AccountStatus Status { get; set; }
    public ICollection<AccountClaim> Claims { get; set; } = new List<AccountClaim>();
}