using FamilyHubs.Idam.Data.Entities;
using FamilyHubs.ReferralService.Shared.Dto;
using System.Text.Json;

namespace FamilyHubs.Orchestration.Core.ClientServices;

public interface IClientService
{
    Task<ReferralDto?> GetReferralById(long id);
    Task<ServiceDirectory.Shared.Dto.ServiceDto?> GetServiceById(long id);
    Task<Account?> GetAccountByEmail(string email);


}

public class ClientService : IClientService
{
    private readonly HttpClient _httpClient;

    public ClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<ServiceDirectory.Shared.Dto.ServiceDto?> GetServiceById(long id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_httpClient.BaseAddress + $"api/services/{id}")
        };

        using var response = await _httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentException("Invalid response from ServiceDirectory Api");
        }

        return JsonSerializer.Deserialize<ServiceDirectory.Shared.Dto.ServiceDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    }

    public async Task<ReferralDto?> GetReferralById(long id)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_httpClient.BaseAddress + $"api/referral/{id}")
        };

        using var response = await _httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentException("Invalid response from Referral Api");
        }

        return JsonSerializer.Deserialize<ReferralDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    }

    public async Task<Account?> GetAccountByEmail(string email)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_httpClient.BaseAddress + $"api/account/{email}")
        };

        using var response = await _httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentException("Invalid response from Idams Api");
        }

        return JsonSerializer.Deserialize<Account>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    }
}
