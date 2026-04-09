namespace FrontendBlazor.Services;

public class TokenState
{
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
}
