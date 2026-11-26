using System.ComponentModel.DataAnnotations;

namespace Pokedex.DTOs;

public class RegisterRequestDto
{
    [Required, StringLength(30, MinimumLength = 3)]
    // letters, numbers, _ . -  and single spaces between words (e.g. "Anthony Baldoza")
    [RegularExpression(@"^[A-Za-z0-9_.-]+(?: [A-Za-z0-9_.-]+)*$", ErrorMessage = "Username can only contain letters, numbers, single spaces, and _ . -")]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    [Required, StringLength(100)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = true;
}

public class UserResponseDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}