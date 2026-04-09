using System.Net.Http.Json;

namespace FrontendBlazor.Services;

public class AuthService
{
    private readonly IHttpClientFactory _factory;
    private readonly TokenState _state;

    public AuthService(IHttpClientFactory factory, TokenState state)
    {
        _factory = factory;
        _state = state;
    }

    public record RegisterRequest(string FullName, string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token, string FullName, string Email);

    private HttpClient CreateClient()
        => _factory.CreateClient("GatewayClient");

    public async Task<bool> RegisterAsync(string fullName, string email, string password)
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "auth/Auth/register", // passe par ApiGateway
            new RegisterRequest(fullName, email, password));

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (result is null) return false;

        _state.Token = result.Token;
        _state.Email = result.Email;
        _state.FullName = result.FullName;

        return true;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "auth/Auth/login",
            new LoginRequest(email, password));

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (result is null) return false;

        _state.Token = result.Token;
        _state.Email = result.Email;
        _state.FullName = result.FullName;

        return true;
    }

    public void Logout()
    {
        _state.Token = null;
        _state.Email = null;
        _state.FullName = null;
    }
}
