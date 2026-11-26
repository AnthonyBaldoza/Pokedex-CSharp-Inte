using Microsoft.AspNetCore.Identity;
using Pokedex.DTOs;

namespace Pokedex.Services;

public record AuthResult(bool Succeeded, UserResponseDto? User = null, string? Error = null, bool LockedOut = false);

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResult> LoginAsync(LoginRequestDto dto);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;

    public AuthService(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequestDto dto)
    {
        var user = new IdentityUser
        {
            UserName = dto.Username.Trim(),
            Email = dto.Email.Trim()
        };

        // Identity hashes the password (PBKDF2) and enforces the password rules from Program.cs.
        var result = await _users.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return new AuthResult(false, Error: string.Join(" ", result.Errors.Select(e => e.Description)));

        await _signIn.SignInAsync(user, isPersistent: true);   // sets the HttpOnly auth cookie
        return new AuthResult(true, ToDto(user));
    }

    public async Task<AuthResult> LoginAsync(LoginRequestDto dto)
    {
        const string invalid = "Invalid username/email or password.";

        var id = dto.UsernameOrEmail.Trim();
        var user = id.Contains('@') ? await _users.FindByEmailAsync(id) : await _users.FindByNameAsync(id);
        if (user is null) return new AuthResult(false, Error: invalid);

        var result = await _signIn.PasswordSignInAsync(user, dto.Password, dto.RememberMe, lockoutOnFailure: true);
        if (result.IsLockedOut) return new AuthResult(false, Error: "Too many failed attempts. Try again in a few minutes.", LockedOut: true);
        if (!result.Succeeded) return new AuthResult(false, Error: invalid);

        return new AuthResult(true, ToDto(user));
    }

    public Task LogoutAsync() => _signIn.SignOutAsync();

    private static UserResponseDto ToDto(IdentityUser u) => new()
    {
        Username = u.UserName ?? string.Empty,
        Email = u.Email ?? string.Empty
    };
}