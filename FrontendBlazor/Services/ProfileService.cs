using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FrontendBlazor.Services;

public class ProfileService
{
    private readonly IHttpClientFactory _factory;
    private readonly TokenState _state;

    public ProfileService(IHttpClientFactory factory, TokenState state)
    {
        _factory = factory;
        _state = state;
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient("GatewayClient");

        if (!string.IsNullOrEmpty(_state.Token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _state.Token);
        }
        else
        {
            client.DefaultRequestHeaders.Authorization = null;
        }

        return client;
    }

    // DTOs côté frontend
    public class ProfileDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    // GET /auth/profile  (passe par l'API Gateway)
    public async Task<(bool ok, ProfileDto? profile, string? error)> GetProfileAsync()
    {
        var client = CreateClient();
        var response = await client.GetAsync("auth/profile");

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return (false, null, err);
        }

        var dto = await response.Content.ReadFromJsonAsync<ProfileDto>();
        return (true, dto, null);
    }

    // PUT /auth/profile
    public async Task<(bool ok, string? error)> UpdateProfileAsync(
        string fullName,
        string email,
        string? currentPassword,
        string? newPassword)
    {
        var client = CreateClient();
        var body = new UpdateProfileRequest
        {
            FullName = fullName,
            Email = email,
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };

        var response = await client.PutAsJsonAsync("auth/profile", body);

        if (response.IsSuccessStatusCode)
            return (true, null);

        var err = await response.Content.ReadAsStringAsync();
        return (false, err);
    }
}
