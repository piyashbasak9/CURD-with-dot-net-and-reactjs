
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _authService.RegisterAsync(dto, cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message = "Account created. Please verify your email."
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _authService.VerifyEmailAsync(
                dto,
                cancellationToken);

            return Ok(new
            {
                message = "Email verified successfully. You can now log in."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
