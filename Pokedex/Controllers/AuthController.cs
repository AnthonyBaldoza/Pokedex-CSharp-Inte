using Microsoft.AspNetCore.Mvc;
using Pokedex.DTOs;
using Pokedex.Services;

namespace Pokedex.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        var r = await _auth.RegisterAsync(dto);
        return r.Succeeded ? Ok(r.User) : BadRequest(new { message = r.Error });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var r = await _auth.LoginAsync(dto);
        if (r.Succeeded) return Ok(r.User);
        return r.LockedOut
            ? StatusCode(StatusCodes.Status429TooManyRequests, new { message = r.Error })
            : Unauthorized(new { message = r.Error });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _auth.LogoutAsync();
        return NoContent();
    }

    // GET /api/auth/me  -> used by the page on load to know if someone is logged in
    [HttpGet("me")]
    public IActionResult Me()
    {
        var isAuth = User.Identity?.IsAuthenticated == true;
        return Ok(new { authenticated = isAuth, username = isAuth ? User.Identity!.Name : null });
    }
}