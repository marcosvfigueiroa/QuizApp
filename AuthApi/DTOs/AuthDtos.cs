namespace AuthApi.DTOs;

public class RegisterRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class ProfileDto
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class UpdateProfileRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
